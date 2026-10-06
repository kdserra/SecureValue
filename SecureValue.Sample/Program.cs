using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SecureValue.Numerics;

namespace SecureValue.Sample
{
	internal static class Program
	{
		private const int HotPathIterations = 1_000_000;
		private const int RenderPreviewCount = 10;

		// Plain string literals for the beginner-friendly arm: all short enough
		// (<=16 chars) to seal inline with zero heap allocations. Allocated once
		// at type init, outside every measurement.
		private static readonly string[] ChestItems =
		{
			"wpn_excalibur_01",
			"crown_01",
			"Dragon Egg",
			"Mithril",
			"hp_potion_03",
		};

		// Preview budget for the beginner arms below: a static field lets their
		// static lambdas count down without capturing locals.
		// Reset at the start of each run; the warmup pass spends one preview,
		// the measured run spends the rest.
		private static int s_previewRemaining;

		private static void Main(string[] args)
		{
			Console.WriteLine("== SecureValue sample ==");
			Console.WriteLine();

			ShowWrapperOverview();
			ShowSpanApiBasics();
			RunHotPathComparison();

			Console.WriteLine();
			Console.WriteLine("Press enter to exit...");
			Console.ReadLine();
		}

		// ---------------------------------------------------------------------
		// Section 1: wrapper basics. Assignments encrypt, reads decrypt, and every
		// supported type works through implicit conversions and operators.
		// ---------------------------------------------------------------------
		private static void ShowWrapperOverview()
		{
			// Implicit conversions are supported.
			SecureInt hp = 100;
			hp += 25;

			SecureFloat speed = 3.75f;
			SecureDouble precise = 1.0 / 3.0;
			SecureDecimal money = 1234.56m;

			// Arithmetic and comparisons run on decrypted temporaries; the
			// stored bytes are always Feistel-encrypted.
			SecureLong big = long.MaxValue;
			SecureULong counter = 0;
			counter++;

			// The remaining integer widths.
			SecureByte level = 255;
			SecureSByte delta = -128;
			SecureShort temperature = -273;
			SecureUShort port = 8080;
			SecureUInt score = 3000000000;

			SecureBool alive = true;
			SecureChar grade = 'A';
			SecureRune emoji = Rune.GetRuneAt("😀", 0);

			SecureGuid sessionId = Guid.NewGuid();
			SecureDateTime lastLogin = DateTime.UtcNow;
			SecureDateTimeOffset expires = DateTimeOffset.UtcNow.AddHours(1);
			SecureDateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
			SecureTimeOnly now = TimeOnly.FromDateTime(DateTime.UtcNow);
			SecureTimeSpan sessionLength = TimeSpan.FromMinutes(30);

			// Every wrapper encrypts with a fresh random salt: two instances
			// holding the same plaintext store completely different ciphertext.
			var a = new SecureInt(42);
			var b = new SecureInt(42);

			// Strings are encrypted too.
			SecureString secret = "Secret!";

			// Numerics
			SecureVector2 direction = new Vector2(1f, 2f);
			SecureVector3 position = new Vector3(1f, 2f, 3f);
			position = (SecureVector3)((Vector3)position + new Vector3(0.5f, 0.5f, 0.5f));
			SecureVector4 homogeneous = new Vector4(1f, 2f, 3f, 1f);
			SecureQuaternion rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f);
			SecurePlane ground = new Plane(0f, 1f, 0f, 0f);
			SecureComplex signal = new Complex(1.5, -2.5);
			SecureMatrix3x2 transform2D = new Matrix3x2(1f, 0f, 0f, 1f, 5f, 6f);
			SecureMatrix4x4 view = Matrix4x4.CreateLookAt(
				Vector3.UnitZ,
				Vector3.Zero,
				Vector3.UnitY
			);
			SecureBigInteger huge = BigInteger.Pow(2, 100);

			// Ensure Rune can be rendered by the Console.
			Console.OutputEncoding = Encoding.UTF8;

			// Display the values.
			Console.WriteLine($"hp: {hp}");
			Console.WriteLine($"speed: {speed}");
			Console.WriteLine($"precise: {precise}");
			Console.WriteLine($"money: {money}");
			Console.WriteLine($"big: {big}");
			Console.WriteLine($"counter: {counter}");
			Console.WriteLine($"level: {level}");
			Console.WriteLine($"delta: {delta}");
			Console.WriteLine($"temperature: {temperature}");
			Console.WriteLine($"port: {port}");
			Console.WriteLine($"score: {score}");
			Console.WriteLine($"alive: {alive}");
			Console.WriteLine($"grade: {grade}");
			Console.WriteLine($"emoji: {emoji}");
			Console.WriteLine($"sessionId: {sessionId}");
			Console.WriteLine($"lastLogin: {lastLogin:O}");
			Console.WriteLine($"expires: {expires:O}");
			Console.WriteLine($"today: {today:O}");
			Console.WriteLine($"now: {now}");
			Console.WriteLine($"sessionLength: {sessionLength}");
			Console.WriteLine($"a: {a}, b: {b} (same plaintext)");
			Console.WriteLine($"secret: {secret}");
			Console.WriteLine($"direction: {direction}");
			Console.WriteLine($"position: {position}");
			Console.WriteLine($"homogeneous: {homogeneous}");
			Console.WriteLine($"rotation: {rotation}");
			Console.WriteLine($"ground: {ground}");
			Console.WriteLine($"signal: {signal}");
			Console.WriteLine($"transform2D: {transform2D}");
			Console.WriteLine($"view: {view}");
			Console.WriteLine($"huge: {huge}");
			Console.WriteLine();
		}

		// ---------------------------------------------------------------------
		// Section 2: SecureString span API basics — zero-alloc reads and writes,
		// zero-copy passing, and the managed stack-buffer callbacks.
		// ---------------------------------------------------------------------
		private static void ShowSpanApiBasics()
		{
			Console.WriteLine("-- SecureString Span API (Advanced) --");
			Console.WriteLine();

			// Zero-alloc read: decrypt into a stack buffer, zero it afterwards.
			SecureString heldItem = "Dragon Egg";
			Span<char> itemBuffer = stackalloc char[heldItem.Length];
			try
			{
				heldItem.CopyTo(itemBuffer);
				if (itemBuffer.SequenceEqual("Dragon Egg"))
				{
					Console.WriteLine("quest item equipped: Dragon Egg");
				}
			}
			finally
			{
				CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(itemBuffer));
			}

			// Zero-alloc write: assign the span directly — no intermediate string,
			// no Parse call. Both Span<char> and ReadOnlySpan<char> bind.
			ReadOnlySpan<char> packetName = "Mithril";
			SecureString lootName = packetName;
			Console.WriteLine($"looted: {lootName}");

			// Zero-copy passing: hand the 112-byte struct to a method by `in`
			// (an 8-byte reference) instead of copying it, then compare in place.
			SecureString equippedWeaponId = "wpn_excalibur_01";
			Console.WriteLine($"equipped weapon damage: {GetItemDamage(in equippedWeaponId)}");

			// `ref readonly`: alias the stored value without copying it, then read
			// through the alias (or hand it onward by `in`) as needed.
			ref readonly SecureString shopDisplay = ref equippedWeaponId;
			Console.WriteLine($"shop item chars: {shopDisplay.Length}");
			Console.WriteLine($"shop weapon damage: {GetItemDamage(in shopDisplay)}");

			// Managed stack buffer: the library sizes, fills, and zeroes the
			// buffer — no manual stackalloc/try-finally at the call site.
			SecureString weaponId = "wpn_excalibur_01";
			weaponId.StackDecrypt(static id =>
			{
				if (id.SequenceEqual("wpn_excalibur_01"))
				{
					Console.WriteLine("stack-decrypted weapon ready");
				}
			});

			if (
				weaponId.TryStackDecrypt(
					static id => id.SequenceEqual("wpn_excalibur_01"),
					out bool weaponMatch
				) && weaponMatch
			)
			{
				Console.WriteLine("try-stack-decrypted weapon ready");
			}

			Console.WriteLine();
		}

		private static int GetItemDamage(in SecureString itemId)
		{
			if (itemId.SequenceEqual("wpn_excalibur_01"))
			{
				return 50;
			}
			return 10;
		}

		// ---------------------------------------------------------------------
		// Section 3: hot-path comparison. Renders a moving player's position every
		// frame for 1M iterations, five ways: materialized strings (allocating
		// baseline), manual span buffers (zero-alloc), the managed stack buffer
		// via StackDecrypt (zero-alloc), beginner-friendly plain-string writes
		// with managed reads (zero-alloc, no spans or stackalloc in sight), and
		// string-literal writes with managed reads (zero-alloc, the simplest
		// possible usage). Arms 1-3 preview the first 10 frames via an explicit
		// counter; Arms 4-5 preview the first 10 reads via a shared static
		// countdown (no state variable, no capturing lambda).
		// ---------------------------------------------------------------------
		private static void RunHotPathComparison()
		{
			Console.WriteLine("SecureString Span vs Non-Span.");
			Console.WriteLine(
				"Renders a moving player's position every frame for 1M iterations, five ways."
			);
			Console.WriteLine();

			WriteBanner("ARM 1: MATERIALIZED STRINGS (ALLOCATING BASELINE)");
			Measure("Arm 1 (materialized strings)", RunMaterializedStrings);

			WriteBanner("ARM 2: MANUAL SPAN BUFFERS (ZERO-ALLOC)");
			Measure("Arm 2 (manual span buffers)", RunManualSpanBuffers);

			WriteBanner("ARM 3: MANAGED STACK BUFFER (ZERO-ALLOC)");
			// Warmup: a single unmeasured iteration fills the static-lambda
			// delegate cache, so the printout reflects steady-state (0 bytes)
			// rather than the one-time 64-byte cache fill.
			RunManagedStackBuffer(1, render: false);
			Measure(
				"Arm 3 (managed stack buffer)",
				() => RunManagedStackBuffer(HotPathIterations, render: true)
			);

			WriteBanner(
				"ARM 4: BASIC DYNAMIC WRITE + MANAGED READ (ZERO-ALLOC, BEGINNER FRIENDLY)"
			);
			// Same warmup rationale as Arm 3: this is a different lambda (different
			// call site), so it has its own one-time delegate cache to fill.
			RunBasicWriteManagedRead(1);
			Measure(
				"Arm 4 (basic write + managed read)",
				() => RunBasicWriteManagedRead(HotPathIterations)
			);

			WriteBanner(
				"ARM 5: BASIC LITERAL WRITE + MANAGED READ (ZERO-ALLOC, BEGINNER FRIENDLY)"
			);
			// Same warmup rationale as Arms 3-4: own lambda, own delegate cache.
			RunLiteralWriteManagedRead(1);
			Measure(
				"Arm 5 (literal write + managed read)",
				() => RunLiteralWriteManagedRead(HotPathIterations)
			);

			WriteBanner("ARM 6: STACKDECRYPT WITH RETURN VALUE (ZERO-ALLOC)");
			RunStackDecryptWithReturn(1);
			Measure(
				"Arm 6 (StackDecrypt return value)",
				() => RunStackDecryptWithReturn(HotPathIterations)
			);
		}

		private static void WriteBanner(string title)
		{
			Console.WriteLine($"------- {title} -------");
			Console.WriteLine();
			Console.WriteLine($"Iterations: {HotPathIterations:N0}");
			Console.WriteLine();
		}

		private static Vector3 MovingPlayerPos() =>
			new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());

		// Renders only the first frames so 1M iterations don't flood the console.
		// Takes the span directly: calling ToString() here would allocate inside
		// the measured loop and taint the zero-alloc arms.
		private static void RenderPreview(int i, ReadOnlySpan<char> text)
		{
			if (i < RenderPreviewCount)
			{
				Console.WriteLine(text);
			}
			else if (i == RenderPreviewCount + 1)
			{
				Console.WriteLine("[Further results upto 1 million not shown]");
			}
		}

		private static void RunMaterializedStrings()
		{
			for (int i = 0; i < HotPathIterations; i++)
			{
				var pos = MovingPlayerPos();

				// Format Vector3 into the string.
				SecureString text = pos.ToString("F1");

				// Simulate rendering the string to UI.
				RenderPreview(i, text.Decrypted.AsSpan());
			}
		}

		private static void RunManualSpanBuffers()
		{
			// Hoisted above the loop on purpose: a stackalloc inside the loop
			// body accumulates stack until this method returns.
			Span<char> buffer = stackalloc char[64];
			Span<char> dst = stackalloc char[64];
			try
			{
				for (int i = 0; i < HotPathIterations; i++)
				{
					var pos = MovingPlayerPos();

					// Format Vector3 into the stack buffer.
					int charsWritten = FormatVector3Span(pos, buffer);

					SecureString text = buffer[..charsWritten];
					text.CopyTo(dst);

					// Simulate rendering the span to UI (slice to what was
					// written: the rest of the reused buffer is stale padding).
					RenderPreview(i, dst[..charsWritten]);

					CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(buffer));
					CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(dst));
				}
			}
			finally
			{
				CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(buffer));
				CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(dst));
			}
		}

		private static void RunManagedStackBuffer(int iterations, bool render)
		{
			Span<char> msgBuffer = stackalloc char[32];
			try
			{
				for (int i = 0; i < iterations; i++)
				{
					i.TryFormat(msgBuffer[0..], out int written);
					ReadOnlySpan<char> msg = msgBuffer[..written];

					// Write with zero heap allocations.
					SecureString write = msg;

					// Read with zero heap allocations.
					write.StackDecrypt(
						(Number: i, Render: render),
						static (id, state) =>
						{
							if (state.Render)
							{
								RenderPreview(state.Number, id);
							}
						}
					);

					CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(msgBuffer));
				}
			}
			finally
			{
				CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(msgBuffer));
			}
		}

		// Beginner-friendly zero-alloc: write using cached string assignment in,
		// managed stack buffer via StackDecrypt on the way out.
		// Allocation free because SecureString <= 16 characters create zero-heap allocations.
		// Note: Typically, you would use the state argument instead of a static field, but to keep this example
		// beginner-friendly, we are using a static field to simplify the lambda expression.
		private static void RunBasicWriteManagedRead(int iterations)
		{
			s_previewRemaining = RenderPreviewCount;
			for (int i = 0; i < iterations; i++)
			{
				// Dynamic write with zero heap allocations.
				SecureString rewardItem = ChestItems[i % ChestItems.Length];

				// Read with zero heap allocations, previewing the first values.
				rewardItem.StackDecrypt(static id =>
				{
					if (s_previewRemaining > 0)
					{
						s_previewRemaining--;
						Console.WriteLine(id);
						if (s_previewRemaining == 0)
						{
							Console.WriteLine("[Further results upto 1 million not shown]");
						}
					}
				});
			}
		}

		// Simplest possible zero-alloc: write using a string literal,
		// managed stack buffer via StackDecrypt on the way out.
		// Allocation free because SecureString <= 16 characters create zero-heap allocations.
		// Note: Typically, you would use the state argument instead of a static field, but to keep this example
		// beginner-friendly, we are using a static field to simplify the lambda expression.
		private static void RunLiteralWriteManagedRead(int iterations)
		{
			s_previewRemaining = RenderPreviewCount;
			for (int i = 0; i < iterations; i++)
			{
				SecureString saved = "wpn_excalibur_01";

				saved.StackDecrypt(static id =>
				{
					if (s_previewRemaining > 0)
					{
						s_previewRemaining--;
						Console.WriteLine(id);
						if (s_previewRemaining == 0)
						{
							Console.WriteLine("[Further results upto 1 million not shown]");
						}
					}
				});
			}
		}

		// StackDecrypt with return value. Demonstrates the TResult overload
		// that decrypts onto the stack and returns the function result — no
		// out-parameter, no intermediate string, no heap allocation.
		// Uses the stateful overload to pass the expected item name without
		// capturing a closure.
		private static void RunStackDecryptWithReturn(int iterations)
		{
			Span<char> buffer = stackalloc char[128];

			/// <summary>
			/// Formats "{item} damage: {damage}" into <paramref name="buffer"/>.
			/// </summary>
			static ReadOnlySpan<char> FormatSpan(Span<char> buf, int i, int damage)
			{
				int pos = 0;
				ChestItems[i % ChestItems.Length].AsSpan().CopyTo(buf[pos..]);
				pos += ChestItems[i % ChestItems.Length].Length;
				" damage: ".AsSpan().CopyTo(buf[pos..]);
				pos += " damage: ".Length;
				damage.TryFormat(buf[pos..], out int written);
				pos += written;
				return buf[..pos];
			}

			try
			{
				for (int i = 0; i < iterations; i++)
				{
					SecureString rewardItem = ChestItems[i % ChestItems.Length];
					int damage = rewardItem.StackDecrypt(
						ChestItems[0],
						static (id, expected) => id.SequenceEqual(expected) ? 50 : 10
					);

					if (i < RenderPreviewCount)
					{
						Console.WriteLine(FormatSpan(buffer, i, damage));
					}
					else if (i == RenderPreviewCount)
					{
						Console.WriteLine("[Further results upto 1 million not shown]");
					}

					CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(buffer));
				}
			}
			finally
			{
				CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(buffer));
			}
		}

		private static int FormatVector3Span(Vector3 pos, Span<char> buffer)
		{
			int charsWritten = 0;
			buffer[charsWritten++] = '<';

			pos.X.TryFormat(buffer[charsWritten..], out int written, "F1");
			charsWritten += written;

			buffer[charsWritten++] = ',';
			buffer[charsWritten++] = ' ';

			pos.Y.TryFormat(buffer[charsWritten..], out written, "F1");
			charsWritten += written;

			buffer[charsWritten++] = ',';
			buffer[charsWritten++] = ' ';

			pos.Z.TryFormat(buffer[charsWritten..], out written, "F1");
			charsWritten += written;

			buffer[charsWritten++] = '>';
			return charsWritten;
		}

		private static void Measure(string name, Action action)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			long before = GC.GetAllocatedBytesForCurrentThread();

			action.Invoke();

			long after = GC.GetAllocatedBytesForCurrentThread();
			Console.WriteLine();
			Console.WriteLine($"{name} Allocated: {after - before:N0} bytes");
			Console.WriteLine();
		}
	}
}
