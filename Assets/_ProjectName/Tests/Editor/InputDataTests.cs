using NUnit.Framework;
using UnityEngine;

public class InputDataTests
{
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void InputData_ButtonReportsPressedState(bool pressed, bool expected)
    {
        InputData data = new InputData(pressed);

        Assert.AreEqual(InputValueType.Button, data.ValueType);
        Assert.AreEqual(expected, data.IsPressed);
    }

    [TestCase(0.5f, true)]
    [TestCase(0f, false)]
    public void InputData_FloatReportsPressedWhenPositive(float value, bool expected)
    {
        InputData data = new InputData(value);

        Assert.AreEqual(InputValueType.Float, data.ValueType);
        Assert.AreEqual(expected, data.IsPressed);
    }

    [Test]
    public void InputData_Vector2ReportsPressedWhenNotZero()
    {
        InputData movingData = new InputData(new Vector2(-1f, 0f));
        InputData idleData = new InputData(Vector2.zero);

        Assert.AreEqual(InputValueType.Vector2, movingData.ValueType);
        Assert.IsTrue(movingData.IsPressed);
        Assert.IsFalse(idleData.IsPressed);
    }
}
