#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureValue
{
	/// <summary>
	/// Global notification for detected memory tampering. Raised just before
	/// a read throws <see cref="TamperedException"/>. Unset reads return
	/// default instead and never raise this.
	/// </summary>
	public static class TamperingNotifier
	{
		private static Action? _handlers;
		private static int _dispatchCount;

		/// <summary>MAC-verified detection state.</summary>
		private static uint _detectionHash = H(false);

		/// <summary>Per-process CSPRNG domain salt for the detection codes. </summary>
		private static readonly ulong _domainSalt = Keys.NewSaltWord();

		/// <summary>Maximum tamper-event dispatches per process run (storm bound).</summary>
		private const int MaxTamperDispatches = 1;

		/// <summary>Keyed state code.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint H(bool detected) =>
			Mac.ComputeTag(detected ? 1UL : 0UL, Vault.DeriveProcessTagKey(_domainSalt));

		/// <summary>Gets whether tampering has been detected at any point during the process run.</summary>
		public static bool HasDetectedTampering
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Volatile.Read(ref _detectionHash) != H(false);
		}

		/// <summary>
		/// Raised when tampering is detected on any secured value.
		/// Delivery is capped at 1 dispatch per process run as a storm bound;
		/// reads still throw <see cref="TamperedException"/> when capped.
		/// Subscription is thread-safe: concurrent += / -= never lose a handler.
		/// Listeners may be removed (test teardown, domain reload); on Unity the
		/// handlers are additionally cleared automatically (see ResetStaticState).
		/// </summary>
		public static event Action TamperingDetected
		{
			add
			{
				Action? current,
					combined;
				do
				{
					current = _handlers;
					combined = (Action?)Delegate.Combine(current, value);
				} while (Interlocked.CompareExchange(ref _handlers, combined, current) != current);
			}
			remove
			{
				Action? current,
					combined;
				do
				{
					current = _handlers;
					combined = (Action?)Delegate.Remove(current, value);
				} while (Interlocked.CompareExchange(ref _handlers, combined, current) != current);
			}
		}

#if UNITY_5_3_OR_NEWER
		/// <summary>
		/// Clears static state (Unity Editor support).
		/// </summary>
		[UnityEngine.RuntimeInitializeOnLoadMethod(
			UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad
		)]
		private static void ResetStaticState()
		{
			_handlers = null;
			_dispatchCount = 0;
			Volatile.Write(ref _detectionHash, H(false));
			// Don't reset the domain salt.
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Raise()
		{
			Volatile.Write(ref _detectionHash, H(true));
			if (Interlocked.Increment(ref _dispatchCount) <= MaxTamperDispatches)
			{
				_handlers?.Invoke();
			}
		}

		/// <summary>Internal test hook: resets the dispatch throttle and detection flag.</summary>
		internal static void ResetThrottleForTesting()
		{
			Interlocked.Exchange(ref _dispatchCount, 0);
			Volatile.Write(ref _detectionHash, H(false));
		}
	}
}
