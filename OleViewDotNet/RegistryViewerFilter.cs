//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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

using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace OleViewDotNet
{
    public enum FilterDecision
    {
        Include,
        Exclude
    }

    public enum FilterType
    {
        CLSID,
        AppID,
        Category,
        Interface,
        ProgID,
        TypeLib,
        LowRights,
        MimeType,
        Server,
    }

    public enum FilterComparison
    {
        Contains,
        Excludes,
        Equals,
        NotEquals,
        StartsWith,
        EndsWith,
    }

    public class RegistryViewerFilterEntry
    {
        public FilterDecision Decision { get; set; }
        public FilterType Type { get; set; }
        public FilterComparison Comparison { get; set; }
        public string Field { get; set; }
        public bool Enabled { get; set; }
        public string Value { get; set; }

        public RegistryViewerFilterEntry Clone()
        {
            return (RegistryViewerFilterEntry)MemberwiseClone();
        }
        
        public bool IsMatch(object entry)
        {
            try
            {
                Type t = RegistryViewerFilter.GetTypeForFilter(Type);
                if (t != entry.GetType())
                {
                    return false;
                }

                FieldInfo fi = RegistryViewerFilter.GetFieldForTypeAndName(Type, Field);
                object value_obj = fi.GetValue(entry);
                if (value_obj == null)
                {
                    return false;
                }

                string value = value_obj.ToString().ToLower();
                string value_compare = Value.ToLower();
                switch (Comparison)
                {
                    case FilterComparison.Contains:
                        return value.Contains(value_compare);
                    case FilterComparison.EndsWith:
                        return value.EndsWith(value_compare);
                    case FilterComparison.Equals:
                        return value.Equals(value_compare);
                    case FilterComparison.Excludes:
                        return !value.Contains(value_compare);
                    case FilterComparison.NotEquals:
                        return !value.Equals(value_compare);
                    case FilterComparison.StartsWith:
                        return value.StartsWith(value_compare);
                }
            }
            catch(ArgumentException)
            {
                return false;
            }

            return false;
        }
    }

    public class RegistryViewerFilter
    {
        public List<RegistryViewerFilterEntry> Filters { get; private set; }

        public RegistryViewerFilter()
        {
            Filters = new List<RegistryViewerFilterEntry>();
        }

        public bool Include(object entry)
        {
            if (entry == null)
            {
                return false;
            }

            foreach (var filter in Filters.Where(f => f.Enabled && f.Decision == FilterDecision.Exclude))
            {
                if (filter.IsMatch(entry))
                {
                    return false;
                }
            }

            foreach (var filter in Filters.Where(f => f.Enabled && f.Decision == FilterDecision.Include))
            {
                if (filter.IsMatch(entry))
                {
                    return true;
                }
            }

            return false;
        }

        public static Type GetTypeForFilter(FilterType type)
        {
            switch (type)
            {
                case FilterType.AppID:
                    return typeof(COMAppIDEntry);
                case FilterType.Category:
                    return typeof(COMCategory);
                case FilterType.CLSID:
                    return typeof(COMCLSIDEntry);
                case FilterType.Interface:
                    return typeof(COMInterfaceEntry);
                case FilterType.LowRights:
                    return typeof(COMIELowRightsElevationPolicy);
                case FilterType.MimeType:
                    return typeof(COMMimeType);
                case FilterType.ProgID:
                    return typeof(COMProgIDEntry);
                case FilterType.Server:
                    return typeof(COMCLSIDServerEntry);
                case FilterType.TypeLib:
                    return typeof(COMTypeLibVersionEntry);
                default:
                    throw new ArgumentException("Invalid filter type", nameof(type));
            }
        }

        private static bool IsFilterField(FieldInfo fi)
        {
            Type t = fi.FieldType;
            return t.IsPrimitive || t.IsEnum || t == typeof(Guid) || t == typeof(string);
        }

        public static IEnumerable<FieldInfo> GetFieldsForType(FilterType type)
        {
            return GetTypeForFilter(type).GetFields().Where(f => IsFilterField(f));
        }

        public static FieldInfo GetFieldForTypeAndName(FilterType type, string name)
        {
            FieldInfo fi = GetTypeForFilter(type).GetField(name);
            if (fi == null || !IsFilterField(fi))
            {
                throw new ArgumentException("Invalid field for filter", nameof(name));
            }
            return fi;
        }
    }
}
