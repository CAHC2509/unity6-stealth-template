using System;
using UnityEngine;

[Serializable]
public class UITransitionDataBase
{
    [SerializeField, Min(0f)] private float showDuration = 0.2f;
    [SerializeField, Min(0f)] private float hideDuration = 0.1f;

    public float ShowDuration => showDuration;
    public float HideDuration => hideDuration;
}
