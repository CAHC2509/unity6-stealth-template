using StealthController.Unity;
using UnityEditor;

namespace StealthController.Inspectors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StealthBehaviourBase), true)]
    public class StealthBehaviourInspectorEditor : BasicSettingsInspectorEditorBase
    {
    }
}
