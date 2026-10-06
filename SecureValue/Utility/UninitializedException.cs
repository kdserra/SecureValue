#nullable enable
using System;

namespace SecureValue
{
	/// <summary>
	/// Thrown when reading a secured value that was never assigned. Assign a
	/// value first. Unlike tampering, this never raises
	/// <see cref="TamperingNotifier.TamperingDetected"/>.
	/// </summary>
	public class UninitializedException : SecureValueException
	{
		/// <summary>Initializes a new UninitializedException.</summary>
		public UninitializedException()
			: base("The secured value was never initialized. Assign a value before reading it.") { }

		/// <summary>Initializes a new UninitializedException with a message.</summary>
		public UninitializedException(string message)
			: base(message) { }

		/// <summary>Initializes a new UninitializedException with a message and inner exception.</summary>
		public UninitializedException(string message, Exception inner)
			: base(message, inner) { }
	}
}
