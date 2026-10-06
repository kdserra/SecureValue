#nullable enable
using System;

namespace SecureValue
{
	/// <summary>
	/// Base class for all SecureValue errors. Catch this type to handle
	/// tampered and uninitialized reads together.
	/// </summary>
	public class SecureValueException : Exception
	{
		/// <summary>Initializes a new SecureValueException.</summary>
		public SecureValueException() { }

		/// <summary>Initializes a new SecureValueException with a message.</summary>
		public SecureValueException(string message)
			: base(message) { }

		/// <summary>Initializes a new SecureValueException with a message and inner exception.</summary>
		public SecureValueException(string message, Exception inner)
			: base(message, inner) { }
	}
}
