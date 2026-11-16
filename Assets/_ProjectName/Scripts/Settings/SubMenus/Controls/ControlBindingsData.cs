using System;
using UnityEngine;

[Serializable]
public class ControlBindingsData
{
    [SerializeField] private string bindingsJson;

    public string BindingsJson { get => bindingsJson; set => bindingsJson = value; }
}