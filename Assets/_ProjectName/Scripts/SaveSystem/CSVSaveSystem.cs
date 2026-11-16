using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CSVSaveSystem : FileSaveSystemBase
{
    private const char SEPARATOR = ',';
    private const char QUOTE = '"';
    private const char RECORD_END = '\n';
    private const char CARRIAGE_RETURN = '\r';

    private static readonly char[] charactersRequiringQuotes = { SEPARATOR, QUOTE, RECORD_END, CARRIAGE_RETURN };

    protected override string FileExtension => "csv";

    protected override void Write<T>(string key, T data)
    {
        string path = GetPath(key);

        if (data is string text)
        {
            WriteSafe(path, text);
            return;
        }

        if (data is StringWrapper wrapper)
        {
            WriteSafe(path, wrapper.Value);
            return;
        }

        WriteSafe(path, data is IEnumerable enumerable ? SerializeCollection(enumerable) : SerializeObject(data));
    }

    protected override T Read<T>(string key)
    {
        string content = File.ReadAllText(GetPath(key));

        if (typeof(T) == typeof(string))
            return (T)(object)content;

        if (typeof(T) == typeof(StringWrapper))
            return (T)(object)new StringWrapper(content);

        List<string[]> records = ParseRecords(content);

        if (records.Count < Constants.CSV_MIN_RECORDS_WITH_DATA)
            return CreateDefault<T>();

        Type type = typeof(T);

        if (!typeof(IEnumerable).IsAssignableFrom(type) || !type.IsGenericType)
            return (T)PopulateInstance(type, GetSerializableFields(type), records[1]);

        Type elementType = type.GetGenericArguments()[0];
        FieldInfo[] fields = GetSerializableFields(elementType);
        IList list = (IList)Activator.CreateInstance(type);

        for (int i = 1; i < records.Count; i++)
            list.Add(PopulateInstance(elementType, fields, records[i]));

        return (T)list;
    }

    private string SerializeCollection(IEnumerable collection)
    {
        IEnumerator enumerator = collection.GetEnumerator();

        if (!enumerator.MoveNext())
            return string.Empty;

        FieldInfo[] fields = GetSerializableFields(enumerator.Current.GetType());
        StringBuilder builder = new();

        AppendRecord(builder, fields.Select(f => f.Name));

        do
        {
            object item = enumerator.Current;
            AppendRecord(builder, fields.Select(f => f.GetValue(item)));
        }
        while (enumerator.MoveNext());

        return builder.ToString();
    }

    private string SerializeObject(object data)
    {
        FieldInfo[] fields = GetSerializableFields(data.GetType());
        StringBuilder builder = new();

        AppendRecord(builder, fields.Select(f => f.Name));
        AppendRecord(builder, fields.Select(f => f.GetValue(data)));

        return builder.ToString();
    }

    private void AppendRecord<TValue>(StringBuilder builder, IEnumerable<TValue> values)
    {
        builder.Append(string.Join(SEPARATOR, values.Select(v => Escape(v))));
        builder.Append(RECORD_END);
    }

    private List<string[]> ParseRecords(string content)
    {
        List<string[]> records = new();
        List<string> fields = new();
        StringBuilder field = new();
        bool insideQuotes = false;

        for (int i = 0; i < content.Length; i++)
        {
            char current = content[i];

            if (insideQuotes)
            {
                if (current != QUOTE)
                {
                    field.Append(current);
                    continue;
                }

                bool isEscapedQuote = i + 1 < content.Length && content[i + 1] == QUOTE;

                if (isEscapedQuote)
                {
                    field.Append(QUOTE);
                    i++;
                    continue;
                }

                insideQuotes = false;
                continue;
            }

            switch (current)
            {
                case QUOTE:
                    insideQuotes = true;
                    break;

                case SEPARATOR:
                    fields.Add(field.ToString());
                    field.Clear();
                    break;

                case RECORD_END:
                    fields.Add(field.ToString());
                    field.Clear();
                    records.Add(fields.ToArray());
                    fields.Clear();
                    break;

                case CARRIAGE_RETURN:
                    break;

                default:
                    field.Append(current);
                    break;
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            records.Add(fields.ToArray());
        }

        return records;
    }

    private object PopulateInstance(Type type, FieldInfo[] fields, string[] values)
    {
        object instance = CreateInstance(type);

        for (int i = 0; i < fields.Length && i < values.Length; i++)
        {
            if (string.IsNullOrEmpty(values[i])) continue;

            fields[i].SetValue(instance, ConvertValue(values[i], fields[i].FieldType));
        }

        return instance;
    }

    private FieldInfo[] GetSerializableFields(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
            .ToArray();
    }

    private object CreateInstance(Type type)
    {
        ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
        return ctor != null
            ? Activator.CreateInstance(type)
            : FormatterServices.GetUninitializedObject(type);
    }

    private object ConvertValue(string value, Type targetType)
    {
        try
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }

    private string Escape(object value)
    {
        if (value == null) return string.Empty;

        string text = Convert.ToString(value, CultureInfo.InvariantCulture);

        if (text.IndexOfAny(charactersRequiringQuotes) < 0) return text;

        return $"{QUOTE}{text.Replace("\"", "\"\"")}{QUOTE}";
    }
}
