using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace SecureValue.Benchmarks
{
	/// <summary>
	/// One write+read round-trip benchmark per supported wrapper type. Every
	/// method name and Description states the secured type being measured so
	/// result tables are self-identifying.
	/// Run with: --filter *AllWrappersBenchmarks*
	/// </summary>
	[MemoryDiagnoser]
	public class AllWrappersBenchmarks
	{
		[Benchmark(Description = "SecureBool")]
		public bool SecureBool_WriteRead()
		{
			SecureBool v = true;
			return v;
		}

		[Benchmark(Description = "SecureByte")]
		public byte SecureByte_WriteRead()
		{
			SecureByte v = 200;
			return v;
		}

		[Benchmark(Description = "SecureSByte")]
		public sbyte SecureSByte_WriteRead()
		{
			SecureSByte v = -100;
			return v;
		}

		[Benchmark(Description = "SecureChar")]
		public char SecureChar_WriteRead()
		{
			SecureChar v = 'X';
			return v;
		}

		[Benchmark(Description = "SecureShort")]
		public short SecureShort_WriteRead()
		{
			SecureShort v = -300;
			return v;
		}

		[Benchmark(Description = "SecureUShort")]
		public ushort SecureUShort_WriteRead()
		{
			SecureUShort v = 60000;
			return v;
		}

		[Benchmark(Description = "SecureInt")]
		public int SecureInt_WriteRead()
		{
			SecureInt v = 123456;
			return v;
		}

		[Benchmark(Description = "SecureUInt")]
		public uint SecureUInt_WriteRead()
		{
			SecureUInt v = 4000000000;
			return v;
		}

		[Benchmark(Description = "SecureLong")]
		public long SecureLong_WriteRead()
		{
			SecureLong v = long.MaxValue;
			return v;
		}

		[Benchmark(Description = "SecureULong")]
		public ulong SecureULong_WriteRead()
		{
			SecureULong v = ulong.MaxValue;
			return v;
		}

		[Benchmark(Description = "SecureFloat")]
		public float SecureFloat_WriteRead()
		{
			SecureFloat v = 3.14f;
			return v;
		}

		[Benchmark(Description = "SecureDouble")]
		public double SecureDouble_WriteRead()
		{
			SecureDouble v = 2.718281828;
			return v;
		}

		[Benchmark(Description = "SecureDecimal")]
		public decimal SecureDecimal_WriteRead()
		{
			SecureDecimal v = 1234.56m;
			return v;
		}

		private static Guid s_writeGuid = Guid.NewGuid();

		[Benchmark(Description = "SecureGuid")]
		public Guid SecureGuid_WriteRead()
		{
			SecureGuid v = s_writeGuid;
			return v;
		}

		[Benchmark(Description = "SecureDateTime")]
		public DateTime SecureDateTime_WriteRead()
		{
			SecureDateTime v = new DateTime(2026, 9, 19, 12, 0, 0, DateTimeKind.Utc);
			return v;
		}

		[Benchmark(Description = "SecureDateTimeOffset")]
		public DateTimeOffset SecureDateTimeOffset_WriteRead()
		{
			SecureDateTimeOffset v = new DateTimeOffset(
				2026,
				9,
				19,
				12,
				0,
				0,
				TimeSpan.FromHours(2)
			);
			return v;
		}

		[Benchmark(Description = "SecureTimeSpan")]
		public TimeSpan SecureTimeSpan_WriteRead()
		{
			SecureTimeSpan v = TimeSpan.FromMinutes(90);
			return v;
		}

        [Benchmark(Description = "SecureString")]
        public string? SecureString_WriteRead()
        {
            SecureString v = "benchmark secret";
            return v;
        }

        private char[] buffer = new char[16];

		[Benchmark(Description = "SecureString_Span")]
		public ReadOnlySpan<char> SecureString_Span_WriteRead()
		{
			SecureString v = "benchmark secret";
			v.CopyTo(buffer);
			return buffer;
		}

        [Benchmark(Description = "SecureBigInteger")]
        public BigInteger SecureBigInteger_WriteRead()
        {
            Numerics.SecureBigInteger v = 4000000000000000000;
            return v;
        }

#if NET6_0_OR_GREATER
        [Benchmark(Description = "SecureDateOnly")]
		public DateOnly SecureDateOnly_WriteRead()
		{
			SecureDateOnly v = new DateOnly(2026, 2, 28);
			return v;
		}

		[Benchmark(Description = "SecureTimeOnly")]
		public TimeOnly SecureTimeOnly_WriteRead()
		{
			SecureTimeOnly v = new TimeOnly(12, 34, 56);
			return v;
		}
#endif

#if NET
		[Benchmark(Description = "SecureRune")]
		public System.Text.Rune SecureRune_WriteRead()
		{
			SecureRune v = new System.Text.Rune(0x1F600);
			return v;
		}
#endif
	}
}
