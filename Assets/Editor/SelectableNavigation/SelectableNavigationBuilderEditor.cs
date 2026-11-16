#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SelectableNavigationBuilderBase), true)]
public class SelectableNavigationBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Rebuild Navigation"))
        {
            SelectableNavigationBuilderBase builder = (SelectableNavigationBuilderBase)target;
            builder.RebuildNavigation();
        }
    }
}

#endif