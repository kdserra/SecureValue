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
			return (byte)(v + 1);
		}

		[Benchmark(Description = "SecureSByte")]
		public sbyte SecureSByte_WriteRead()
		{
			SecureSByte v = -100;
			return (sbyte)(v - 1);
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
			return (short)(v + 1);
		}

		[Benchmark(Description = "SecureUShort")]
		public ushort SecureUShort_WriteRead()
		{
			SecureUShort v = 60000;
			return (ushort)(v + 1);
		}

		[Benchmark(Description = "SecureInt")]
		public int SecureInt_WriteRead()
		{
			SecureInt v = 123456;
			return v * 2;
		}

		[Benchmark(Description = "SecureUInt")]
		public uint SecureUInt_WriteRead()
		{
			SecureUInt v = 4000000000;
			return (uint)(long)(v - 1);
		}

		[Benchmark(Description = "SecureLong")]
		public long SecureLong_WriteRead()
		{
			SecureLong v = long.MaxValue;
			return v - 1;
		}

		[Benchmark(Description = "SecureULong")]
		public ulong SecureULong_WriteRead()
		{
			SecureULong v = ulong.MaxValue;
			return v - 1UL;
		}

		[Benchmark(Description = "SecureFloat")]
		public float SecureFloat_WriteRead()
		{
			SecureFloat v = 3.14f;
			return v * 2f;
		}

		[Benchmark(Description = "SecureDouble")]
		public double SecureDouble_WriteRead()
		{
			SecureDouble v = 2.718281828;
			return v * 2.0;
		}

		[Benchmark(Description = "SecureDecimal")]
		public decimal SecureDecimal_WriteRead()
		{
			SecureDecimal v = 1234.56m;
			return v + 1m;
		}

		[Benchmark(Description = "SecureGuid")]
		public Guid SecureGuid_WriteRead()
		{
			SecureGuid v = new Guid("0123456789abcdef0123456789abcdef");
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
		public string SecureString_WriteRead()
		{
			SecureString v = "benchmark secret";
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
