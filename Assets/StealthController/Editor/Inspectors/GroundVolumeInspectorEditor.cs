using StealthController.Unity;
using UnityEditor;

namespace StealthController.Inspectors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GroundVolumeAdapterBase), true)]
    public class GroundVolumeInspectorEditor : BasicSettingsInspectorEditorBase
    {
    }
}
