using System;
using UnityEngine;

[Serializable]
public class SensitivitySettingsData
{
    [SerializeField] private float generalSensitivity = Constants.SENSITIVITY_DEFAULT_GENERAL;
    [SerializeField] private bool invertY;

    public float GeneralSensitivity { get => generalSensitivity; set => generalSensitivity = value; }
    public bool InvertY { get => invertY; set => invertY = value; }

    public float InvertYMultiplier => invertY ? -1f : 1f;
}