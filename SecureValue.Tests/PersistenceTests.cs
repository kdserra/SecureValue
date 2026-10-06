using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SecureValue;
using Xunit;

namespace SecureValue.Tests
{
	/// <summary>
	/// Cross-session persistence at the storage layer: the FIRST run in a fresh
	/// state only writes the payload file (an initialization run cannot reload
	/// anything, so it passes). Every SUBSEQUENT run — a new OS process with fresh
	/// static keys, like a Unity restart — reloads the stored uint[] payloads and
	/// asserts them against the original constants. CI runs the suite once (init)
	/// and then re-runs just this test (validate). The file lives in the temp dir
	/// so clones stay clean; deleting it forces a fresh initialization run.
	/// A version marker re-initializes (instead of failing) when the storage
	/// format itself changes; anything else wrong with a versioned file fails.
	/// </summary>
	public class PersistenceTests
	{
		private const int PayloadVersion = 1;

		private static string PayloadPath =>
			Path.Combine(Path.GetTempPath(), "SecureValue.CrossSessionPersistence.json");

		[Fact]
		public void CrossSession_StoredPayloadsReloadAgainstOriginalConstants()
		{
			string path = PayloadPath;
			if (!File.Exists(path))
			{
				File.WriteAllText(path, CapturePayloads());
				return;
			}
			string json = File.ReadAllText(path);
			Dictionary<string, JsonElement>? envelope;
			try
			{
				envelope = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
			}
			catch (JsonException ex)
			{
				Assert.Fail(
					$"Persistence file {path} is not valid JSON ({ex.Message}); delete it to re-initialize."
				);
				return;
			}
			if (
				envelope == null
				|| !envelope.TryGetValue("version", out JsonElement versionElement)
				|| versionElement.ValueKind != JsonValueKind.Number
				|| !versionElement.TryGetInt32(out int version)
			)
			{
				Assert.Fail(
					$"Persistence file {path} has no version marker; delete it to re-initialize."
				);
				return;
			}
			if (version != PayloadVersion)
			{
				// Storage format moved on: the old file is meaningless, not wrong.
				File.WriteAllText(path, CapturePayloads());
				return;
			}
			if (!envelope.TryGetValue("entries", out JsonElement entriesElement))
			{
				Assert.Fail($"Persistence file {path} has no entries; delete it to re-initialize.");
				return;
			}
			Dictionary<string, uint[]>? entries = JsonSerializer.Deserialize<
				Dictionary<string, uint[]>
			>(entriesElement.GetRawText());
			Assert.NotNull(entries);
			ValidatePayloads(entries!);
		}

		private static string CapturePayloads()
		{
			var entries = new Dictionary<string, uint[]>();
			entries["playerHealth"] = SaveCell(Bits.From(100));
			entries["totalScore"] = SaveCell(Bits.From(250000u));
			entries["movementSpeed"] = SaveCell(Bits.From(6.5f));
			entries["lifetimeExperiencePoints"] = SaveCell(Bits.From(123456789012345L));
			entries["walletBalance128"] = SaveCell128(0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL);
			entries["boundsMulti"] = SaveMultiCell(11UL, 22UL, 33UL);
			var envelope = new Dictionary<string, object>
			{
				{ "version", PayloadVersion },
				{ "entries", entries },
			};
			return JsonSerializer.Serialize(envelope);
		}

		private static void ValidatePayloads(Dictionary<string, uint[]> entries)
		{
			Assert.Equal(100, Bits.ToInt(LoadCell(Require(entries, "playerHealth"))));
			Assert.Equal(250000u, Bits.ToUInt(LoadCell(Require(entries, "totalScore"))));
			Assert.Equal(6.5f, Bits.ToFloat(LoadCell(Require(entries, "movementSpeed"))));
			Assert.Equal(
				123456789012345L,
				Bits.ToLong(LoadCell(Require(entries, "lifetimeExperiencePoints")))
			);
			(ulong lo, ulong hi) = LoadCell128(Require(entries, "walletBalance128"));
			Assert.Equal(0x0123456789ABCDEFUL, lo);
			Assert.Equal(0xFEDCBA9876543210UL, hi);
			(ulong mLo, ulong mHi, ulong tail) = LoadMultiCell(Require(entries, "boundsMulti"));
			Assert.Equal(11UL, mLo);
			Assert.Equal(22UL, mHi);
			Assert.Equal(33UL, tail);
		}

		private static uint[] Require(Dictionary<string, uint[]> entries, string name)
		{
			Assert.True(
				entries.TryGetValue(name, out uint[]? packed),
				$"Persistence entry {name} is missing."
			);
			Assert.NotNull(packed);
			return packed!;
		}

		private static uint[] SaveCell(ulong plain)
		{
			var cell = default(Cell);
			cell.Protect(plain);
			Span<ulong> words = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(words, saveKey);
			saveKey.CopyTo(words.Slice(Cell.WordCount));
			return SerializationFormat.Pack(words);
		}

		private static ulong LoadCell(uint[] packed)
		{
			var cell = default(Cell);
			Span<ulong> words = stackalloc ulong[Cell.WordCount + KeySet.WordCount];
			Assert.True(
				SerializationFormat.TryUnpack(packed, words),
				"Cell payload has an unexpected length."
			);
			cell.RestoreStorageWords(words, KeySet.FromWords(words.Slice(Cell.WordCount)));
			return cell.Unprotect();
		}

		private static uint[] SaveCell128(ulong lo, ulong hi)
		{
			var cell = default(Cell128);
			cell.Protect(lo, hi);
			Span<ulong> words = stackalloc ulong[Cell128.WordCount + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cell.CopyStorageWords(words, saveKey);
			saveKey.CopyTo(words.Slice(Cell128.WordCount));
			return SerializationFormat.Pack(words);
		}

		private static (ulong Lo, ulong Hi) LoadCell128(uint[] packed)
		{
			var cell = default(Cell128);
			Span<ulong> words = stackalloc ulong[Cell128.WordCount + KeySet.WordCount];
			Assert.True(
				SerializationFormat.TryUnpack(packed, words),
				"Cell128 payload has an unexpected length."
			);
			cell.RestoreStorageWords(words, KeySet.FromWords(words.Slice(Cell128.WordCount)));
			return cell.Unprotect();
		}

		private static uint[] SaveMultiCell(ulong lo, ulong hi, ulong tail)
		{
			// Mirrors the 192-bit wrappers (one Cell128 + one Cell, one save key).
			var cellA = default(Cell128);
			var cellB = default(Cell);
			cellA.Protect(lo, hi);
			cellB.Protect(tail);
			const int dataWords = Cell128.WordCount + Cell.WordCount;
			Span<ulong> words = stackalloc ulong[dataWords + KeySet.WordCount];
			KeySet saveKey = Vault.NewStorageKey();
			cellA.CopyStorageWords(words, saveKey);
			cellB.CopyStorageWords(words.Slice(Cell128.WordCount), saveKey);
			saveKey.CopyTo(words.Slice(dataWords));
			return SerializationFormat.Pack(words);
		}

		private static (ulong Lo, ulong Hi, ulong Tail) LoadMultiCell(uint[] packed)
		{
			var cellA = default(Cell128);
			var cellB = default(Cell);
			const int dataWords = Cell128.WordCount + Cell.WordCount;
			Span<ulong> words = stackalloc ulong[dataWords + KeySet.WordCount];
			Assert.True(
				SerializationFormat.TryUnpack(packed, words),
				"Multi-cell payload has an unexpected length."
			);
			KeySet saveKey = KeySet.FromWords(words.Slice(dataWords));
			cellA.RestoreStorageWords(words, saveKey);
			cellB.RestoreStorageWords(words.Slice(Cell128.WordCount), saveKey);
			(ulong lo, ulong hi) = cellA.Unprotect();
			return (lo, hi, cellB.Unprotect());
		}
	}
}
