using UnityEngine;

public readonly struct InputData
{
    public InputValueType ValueType { get; }
    public float FloatValue { get; }
    public Vector2 Vector2Value { get; }
    public bool IsAnalog { get; }
    public bool IsPressed => ValueType == InputValueType.Vector2 ? Vector2Value.sqrMagnitude > 0f : FloatValue > 0f;

    public InputData(float value)
    {
        ValueType = InputValueType.Float;
        FloatValue = value;
        Vector2Value = default;
        IsAnalog = false;
    }

    public InputData(Vector2 value) : this(value, false) { }

    public InputData(Vector2 value, bool isAnalog)
    {
        ValueType = InputValueType.Vector2;
        Vector2Value = value;
        FloatValue = 0f;
        IsAnalog = isAnalog;
    }

    public InputData(bool pressed)
    {
        ValueType = InputValueType.Button;
        FloatValue = pressed ? 1f : 0f;
        Vector2Value = default;
        IsAnalog = false;
    }
}