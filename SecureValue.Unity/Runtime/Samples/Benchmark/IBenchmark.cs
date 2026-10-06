/// <summary>
/// Contract for a benchmarked wrapped-value implementation. Implement one
/// component per implementation (C# primitives, SecureValue, competitors)
/// and drop them on the same GameObject as the BenchmarkRunner.
/// The runner drives all measurement; implementations only expose state.
/// </summary>
public interface IBenchmark
{
	/// <summary>Shown in result logs.</summary>
	string DisplayName { get; }

	/// <summary>Called before this implementation's run (reset state here).</summary>
	void Prepare();

	/// <summary>Stores the int slot under test.</summary>
	void SetInt(int value);

	/// <summary>Reads the int slot under test.</summary>
	int GetInt();

	/// <summary>Stores the float slot under test.</summary>
	void SetFloat(float value);

	/// <summary>Reads the float slot under test.</summary>
	float GetFloat();

	/// <summary>Stores the string slot under test.</summary>
	void SetString(string value);

	/// <summary>Reads the string slot under test.</summary>
	string GetString();
}
