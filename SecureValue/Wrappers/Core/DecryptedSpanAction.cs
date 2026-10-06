#nullable enable
using System;

namespace SecureValue
{
	/// <summary>
	/// Handles a decrypted character span (e.g. inside <c>SecureString.StackDecrypt</c>).
	/// The span is stack-allocated and zeroed after the call returns.
	/// </summary>
	/// <param name="decrypted">The decrypted characters. Do not store or capture this span.</param>
	public delegate void DecryptedSpanAction(ReadOnlySpan<char> decrypted);

	/// <summary>
	/// Handles a decrypted character span with additional state (e.g. inside
	/// <c>SecureString.StackDecrypt</c>). The span is stack-allocated and zeroed
	/// after the call returns. Prefer this overload over capturing locals: a
	/// <c>static</c> lambda with explicit state stays allocation-free, while a
	/// capturing lambda allocates a closure.
	/// </summary>
	/// <typeparam name="TState">The state type.</typeparam>
	/// <param name="decrypted">The decrypted characters. Do not store or capture this span.</param>
	/// <param name="state">Caller-provided state.</param>
	public delegate void DecryptedSpanAction<TState>(ReadOnlySpan<char> decrypted, TState state);
}
