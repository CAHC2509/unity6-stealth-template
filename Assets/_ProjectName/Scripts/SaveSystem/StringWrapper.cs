using System;
using UnityEngine;

[Serializable]
public class StringWrapper
{
    [SerializeField] private string value;

    public string Value => value;

    public StringWrapper(string value)
    {
        this.value = value;
    }
}
