#nullable enable
namespace SecureValue
{
	/// <summary>
	/// Bridge to the serialized storage form (decrypt/display/edit/re-encrypt).
	/// Implemented explicitly by every wrapper; members stay non-public.
	/// </summary>
	public interface ISecureSerialization
	{
		/// <summary>True when never assigned.</summary>
		bool IsUnset { get; }

		/// <summary>
		/// Exports the value in portable storage form (fresh storage key, storage MAC),
		/// so saves load in any process. Never exposes plaintext.
		/// </summary>
		uint[] SaveToSerialized();

		/// <summary>
		/// Restores storage-form words from <see cref="SaveToSerialized"/> (verifying the
		/// storage MAC and re-encrypting with the process key). A null or wrong-length
		/// payload leaves the value unset (reads return default); tampered saves throw on next read.
		/// </summary>
		/// <param name="packed">Storage-form words, or null when never serialized.</param>
		void LoadFromSerialized(uint[]? packed);
	}
}
