using UnityEngine;

/// <summary>Baseline: plain C# fields, no security.</summary>
public class PrimitiveBenchmark : MonoBehaviour, IBenchmark
{
	private int _valueInt;
	private float _valueFloat;
	private string _valueString;

	/// <inheritdoc/>
	public string DisplayName => "C# Primitive";

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
