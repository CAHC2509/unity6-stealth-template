using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;

namespace StealthController.Inspectors
{
    public abstract class BasicSettingsInspectorEditorBase : UnityEditor.Editor
    {
        private string advancedStateKey;

        private void OnEnable()
        {
            advancedStateKey = StealthInspectorConstants.ADVANCED_FOLDOUT_STATE_KEY_PREFIX + target.GetType().FullName;
        }

        public override void OnInspectorGUI()
        {
            Type targetType = target.GetType();

            if (!BasicSettingsReflectionUtility.HasBasicSettings(targetType))
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            DrawScriptField();

            string lastDrawnHeader = null;
            DrawBasicChildren(GetRootChildren(), targetType, null, ref lastDrawnHeader);

            if (BasicSettingsReflectionUtility.HasAdvancedSettings(targetType))
            {
                DrawAdvancedSection(targetType);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Basic Settings

        private void DrawBasicChildren(List<SerializedProperty> children, Type containerType, string inheritedHeader, ref string lastDrawnHeader)
        {
            string currentHeader = inheritedHeader;

            foreach (SerializedProperty child in children)
            {
                FieldInfo field = BasicSettingsReflectionUtility.FindField(containerType, child.name);
                if (field == null) continue;

                HeaderAttribute header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null) currentHeader = header.header;

                if (BasicSettingsReflectionUtility.IsBasic(field))
                {
                    DrawBasicField(child, field, header != null, currentHeader, ref lastDrawnHeader);
                    continue;
                }

                if (!BasicSettingsReflectionUtility.IsBasicContainer(field)) continue;

                DrawBasicChildren(GetChildren(child), field.FieldType, currentHeader, ref lastDrawnHeader);
            }
        }

        private static void DrawBasicField(SerializedProperty property, FieldInfo field, bool hasOwnHeader, string currentHeader, ref string lastDrawnHeader)
        {
            bool usesDefaultDrawer = !HasSimpleDrawer(property.propertyType);

            if (usesDefaultDrawer && hasOwnHeader)
            {
                lastDrawnHeader = currentHeader;
            }
            else if (currentHeader != null && currentHeader != lastDrawnHeader)
            {
                DrawHeader(currentHeader);
                lastDrawnHeader = currentHeader;
            }

            if (usesDefaultDrawer)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            DrawSimpleField(property, field);
        }

        private static bool HasSimpleDrawer(SerializedPropertyType propertyType)
        {
            return propertyType == SerializedPropertyType.Float
                || propertyType == SerializedPropertyType.Integer
                || propertyType == SerializedPropertyType.Boolean
                || propertyType == SerializedPropertyType.ObjectReference;
        }

        private static void DrawSimpleField(SerializedProperty property, FieldInfo field)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = EditorGUI.BeginProperty(rect, new GUIContent(property.displayName, property.tooltip), property);

            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();
            MinAttribute min = field.GetCustomAttribute<MinAttribute>();

            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    DrawFloatField(rect, property, label, range, min);
                    break;
                case SerializedPropertyType.Integer:
                    DrawIntegerField(rect, property, label, range, min);
                    break;
                case SerializedPropertyType.Boolean:
                    DrawToggleField(rect, property, label);
                    break;
                case SerializedPropertyType.ObjectReference:
                    EditorGUI.ObjectField(rect, property, field.FieldType, label);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private static void DrawFloatField(Rect rect, SerializedProperty property, GUIContent label, RangeAttribute range, MinAttribute min)
        {
            if (range != null)
            {
                EditorGUI.Slider(rect, property, range.min, range.max, label);
                return;
            }

            EditorGUI.BeginChangeCheck();
            float value = EditorGUI.FloatField(rect, label, property.floatValue);
            if (!EditorGUI.EndChangeCheck()) return;

            property.floatValue = min == null ? value : Mathf.Max(min.min, value);
        }

        private static void DrawIntegerField(Rect rect, SerializedProperty property, GUIContent label, RangeAttribute range, MinAttribute min)
        {
            if (range != null)
            {
                EditorGUI.IntSlider(rect, property, Mathf.RoundToInt(range.min), Mathf.RoundToInt(range.max), label);
                return;
            }

            EditorGUI.BeginChangeCheck();
            int value = EditorGUI.IntField(rect, label, property.intValue);
            if (!EditorGUI.EndChangeCheck()) return;

            property.intValue = min == null ? value : Mathf.Max(Mathf.RoundToInt(min.min), value);
        }

        private static void DrawToggleField(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            bool value = EditorGUI.Toggle(rect, label, property.boolValue);
            if (!EditorGUI.EndChangeCheck()) return;

            property.boolValue = value;
        }

        #endregion

        #region Advanced Settings

        private void DrawAdvancedSection(Type targetType)
        {
            EditorGUILayout.Space();

            bool wasExpanded = SessionState.GetBool(advancedStateKey, false);
            bool isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wasExpanded, StealthInspectorConstants.ADVANCED_FOLDOUT_LABEL);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (isExpanded != wasExpanded)
            {
                SessionState.SetBool(advancedStateKey, isExpanded);
            }

            if (!isExpanded) return;

            EditorGUI.indentLevel++;
            DrawAdvancedChildren(GetRootChildren(), targetType);
            EditorGUI.indentLevel--;
        }

        private void DrawAdvancedChildren(List<SerializedProperty> children, Type containerType)
        {
            string pendingHeader = null;

            foreach (SerializedProperty child in children)
            {
                FieldInfo field = BasicSettingsReflectionUtility.FindField(containerType, child.name);

                if (field == null)
                {
                    EditorGUILayout.PropertyField(child, true);
                    continue;
                }

                HeaderAttribute header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null) pendingHeader = header.header;

                if (BasicSettingsReflectionUtility.IsBasic(field)) continue;

                if (BasicSettingsReflectionUtility.IsBasicContainer(field))
                {
                    if (!BasicSettingsReflectionUtility.HasAdvancedSettings(field.FieldType)) continue;

                    DrawPendingHeader(ref pendingHeader);
                    DrawAdvancedContainer(child, field.FieldType);
                    continue;
                }

                if (header != null)
                {
                    pendingHeader = null;
                }
                else
                {
                    DrawPendingHeader(ref pendingHeader);
                }

                EditorGUILayout.PropertyField(child, true);
            }
        }

        private void DrawAdvancedContainer(SerializedProperty container, Type containerType)
        {
            container.isExpanded = EditorGUILayout.Foldout(container.isExpanded, container.displayName, true);
            if (!container.isExpanded) return;

            EditorGUI.indentLevel++;
            DrawAdvancedChildren(GetChildren(container), containerType);
            EditorGUI.indentLevel--;
        }

        private static void DrawPendingHeader(ref string pendingHeader)
        {
            if (pendingHeader == null) return;

            DrawHeader(pendingHeader);
            pendingHeader = null;
        }

        #endregion

        #region Helpers

        private void DrawScriptField()
        {
            SerializedProperty script = serializedObject.FindProperty(StealthInspectorConstants.SCRIPT_PROPERTY_PATH);
            if (script == null) return;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(script);
            }
        }

        private static void DrawHeader(string text)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        private List<SerializedProperty> GetRootChildren()
        {
            List<SerializedProperty> children = new List<SerializedProperty>();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == StealthInspectorConstants.SCRIPT_PROPERTY_PATH) continue;

                children.Add(iterator.Copy());
            }

            return children;
        }

        private static List<SerializedProperty> GetChildren(SerializedProperty parent)
        {
            List<SerializedProperty> children = new List<SerializedProperty>();
            SerializedProperty iterator = parent.Copy();
            SerializedProperty end = parent.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                children.Add(iterator.Copy());
            }

            return children;
        }

        #endregion
    }
}
