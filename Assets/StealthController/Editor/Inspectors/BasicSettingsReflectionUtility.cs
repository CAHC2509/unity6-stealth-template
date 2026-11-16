using System.Collections.Generic;
using System.Reflection;
using System;
using StealthController.Core;
using UnityEngine;

namespace StealthController.Inspectors
{
    public static class BasicSettingsReflectionUtility
    {
        private const BindingFlags DECLARED_INSTANCE_FIELDS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, bool> basicSettingsCache = new Dictionary<Type, bool>();
        private static readonly Dictionary<Type, bool> advancedSettingsCache = new Dictionary<Type, bool>();
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static FieldInfo FindField(Type type, string fieldName)
        {
            if (!fieldCache.TryGetValue(type, out Dictionary<string, FieldInfo> fields))
            {
                fields = new Dictionary<string, FieldInfo>();
                fieldCache.Add(type, fields);
            }

            if (fields.TryGetValue(fieldName, out FieldInfo cachedField)) return cachedField;

            FieldInfo field = null;
            for (Type current = type; current != null && field == null; current = current.BaseType)
            {
                field = current.GetField(fieldName, DECLARED_INSTANCE_FIELDS);
            }

            fields.Add(fieldName, field);
            return field;
        }

        public static bool IsBasic(FieldInfo field)
        {
            return field.IsDefined(typeof(BasicSettingAttribute), false);
        }

        public static bool IsBasicContainer(FieldInfo field)
        {
            return IsSettingsContainer(field.FieldType) && HasBasicSettings(field.FieldType);
        }

        public static bool HasBasicSettings(Type type)
        {
            if (basicSettingsCache.TryGetValue(type, out bool cached)) return cached;

            basicSettingsCache[type] = false;

            bool result = false;
            foreach (FieldInfo field in GetSerializedFields(type))
            {
                if (!IsBasic(field) && !IsBasicContainer(field)) continue;

                result = true;
                break;
            }

            basicSettingsCache[type] = result;
            return result;
        }

        public static bool HasAdvancedSettings(Type type)
        {
            if (advancedSettingsCache.TryGetValue(type, out bool cached)) return cached;

            advancedSettingsCache[type] = false;

            bool result = false;
            foreach (FieldInfo field in GetSerializedFields(type))
            {
                if (IsBasic(field)) continue;
                if (IsBasicContainer(field) && !HasAdvancedSettings(field.FieldType)) continue;

                result = true;
                break;
            }

            advancedSettingsCache[type] = result;
            return result;
        }

        private static bool IsSettingsContainer(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type.IsArray || type.IsGenericType) return false;
            if (type == typeof(string)) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;

            return type.IsDefined(typeof(SerializableAttribute), false);
        }

        private static IEnumerable<FieldInfo> GetSerializedFields(Type type)
        {
            for (Type current = type; current != null && current != typeof(MonoBehaviour) && current != typeof(object); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(DECLARED_INSTANCE_FIELDS))
                {
                    if (field.IsNotSerialized) continue;
                    if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), false)) continue;

                    yield return field;
                }
            }
        }
    }
}
