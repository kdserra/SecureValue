using SecureValue;
using UnityEngine;

/// <summary>SecureValue implementation (memory-encrypted wrapped values).</summary>
public class SecureValueBenchmark : MonoBehaviour, IBenchmark
{
	private SecureInt _valueInt;
	private SecureFloat _valueFloat;
	private SecureString _valueString;

	/// <inheritdoc/>
	public string DisplayName => "SecureValue";

	/// <inheritdoc/>
	public void Prepare()
	{
		_valueInt = 0;
		_valueFloat = 0f;
		_valueString = string.Empty;
	}

	/// <inheritdoc/>
	public void SetInt(int value) => _valueInt = value;

	/// <inheritdoc/>
	public int GetInt() => _valueInt;

	/// <inheritdoc/>
	public void SetFloat(float value) => _valueFloat = value;

	/// <inheritdoc/>
	public float GetFloat() => _valueFloat;

	/// <inheritdoc/>
	public void SetString(string value) => _valueString = value;

	/// <inheritdoc/>
	public string GetString() => _valueString;
}
