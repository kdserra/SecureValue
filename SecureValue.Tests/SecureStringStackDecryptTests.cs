#nullable enable
using SecureValue;

namespace SecureValue.Tests
{
	/// <summary>
	/// SecureString.StackDecrypt / TryStackDecrypt: the library owns the stack
	/// buffer (sized from Length, zeroed on return) and hands the plaintext to a
	/// callback. Throwing overloads mirror CopyTo semantics; Try overloads mirror
	/// TryCopyTo semantics (false on unassigned/doubly-tampered, no exception tax).
	/// </summary>
	[Collection("TamperNotifier")]
	public class SecureStringStackDecryptTests
	{
		public SecureStringStackDecryptTests()
		{
			// The tamper-event throttle is process-global: reset per test so
			// event-count asserts never starve on budget consumed elsewhere.
			TamperingNotifier.ResetThrottleForTesting();
		}

		[Theory]
		[InlineData("")]
		[InlineData("a")]
		[InlineData("crown_01")]
		[InlineData("abcdefghijklmnop")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		[InlineData("héllo wörld🌍")]
		public void StackDecrypt_ActionReceivesPlaintext(string plain)
		{
			SecureString s = plain;
			string seen = null!;
			// Non-static path (captures): simplest assertion of delivered content.
			s.StackDecrypt(span => seen = span.ToString());
			Assert.Equal(plain, seen);
		}

		[Fact]
		public void StackDecrypt_StatelessFuncReturnsResult()
		{
			SecureString s = "wpn_excalibur_01";
			int damage = s.StackDecrypt(static id =>
				id.SequenceEqual("wpn_excalibur_01") ? 50 : 10
			);
			Assert.Equal(50, damage);
		}

		[Fact]
		public void StackDecrypt_StatefulFuncReturnsResult()
		{
			SecureString s = "wpn_excalibur_01";
			int damage = s.StackDecrypt(
				"wpn_excalibur_01",
				static (id, expected) => id.SequenceEqual(expected) ? 50 : 10
			);
			Assert.Equal(50, damage);

			int miss = s.StackDecrypt(
				"wpn_rusty_dagger",
				static (id, expected) => id.SequenceEqual(expected) ? 50 : 10
			);
			Assert.Equal(10, miss);
		}

		[Theory]
		[InlineData("")]
		[InlineData("crown_01")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		public void StackDecrypt_StatefulActionReceivesPlaintextAndState(string plain)
		{
			SecureString s = plain;
			// Multi-arg state via tuple: the span content plus every state
			// field must arrive intact, with no capturing lambda.
			var state = (Expected: plain, Seen: new string[1]);
			s.StackDecrypt(state, static (span, st) => st.Seen[0] = span.ToString());
			Assert.Equal(plain, state.Seen[0]);
			Assert.Equal(plain, state.Expected);
		}

		[Fact]
		public void StackDecrypt_StatefulActionMatchesSamplePattern()
		{
			SecureString s = "Hello, world: 7";
			var box = new int[1] { -1 };
			s.StackDecrypt(
				(box, Number: 7),
				static (span, st) =>
				{
					if (span.SequenceEqual("Hello, world: 7"))
					{
						st.box[0] = st.Number;
					}
				}
			);
			Assert.Equal(7, box[0]);
		}

		[Fact]
		public void StackDecrypt_NullCallbackThrows()
		{
			SecureString s = "x";
			Assert.Throws<ArgumentNullException>(() => s.StackDecrypt((DecryptedSpanAction)null!));
			Assert.Throws<ArgumentNullException>(() =>
				s.StackDecrypt((DecryptedSpanFunc<int>)null!)
			);
			Assert.Throws<ArgumentNullException>(() =>
				s.StackDecrypt("state", (DecryptedSpanFunc<string, int>)null!)
			);
			Assert.Throws<ArgumentNullException>(() =>
				s.TryStackDecrypt((DecryptedSpanAction)null!)
			);
			Assert.Throws<ArgumentNullException>(() =>
				s.TryStackDecrypt((DecryptedSpanFunc<int>)null!, out _)
			);
			Assert.Throws<ArgumentNullException>(() =>
				s.TryStackDecrypt("state", (DecryptedSpanFunc<string, int>)null!, out _)
			);
			Assert.Throws<ArgumentNullException>(() =>
				s.StackDecrypt("state", (DecryptedSpanAction<string>)null!)
			);
			Assert.Throws<ArgumentNullException>(() =>
				s.TryStackDecrypt("state", (DecryptedSpanAction<string>)null!)
			);
		}

		[Fact]
		public void StackDecrypt_DefaultThrows()
		{
			Assert.Throws<UninitializedException>(() =>
				default(SecureString).StackDecrypt(static _ => { })
			);
			Assert.Throws<UninitializedException>(() =>
				default(SecureString).StackDecrypt(static _ => 0)
			);
			Assert.Throws<UninitializedException>(() =>
				default(SecureString).StackDecrypt("s", static (_, __) => 0)
			);
			Assert.Throws<UninitializedException>(() =>
				default(SecureString).StackDecrypt("s", static (_, __) => { })
			);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void StackDecrypt_SinglyTamperedRestoresAndRaises(string plain)
		{
			SecureString s = plain;
			object box = s;
			CorruptFirstWord(box, backup: false, heap: plain.Length > 16);
			var p = (SecureString)box;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			string seen = null!;
			p.StackDecrypt(span => seen = span.ToString());
			Assert.Equal(plain, seen);
			Assert.Equal(1, fired);
			// Healed by the first call: the second is silent.
			int before = fired;
			p.StackDecrypt(static _ => { });
			Assert.Equal(before, fired);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void StackDecrypt_DoublyTamperedThrows(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			Assert.Throws<TamperedException>(() =>
				((SecureString)box).StackDecrypt(static _ => { })
			);
		}

		[Fact]
		public void StackDecrypt_CallbackExceptionPropagates()
		{
			SecureString s = "boom";
			Assert.Throws<InvalidOperationException>(() =>
				s.StackDecrypt(static _ => throw new InvalidOperationException("user failure"))
			);
			// The value is untouched by the failed callback: still readable.
			Assert.Equal("boom", s.Decrypted);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void StackDecrypt_StatefulActionDoublyTamperedThrows(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			Assert.Throws<TamperedException>(() =>
				((SecureString)box).StackDecrypt("state", static (_, __) => { })
			);
		}

		[Theory]
		[InlineData("")]
		[InlineData("crown_01")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		public void TryStackDecrypt_ActionRoundTrips(string plain)
		{
			SecureString s = plain;
			string seen = "untouched";
			Assert.True(s.TryStackDecrypt(span => seen = span.ToString()));
			Assert.Equal(plain, seen);
		}

		[Fact]
		public void TryStackDecrypt_DefaultReturnsFalse()
		{
			Assert.False(default(SecureString).TryStackDecrypt(static _ => { }));
			Assert.False(default(SecureString).TryStackDecrypt(static _ => 0, out int result));
			Assert.Equal(0, result);
			Assert.False(
				default(SecureString).TryStackDecrypt("s", static (_, __) => 0, out result)
			);
			Assert.Equal(0, result);
			Assert.False(default(SecureString).TryStackDecrypt("s", static (_, __) => { }));
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryStackDecrypt_SinglyTamperedRecoversHealsAndRaises(string plain)
		{
			SecureString s = plain;
			object box = s;
			CorruptFirstWord(box, backup: false, heap: plain.Length > 16);
			var p = (SecureString)box;
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			string seen = null!;
			Assert.True(p.TryStackDecrypt(span => seen = span.ToString()));
			Assert.Equal(plain, seen);
			Assert.Equal(1, fired);
			// Healed by the first call: the second is silent.
			Assert.True(p.TryStackDecrypt(static _ => { }));
			Assert.Equal(1, fired);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryStackDecrypt_DoublyTamperedReturnsFalse(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			int fired = 0;
			TamperingNotifier.TamperingDetected += () => fired++;
			Assert.False(((SecureString)box).TryStackDecrypt(static _ => { }));
			Assert.True(fired > 0);
		}

		[Theory]
		[InlineData("")]
		[InlineData("crown_01")]
		[InlineData("hello world, this is a longer string for testing spans!")]
		public void TryStackDecrypt_StatefulActionRoundTrips(string plain)
		{
			SecureString s = plain;
			var state = (Expected: plain, Seen: new string[1]);
			Assert.True(
				s.TryStackDecrypt(state, static (span, st) => st.Seen[0] = span.ToString())
			);
			Assert.Equal(plain, state.Seen[0]);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryStackDecrypt_StatefulActionDoublyTamperedReturnsFalse(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			bool invoked = false;
			Assert.False(((SecureString)box).TryStackDecrypt("state", (_, __) => invoked = true));
			Assert.False(invoked);
		}

		[Fact]
		public void TryStackDecrypt_FuncOutReturnsResult()
		{
			SecureString s = "wpn_excalibur_01";
			Assert.True(
				s.TryStackDecrypt(static id => id.SequenceEqual("wpn_excalibur_01"), out bool match)
			);
			Assert.True(match);

			// A predicate miss is still a successful decrypt: returns true with false result.
			Assert.True(
				s.TryStackDecrypt(static id => id.SequenceEqual("wpn_rusty_dagger"), out match)
			);
			Assert.False(match);
		}

		[Fact]
		public void TryStackDecrypt_StatefulFuncOutReturnsResult()
		{
			SecureString s = "wpn_excalibur_01";
			Assert.True(
				s.TryStackDecrypt(
					"wpn_excalibur_01",
					static (id, expected) => id.SequenceEqual(expected) ? 50 : 10,
					out int damage
				)
			);
			Assert.Equal(50, damage);
		}

		[Theory]
		[InlineData("tamper me")]
		[InlineData("tamper me, this string is long enough for the heap")]
		public void TryStackDecrypt_FuncOutDoublyTamperedReturnsFalseWithDefault(string plain)
		{
			SecureString s = plain;
			object box = s;
			bool heap = plain.Length > 16;
			CorruptFirstWord(box, backup: false, heap: heap);
			CorruptFirstWord(box, backup: true, heap: heap);
			Assert.False(
				((SecureString)box).TryStackDecrypt(static id => id.Length, out int length)
			);
			Assert.Equal(0, length);
		}

		[Fact]
		public void TryStackDecrypt_CallbackExceptionPropagates()
		{
			SecureString s = "boom";
			Assert.Throws<InvalidOperationException>(() =>
				s.TryStackDecrypt(static _ => throw new InvalidOperationException("user failure"))
			);
		}

		[Fact]
		public void TryStackDecrypt_AllocationFree()
		{
			SecureString s = "allocation probe string for stack decrypt";
			Consume(s);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			Consume(s);
			long after = GC.GetAllocatedBytesForCurrentThread();
			Assert.Equal(before, after);
		}

		private static void Consume(SecureString s)
		{
			for (int i = 0; i < 1000; i++)
			{
				s.TryStackDecrypt(
					"crown_01",
					static (span, expected) => span.SequenceEqual(expected),
					out _
				);
				if (!s.TryStackDecrypt(static span => span.Length > 0, out bool ok) || !ok)
				{
					throw new InvalidOperationException("Corrupted stack decrypt.");
				}
			}
		}

		private static void CorruptFirstWord(object box, bool backup, bool heap)
		{
			const System.Reflection.BindingFlags flags =
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
			if (heap)
			{
				CipherArray(box, backup ? "_ciphersB" : "_ciphers")[0] ^= 1UL;
				return;
			}
			string field = backup ? "_w0B" : "_w0";
			var f = box.GetType().GetField(field, flags)!;
			f.SetValue(box, (ulong)f.GetValue(box)! ^ 1UL);
		}

		private static ulong[] CipherArray(object box, string field)
		{
			var f = box.GetType()
				.GetField(
					field,
					System.Reflection.BindingFlags.NonPublic
						| System.Reflection.BindingFlags.Instance
				)!;
			return (ulong[])f.GetValue(box)!;
		}
	}
}
