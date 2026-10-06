#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using SecureValue;

namespace SecureValue.Unity.Tests
{
	/// <summary>
	/// Cross-session persistence at the wrapper layer: the FIRST run in a fresh
	/// state only writes Assets/test-persistence.json (an initialization run cannot
	/// reload anything, so it passes). Every SUBSEQUENT run — a new editor session
	/// with fresh static keys, like a Unity restart — reloads the stored uint[]
	/// payloads through the genuine SaveToSerialized/LoadFromSerialized path and
	/// asserts them against the demo's fixed constants. Run the suite once (init),
	/// restart Unity, run it again (validate). Deleting the JSON forces a fresh
	/// initialization run. A version marker re-initializes (instead of failing)
	/// when the storage format itself changes; anything else wrong with a versioned
	/// file fails. Unity-family types share simple names with the Numerics family,
	/// so every wrapper below is FULLY QUALIFIED — never rely on usings here.
	/// </summary>
	public class PersistenceTests
	{
		private const int PayloadVersion = 1;

		private static string PayloadPath =>
			Path.Combine(Application.dataPath, "test-persistence.json");

		[Test]
		public void CrossSession_StoredPayloadsReloadAgainstOriginalConstants()
		{
			string path = PayloadPath;
			if (!File.Exists(path))
			{
				File.WriteAllText(path, CapturePayloads());
				UnityEditor.AssetDatabase.Refresh();
				Assert.Pass(
					"Initialized Assets/test-persistence.json; restart Unity and run again to validate."
				);
				return;
			}
			Dictionary<string, uint[]> entries = Parse(File.ReadAllText(path), out int version);
			if (version != PayloadVersion)
			{
				// Storage format moved on: the old file is meaningless, not wrong.
				File.WriteAllText(path, CapturePayloads());
				UnityEditor.AssetDatabase.Refresh();
				Assert.Pass("Storage format changed; re-initialized Assets/test-persistence.json.");
				return;
			}
			int checks = 0;
			int failures = 0;
			var log = new StringBuilder();
			Check(
				"playerHealth",
				Reload<SecureInt>(entries, "playerHealth", ref checks, ref failures, log).Decrypted,
				100,
				ref checks,
				ref failures,
				log
			);
			Check(
				"totalScore",
				Reload<SecureUInt>(entries, "totalScore", ref checks, ref failures, log).Decrypted,
				250000u,
				ref checks,
				ref failures,
				log
			);
			Check(
				"movementSpeed",
				Reload<SecureFloat>(
					entries,
					"movementSpeed",
					ref checks,
					ref failures,
					log
				).Decrypted,
				6.5f,
				ref checks,
				ref failures,
				log
			);
			Check(
				"playerName",
				Reload<SecureString>(
					entries,
					"playerName",
					ref checks,
					ref failures,
					log
				).Decrypted,
				"SecureValue",
				ref checks,
				ref failures,
				log
			);
			Check(
				"playerRankInitial",
				Reload<SecureChar>(
					entries,
					"playerRankInitial",
					ref checks,
					ref failures,
					log
				).Decrypted,
				'S',
				ref checks,
				ref failures,
				log
			);
			Check(
				"playerAccountId",
				Reload<SecureGuid>(
					entries,
					"playerAccountId",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Guid("11111111-2222-3333-4444-555555555555"),
				ref checks,
				ref failures,
				log
			);
			Check(
				"walletBalance",
				Reload<SecureDecimal>(
					entries,
					"walletBalance",
					ref checks,
					ref failures,
					log
				).Decrypted,
				1234.56m,
				ref checks,
				ref failures,
				log
			);
			Check(
				"lifetimeExperiencePoints",
				Reload<SecureLong>(
					entries,
					"lifetimeExperiencePoints",
					ref checks,
					ref failures,
					log
				).Decrypted,
				123456789012345L,
				ref checks,
				ref failures,
				log
			);
			Check(
				"globalLeaderboardScore",
				Reload<SecureULong>(
					entries,
					"globalLeaderboardScore",
					ref checks,
					ref failures,
					log
				).Decrypted,
				9876543210UL,
				ref checks,
				ref failures,
				log
			);
			Check(
				"enemyKillCount",
				Reload<SecureUShort>(
					entries,
					"enemyKillCount",
					ref checks,
					ref failures,
					log
				).Decrypted,
				(ushort)1234,
				ref checks,
				ref failures,
				log
			);
			Check(
				"ammoInMagazine",
				Reload<SecureShort>(
					entries,
					"ammoInMagazine",
					ref checks,
					ref failures,
					log
				).Decrypted,
				(short)30,
				ref checks,
				ref failures,
				log
			);
			Check(
				"currentPlayerLevel",
				Reload<SecureByte>(
					entries,
					"currentPlayerLevel",
					ref checks,
					ref failures,
					log
				).Decrypted,
				(byte)7,
				ref checks,
				ref failures,
				log
			);
			Check(
				"isGodModeEnabled",
				Reload<SecureBool>(
					entries,
					"isGodModeEnabled",
					ref checks,
					ref failures,
					log
				).Decrypted,
				true,
				ref checks,
				ref failures,
				log
			);
			Check(
				"difficultyLevel",
				Reload<SecureSByte>(
					entries,
					"difficultyLevel",
					ref checks,
					ref failures,
					log
				).Decrypted,
				(sbyte)2,
				ref checks,
				ref failures,
				log
			);
			Check(
				"criticalHitDamageMultiplier",
				Reload<SecureDouble>(
					entries,
					"criticalHitDamageMultiplier",
					ref checks,
					ref failures,
					log
				).Decrypted,
				1.5,
				ref checks,
				ref failures,
				log
			);
			Check(
				"eventActivationDate",
				Reload<SecureDateTimeOffset>(
					entries,
					"eventActivationDate",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.FromHours(2)),
				ref checks,
				ref failures,
				log
			);
			Check(
				"lastLoginTimeUtc",
				Reload<SecureDateTime>(
					entries,
					"lastLoginTimeUtc",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new DateTime(2026, 9, 24, 12, 30, 0, DateTimeKind.Utc),
				ref checks,
				ref failures,
				log
			);
			Check(
				"activeBoostDuration",
				Reload<SecureTimeSpan>(
					entries,
					"activeBoostDuration",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new TimeSpan(1, 37, 42),
				ref checks,
				ref failures,
				log
			);
			Check(
				"uiPosition",
				Reload<SecureValue.Unity.SecureVector2>(
					entries,
					"uiPosition",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Vector2(12.5f, -3.25f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"tileCoordinate",
				Reload<SecureValue.Unity.SecureVector2Int>(
					entries,
					"tileCoordinate",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Vector2Int(7, -3),
				ref checks,
				ref failures,
				log
			);
			Check(
				"respawnPoint",
				Reload<SecureValue.Unity.SecureVector3>(
					entries,
					"respawnPoint",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Vector3(1f, 2f, 3f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"gridCell",
				Reload<SecureValue.Unity.SecureVector3Int>(
					entries,
					"gridCell",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Vector3Int(1, -2, 3),
				ref checks,
				ref failures,
				log
			);
			Check(
				"spellEffectParameters",
				Reload<SecureValue.Unity.SecureVector4>(
					entries,
					"spellEffectParameters",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Vector4(1f, 2f, 3f, 4f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"minimapViewport",
				Reload<SecureValue.Unity.SecureRect>(
					entries,
					"minimapViewport",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Rect(10f, 20f, 100f, 50f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"inventorySlotArea",
				Reload<SecureValue.Unity.SecureRectInt>(
					entries,
					"inventorySlotArea",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new RectInt(1, 2, 10, 20),
				ref checks,
				ref failures,
				log
			);
			Check(
				"arenaBounds",
				Reload<SecureValue.Unity.SecureBounds>(
					entries,
					"arenaBounds",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)),
				ref checks,
				ref failures,
				log
			);
			Check(
				"buildZone",
				Reload<SecureValue.Unity.SecureBoundsInt>(
					entries,
					"buildZone",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
				ref checks,
				ref failures,
				log
			);
			Check(
				"teamColor",
				Reload<SecureValue.Unity.SecureColor>(
					entries,
					"teamColor",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Color(0.15f, 0.5f, 0.85f, 0.75f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"minimapPixel",
				Reload<SecureValue.Unity.SecureColor32>(
					entries,
					"minimapPixel",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Color32(237, 129, 64, 210),
				ref checks,
				ref failures,
				log
			);
			Check(
				"doorRotation",
				Reload<SecureValue.Unity.SecureQuaternion>(
					entries,
					"doorRotation",
					ref checks,
					ref failures,
					log
				).Decrypted,
				Quaternion.Euler(10f, 20f, 30f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"portalTransform",
				Reload<SecureValue.Unity.SecureMatrix4x4>(
					entries,
					"portalTransform",
					ref checks,
					ref failures,
					log
				).Decrypted,
				Matrix4x4.TRS(
					new Vector3(1f, 2f, 3f),
					Quaternion.Euler(15f, 30f, 45f),
					new Vector3(2.5f, 0.5f, 1.75f)
				),
				ref checks,
				ref failures,
				log
			);
			Check(
				"waterPlaneHeight",
				Reload<SecureValue.Unity.SecurePlane>(
					entries,
					"waterPlaneHeight",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"aimRay",
				Reload<SecureValue.Unity.SecureRay>(
					entries,
					"aimRay",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f)),
				ref checks,
				ref failures,
				log
			);
			Check(
				"enemyCollisionLayers",
				Reload<SecureValue.Unity.SecureLayerMask>(
					entries,
					"enemyCollisionLayers",
					ref checks,
					ref failures,
					log
				).Decrypted.value,
				5,
				ref checks,
				ref failures,
				log
			);
			Check(
				"joystickInput",
				Reload<SecureValue.Numerics.SecureVector2>(
					entries,
					"joystickInput",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Vector2(1.5f, -2.5f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"spawnPointPosition",
				Reload<SecureValue.Numerics.SecureVector3>(
					entries,
					"spawnPointPosition",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Vector3(1f, 2f, 3f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"shaderEffectParameters",
				Reload<SecureValue.Numerics.SecureVector4>(
					entries,
					"shaderEffectParameters",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Vector4(1f, 2f, 3f, 4f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"cameraRotation",
				Reload<SecureValue.Numerics.SecureQuaternion>(
					entries,
					"cameraRotation",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Quaternion(0.27f, 0.36f, 0.48f, 0.76f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"groundCollisionPlane",
				Reload<SecureValue.Numerics.SecurePlane>(
					entries,
					"groundCollisionPlane",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Plane(0.25f, 0.9f, 0.35f, 5.25f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"uiTransformMatrix",
				Reload<SecureValue.Numerics.SecureMatrix3x2>(
					entries,
					"uiTransformMatrix",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f),
				ref checks,
				ref failures,
				log
			);
			Check(
				"characterRigTransformMatrix",
				Reload<SecureValue.Numerics.SecureMatrix4x4>(
					entries,
					"characterRigTransformMatrix",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Matrix4x4(
					1f,
					2f,
					3f,
					4f,
					5f,
					6f,
					7f,
					8f,
					9f,
					10f,
					11f,
					12f,
					13f,
					14f,
					15f,
					16f
				),
				ref checks,
				ref failures,
				log
			);
			Check(
				"signalProcessingValue",
				Reload<SecureValue.Numerics.SecureComplex>(
					entries,
					"signalProcessingValue",
					ref checks,
					ref failures,
					log
				).Decrypted,
				new System.Numerics.Complex(1.5, -2.5),
				ref checks,
				ref failures,
				log
			);
			Check(
				"puzzleSolutionNumber",
				Reload<SecureValue.Numerics.SecureBigInteger>(
					entries,
					"puzzleSolutionNumber",
					ref checks,
					ref failures,
					log
				).Decrypted,
				System.Numerics.BigInteger.Parse("123456789012345678901234567890"),
				ref checks,
				ref failures,
				log
			);
			if (failures > 0)
			{
				Assert.Fail(
					$"Cross-session persistence: {failures}/{checks} values deviated:\n{log}"
				);
			}
			Assert.Pass(
				$"Cross-session persistence: {checks}/{checks} values match their original constants."
			);
		}

		private static string CapturePayloads()
		{
			var entries = new List<(string name, uint[] packed)>();
			entries.Add(("playerHealth", Save(new SecureInt(100))));
			entries.Add(("totalScore", Save(new SecureUInt(250000u))));
			entries.Add(("movementSpeed", Save(new SecureFloat(6.5f))));
			entries.Add(("playerName", Save(new SecureString("SecureValue"))));
			entries.Add(("playerRankInitial", Save(new SecureChar('S'))));
			entries.Add(
				(
					"playerAccountId",
					Save(new SecureGuid(new Guid("11111111-2222-3333-4444-555555555555")))
				)
			);
			entries.Add(("walletBalance", Save(new SecureDecimal(1234.56m))));
			entries.Add(("lifetimeExperiencePoints", Save(new SecureLong(123456789012345L))));
			entries.Add(("globalLeaderboardScore", Save(new SecureULong(9876543210UL))));
			entries.Add(("enemyKillCount", Save(new SecureUShort(1234))));
			entries.Add(("ammoInMagazine", Save(new SecureShort(30))));
			entries.Add(("currentPlayerLevel", Save(new SecureByte(7))));
			entries.Add(("isGodModeEnabled", Save(new SecureBool(true))));
			entries.Add(("difficultyLevel", Save(new SecureSByte(2))));
			entries.Add(("criticalHitDamageMultiplier", Save(new SecureDouble(1.5))));
			entries.Add(
				(
					"eventActivationDate",
					Save(
						new SecureDateTimeOffset(
							new DateTimeOffset(2026, 12, 31, 23, 59, 0, TimeSpan.FromHours(2))
						)
					)
				)
			);
			entries.Add(
				(
					"lastLoginTimeUtc",
					Save(new SecureDateTime(new DateTime(2026, 9, 24, 12, 30, 0, DateTimeKind.Utc)))
				)
			);
			entries.Add(("activeBoostDuration", Save(new SecureTimeSpan(new TimeSpan(1, 37, 42)))));
			entries.Add(
				(
					"uiPosition",
					Save(new SecureValue.Unity.SecureVector2(new Vector2(12.5f, -3.25f)))
				)
			);
			entries.Add(
				(
					"tileCoordinate",
					Save(new SecureValue.Unity.SecureVector2Int(new Vector2Int(7, -3)))
				)
			);
			entries.Add(
				("respawnPoint", Save(new SecureValue.Unity.SecureVector3(new Vector3(1f, 2f, 3f))))
			);
			entries.Add(
				("gridCell", Save(new SecureValue.Unity.SecureVector3Int(new Vector3Int(1, -2, 3))))
			);
			entries.Add(
				(
					"spellEffectParameters",
					Save(new SecureValue.Unity.SecureVector4(new Vector4(1f, 2f, 3f, 4f)))
				)
			);
			entries.Add(
				(
					"minimapViewport",
					Save(new SecureValue.Unity.SecureRect(new Rect(10f, 20f, 100f, 50f)))
				)
			);
			entries.Add(
				(
					"inventorySlotArea",
					Save(new SecureValue.Unity.SecureRectInt(new RectInt(1, 2, 10, 20)))
				)
			);
			entries.Add(
				(
					"arenaBounds",
					Save(
						new SecureValue.Unity.SecureBounds(
							new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f))
						)
					)
				)
			);
			entries.Add(
				(
					"buildZone",
					Save(
						new SecureValue.Unity.SecureBoundsInt(
							new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6))
						)
					)
				)
			);
			entries.Add(
				(
					"teamColor",
					Save(new SecureValue.Unity.SecureColor(new Color(0.15f, 0.5f, 0.85f, 0.75f)))
				)
			);
			entries.Add(
				(
					"minimapPixel",
					Save(new SecureValue.Unity.SecureColor32(new Color32(237, 129, 64, 210)))
				)
			);
			entries.Add(
				(
					"doorRotation",
					Save(new SecureValue.Unity.SecureQuaternion(Quaternion.Euler(10f, 20f, 30f)))
				)
			);
			entries.Add(
				(
					"portalTransform",
					Save(
						new SecureValue.Unity.SecureMatrix4x4(
							Matrix4x4.TRS(
								new Vector3(1f, 2f, 3f),
								Quaternion.Euler(15f, 30f, 45f),
								new Vector3(2.5f, 0.5f, 1.75f)
							)
						)
					)
				)
			);
			entries.Add(
				(
					"waterPlaneHeight",
					Save(
						new SecureValue.Unity.SecurePlane(
							new Plane(new Vector3(0.25f, 0.9f, 0.35f), 5.25f)
						)
					)
				)
			);
			entries.Add(
				(
					"aimRay",
					Save(
						new SecureValue.Unity.SecureRay(
							new Ray(new Vector3(1f, 2f, 3f), new Vector3(0.36f, 0.48f, 0.8f))
						)
					)
				)
			);
			entries.Add(
				(
					"enemyCollisionLayers",
					Save(new SecureValue.Unity.SecureLayerMask(new LayerMask() { value = 5 }))
				)
			);
			entries.Add(
				(
					"joystickInput",
					Save(
						new SecureValue.Numerics.SecureVector2(
							new System.Numerics.Vector2(1.5f, -2.5f)
						)
					)
				)
			);
			entries.Add(
				(
					"spawnPointPosition",
					Save(
						new SecureValue.Numerics.SecureVector3(
							new System.Numerics.Vector3(1f, 2f, 3f)
						)
					)
				)
			);
			entries.Add(
				(
					"shaderEffectParameters",
					Save(
						new SecureValue.Numerics.SecureVector4(
							new System.Numerics.Vector4(1f, 2f, 3f, 4f)
						)
					)
				)
			);
			entries.Add(
				(
					"cameraRotation",
					Save(
						new SecureValue.Numerics.SecureQuaternion(
							new System.Numerics.Quaternion(0.27f, 0.36f, 0.48f, 0.76f)
						)
					)
				)
			);
			entries.Add(
				(
					"groundCollisionPlane",
					Save(
						new SecureValue.Numerics.SecurePlane(
							new System.Numerics.Plane(0.25f, 0.9f, 0.35f, 5.25f)
						)
					)
				)
			);
			entries.Add(
				(
					"uiTransformMatrix",
					Save(
						new SecureValue.Numerics.SecureMatrix3x2(
							new System.Numerics.Matrix3x2(1f, 2f, 3f, 4f, 5f, 6f)
						)
					)
				)
			);
			entries.Add(
				(
					"characterRigTransformMatrix",
					Save(
						new SecureValue.Numerics.SecureMatrix4x4(
							new System.Numerics.Matrix4x4(
								1f,
								2f,
								3f,
								4f,
								5f,
								6f,
								7f,
								8f,
								9f,
								10f,
								11f,
								12f,
								13f,
								14f,
								15f,
								16f
							)
						)
					)
				)
			);
			entries.Add(
				(
					"signalProcessingValue",
					Save(
						new SecureValue.Numerics.SecureComplex(
							new System.Numerics.Complex(1.5, -2.5)
						)
					)
				)
			);
			entries.Add(
				(
					"puzzleSolutionNumber",
					Save(
						new SecureValue.Numerics.SecureBigInteger(
							System.Numerics.BigInteger.Parse("123456789012345678901234567890")
						)
					)
				)
			);
			return Write(entries);
		}

		private static uint[] Save<T>(T wrapper)
			where T : struct, ISecureSerialization
		{
			// One box for the whole call: SaveToSerialized only reads, so the box
			// can be discarded right after.
			object box = wrapper;
			return ((ISecureSerialization)box).SaveToSerialized();
		}

		private static T Reload<T>(
			Dictionary<string, uint[]> entries,
			string name,
			ref int checks,
			ref int failures,
			StringBuilder log
		)
			where T : struct, ISecureSerialization
		{
			checks++;
			if (!entries.TryGetValue(name, out uint[] packed) || packed == null)
			{
				failures++;
				log.AppendLine($"[Persistence] {name} has no recorded payload.");
				return default;
			}
			try
			{
				// One box for the whole call: the restored cell must survive on the
				// box until it is unboxed below (a per-call box would lose it).
				object box = default(T);
				((ISecureSerialization)box).LoadFromSerialized(packed);
				return (T)box;
			}
			catch (Exception ex)
			{
				failures++;
				log.AppendLine($"[Persistence] {name} load threw {ex.GetType().Name}.");
				return default;
			}
		}

		private static void Check<T>(
			string name,
			T actual,
			T expected,
			ref int checks,
			ref int failures,
			StringBuilder log
		)
		{
			checks++;
			if (!EqualityComparer<T>.Default.Equals(actual, expected))
			{
				failures++;
				log.AppendLine(
					$"[Persistence] {name} mismatch: expected {expected}, got {actual}."
				);
			}
		}

		private static string Write(List<(string name, uint[] packed)> entries)
		{
			var json = new StringBuilder();
			json.Append("{\"version\":");
			json.Append(PayloadVersion);
			foreach ((string name, uint[] packed) in entries)
			{
				json.Append(",\"");
				json.Append(name);
				json.Append("\":[");
				for (int i = 0; i < packed.Length; i++)
				{
					if (i > 0)
					{
						json.Append(',');
					}
					json.Append(
						packed[i].ToString(System.Globalization.CultureInfo.InvariantCulture)
					);
				}
				json.Append(']');
			}
			json.Append('}');
			return json.ToString();
		}

		private static Dictionary<string, uint[]> Parse(string json, out int version)
		{
			var entries = new Dictionary<string, uint[]>();
			version = 0;
			int i = 0;
			Expect(json, ref i, '{');
			while (true)
			{
				SkipWhitespace(json, ref i);
				if (Peek(json, i) == '}')
				{
					i++;
					break;
				}
				if (Peek(json, i) == ',')
				{
					i++;
					continue;
				}
				string key = ReadQuoted(json, ref i);
				SkipWhitespace(json, ref i);
				Expect(json, ref i, ':');
				SkipWhitespace(json, ref i);
				if (Peek(json, i) == '[')
				{
					entries[key] = ReadArray(json, ref i);
				}
				else
				{
					long number = ReadNumber(json, ref i);
					if (key == "version")
					{
						version = (int)number;
					}
				}
			}
			return entries;
		}

		private static uint[] ReadArray(string json, ref int i)
		{
			var values = new List<uint>();
			Expect(json, ref i, '[');
			while (true)
			{
				SkipWhitespace(json, ref i);
				if (Peek(json, i) == ']')
				{
					i++;
					break;
				}
				if (Peek(json, i) == ',')
				{
					i++;
					continue;
				}
				values.Add((uint)ReadNumber(json, ref i));
			}
			return values.ToArray();
		}

		private static long ReadNumber(string json, ref int i)
		{
			int start = i;
			while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '-'))
			{
				i++;
			}
			if (!long.TryParse(json.Substring(start, i - start), out long value))
			{
				throw new FormatException($"Expected a number at offset {start}.");
			}
			return value;
		}

		private static string ReadQuoted(string json, ref int i)
		{
			Expect(json, ref i, '"');
			int start = i;
			while (i < json.Length && json[i] != '"')
			{
				i++;
			}
			string key = json.Substring(start, i - start);
			Expect(json, ref i, '"');
			return key;
		}

		private static void SkipWhitespace(string json, ref int i)
		{
			while (i < json.Length && char.IsWhiteSpace(json[i]))
			{
				i++;
			}
		}

		private static char Peek(string json, int i)
		{
			if (i >= json.Length)
			{
				throw new FormatException("Unexpected end of JSON.");
			}
			return json[i];
		}

		private static void Expect(string json, ref int i, char c)
		{
			SkipWhitespace(json, ref i);
			if (Peek(json, i) != c)
			{
				throw new FormatException($"Expected '{c}' at offset {i}.");
			}
			i++;
		}
	}
}
#endif
