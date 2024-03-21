//    Copyright (C) James Forshaw 2014. 2016
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

using NtApiDotNet;
using OleViewDotNet.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Utilities;

internal static class XmlUtils
{
    internal static void WriteSerializableObjects<T>(this XmlWriter writer, string name, IEnumerable<T> objs) where T : IXmlSerializable
    {
        writer.WriteStartElement(name);
        foreach (IXmlSerializable e in objs)
        {
            writer.WriteStartElement(typeof(T).Name);
            e.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    internal static IEnumerable<T> ReadSerializableObjects<T>(this XmlReader reader, string name, Func<T> factory) where T : IXmlSerializable
    {
        List<T> ret = new();

        while (reader.NodeType != XmlNodeType.Element || reader.LocalName != name)
        {
            if (!reader.Read())
            {
                return new T[0];
            }
        }

        if (reader.IsStartElement(name))
        {
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                do
                {
                    T obj = factory();
                    obj.ReadXml(reader);
                    ret.Add(obj);
                }
                while (reader.ReadToNextSibling(typeof(T).Name));
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }
        }
        return ret;
    }

    private class XmlNameValuePair : IXmlSerializable
    {
        public string Name;
        public string Value;

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        public XmlNameValuePair()
        {
        }

        public XmlNameValuePair(KeyValuePair<string, string> pair)
        {
            Name = pair.Key;
            Value = pair.Value;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Name = reader.ReadString("n");
            Value = reader.ReadString("v");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("n", Name);
            writer.WriteOptionalAttributeString("v", Value);
        }
    }

    internal static Dictionary<string, string> ReadDictionary(this XmlReader reader, string name)
    {
        return reader.ReadSerializableObjects(name, () => new XmlNameValuePair()).ToDictionary(p => p.Name, p => p.Value);
    }

    internal static void WriteDictionary(this XmlWriter writer, IReadOnlyDictionary<string, string> dict, string name)
    {
        writer.WriteSerializableObjects(name, dict.Select(p => new XmlNameValuePair(p)));
    }

    internal static void WriteGuids(this XmlWriter writer, string name, IEnumerable<Guid> guids)
    {
        if (guids.Any())
        {
            writer.WriteAttributeString(name, string.Join(",", guids.Select(g => g.ToString())));
        }
    }

    internal static IEnumerable<Guid> ReadGuids(this XmlReader reader, string name)
    {
        string guids = reader.GetAttribute(name);
        if (guids is null)
        {
            return new Guid[0];
        }

        return guids.Split(',').Select(s => new Guid(s));
    }

    internal static void WriteGuid(this XmlWriter writer, string name, Guid g)
    {
        if (g != Guid.Empty)
        {
            writer.WriteAttributeString(name, g.ToString());
        }
    }

    internal static Guid ReadGuid(this XmlReader reader, string name)
    {
        string value = reader.GetAttribute(name);
        if (value is null || !Guid.TryParse(value, out Guid guid))
        {
            return Guid.Empty;
        }
        return guid;
    }

    internal static void WriteOptionalAttributeString(this XmlWriter writer, string name, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            writer.WriteAttributeString(name, value.CleanupXmlString());
        }
    }

    internal static void CheckName(this XmlReader reader, string name)
    {
        if (reader.NodeType != XmlNodeType.Element || reader.LocalName != name)
        {
            throw new XmlException("Unexpected Node");
        }
    }

    internal static int ReadInt(this XmlReader reader, string name)
    {
        string value = reader.GetAttribute(name);
        if (value is null)
        {
            return 0;
        }
        return int.Parse(value);
    }

    internal static void WriteInt(this XmlWriter writer, string name, int value)
    {
        if (value != 0)
        {
            writer.WriteAttributeString(name, value.ToString());
        }
    }

    internal static long ReadLong(this XmlReader reader, string name)
    {
        string value = reader.GetAttribute(name);
        if (value is null)
        {
            return 0;
        }
        return long.Parse(value);
    }

    internal static void WriteLong(this XmlWriter writer, string name, long value)
    {
        if (value != 0)
        {
            writer.WriteAttributeString(name, value.ToString());
        }
    }

    internal static void WriteEnum(this XmlWriter writer, string name, Enum value)
    {
        writer.WriteAttributeString(name, value.ToString());
    }

    internal static void WriteSecurityDescriptor(this XmlWriter writer, string name, COMSecurityDescriptor sd)
    {
        if (sd is null)
            return;
        writer.WriteOptionalAttributeString(name, sd.ToBase64());
    }

    internal static T ReadEnum<T>(this XmlReader reader, string name) where T : struct
    {
        string value = reader.GetAttribute(name);
        if (value is null)
        {
            return default;
        }

        if (!Enum.TryParse(value, true, out T ret))
        {
            return default;
        }
        return ret;
    }

    internal static COMSecurityDescriptor ReadSecurityDescriptor(this XmlReader reader, string name)
    {
        string value = reader.ReadString(name);
        if (string.IsNullOrEmpty(value))
            return null;
        var sd = SecurityDescriptor.ParseBase64(value, false);
        if (sd.IsSuccess)
            return new(sd.Result);
        sd = SecurityDescriptor.Parse(value, false);
        if (sd.IsSuccess)
            return new(sd.Result);
        return null;
    }

    internal static void WriteBool(this XmlWriter writer, string name, bool value)
    {
        if (value)
        {
            writer.WriteAttributeString(name, value.ToString());
        }
    }

    internal static bool ReadBool(this XmlReader reader, string name)
    {
        string value = reader.GetAttribute(name);
        if (value is null)
        {
            return false;
        }

        return bool.Parse(value);
    }

    internal static string ReadString(this XmlReader reader, string name)
    {
        return reader.GetAttribute(name) ?? string.Empty;
    }

    internal static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<TValue> enumerable, Func<TValue, TKey> key_selector, IComparer<TKey> comparer)
    {
        return new SortedDictionary<TKey, TValue>(enumerable.ToDictionary(key_selector), comparer);
    }

    internal static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<TValue> enumerable, Func<TValue, TKey> key_selector)
    {
        return enumerable.ToSortedDictionary(key_selector, Comparer<TKey>.Default);
    }

    internal static SortedDictionary<TKey, TElement> ToSortedDictionary<TKey, TValue, TElement>(this IEnumerable<TValue> enumerable,
        Func<TValue, TKey> key_selector, Func<TValue, TElement> value_selector)
    {
        return new SortedDictionary<TKey, TElement>(enumerable.ToDictionary(key_selector, value_selector));
    }

    internal static SortedDictionary<TKey, TElement> ToSortedDictionary<TKey, TValue, TElement>(this IEnumerable<TValue> enumerable,
            Func<TValue, TKey> key_selector, Func<TValue, TElement> value_selector, IComparer<TKey> comparer)
    {
        return new SortedDictionary<TKey, TElement>(enumerable.ToDictionary(key_selector, value_selector), comparer);
    }

    internal static string CleanupXmlString(this string str)
    {
        return new string(str.Where(c => c >= ' ' && !char.IsSurrogate(c) && c != 0xFFFF).ToArray());
    }
}
