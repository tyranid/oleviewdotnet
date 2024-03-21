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
using OleViewDotNet.Database;
using OleViewDotNet.Processes;
using OleViewDotNet.Security;
using OleViewDotNet.TypeLib;
using OleViewDotNet.Proxy;

namespace OleViewDotNet.Forms;

internal enum FilterDecision
{
    Include,
    Exclude
}

internal enum FilterType
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
    Process,
    Ipid,
    RuntimeClass,
    RuntimeServer,
    TypeLibTypeInfo,
    ProxyTypeInfo,
}

internal enum FilterComparison
{
    Contains,
    Excludes,
    Equals,
    NotEquals,
    StartsWith,
    EndsWith,
}

internal enum FilterResult
{
    None,
    Include,
    Exclude,
}

internal interface IRegistryViewerFilter : IDisposable
{
    FilterResult Filter(string text, object tag);
}

internal class RegistryViewerDisplayFilter : IRegistryViewerFilter
{
    private readonly Func<string, bool> m_filter;

    public RegistryViewerDisplayFilter(Func<string, bool> filter)
    {
        m_filter = filter;
    }

    public void Dispose()
    {
    }

    public FilterResult Filter(string text, object tag)
    {
        return m_filter(text) ? FilterResult.Include : FilterResult.None;
    }
}

internal class RegistryViewerAccessibleFilter : IRegistryViewerFilter
{
    private readonly COMAccessCheck m_access_check;
    private readonly bool m_not_accessible;

    public RegistryViewerAccessibleFilter(COMAccessCheck access_check, bool not_accessible)
    {
        m_access_check = access_check;
        m_not_accessible = not_accessible;
    }

    public void Dispose()
    {
        m_access_check?.Dispose();
    }

    public FilterResult Filter(string text, object tag)
    {
        if (tag is not ICOMAccessSecurity obj)
        {
            return FilterResult.Exclude;
        }

        bool result = m_access_check.AccessCheck(obj);
        if (m_not_accessible)
            result = !result;

        return result ? FilterResult.Include : FilterResult.Exclude;
    }
}

internal class RegistryViewerFilterEntry
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
            if (!t.IsAssignableFrom(entry.GetType()))
            {
                return false;
            }

            PropertyInfo pi = RegistryViewerFilter.GetFieldForTypeAndName(Type, Field);
            object value_obj = pi.GetValue(entry);
            if (value_obj is null)
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

internal class RegistryViewerFilter : IRegistryViewerFilter
{
    public List<RegistryViewerFilterEntry> Filters { get; private set; }

    public RegistryViewerFilter()
    {
        Filters = new List<RegistryViewerFilterEntry>();
    }

    private FilterResult Filter(object entry)
    {
        if (entry is not null)
        {
            foreach (var filter in Filters.Where(f => f.Enabled && f.Decision == FilterDecision.Exclude))
            {
                if (filter.IsMatch(entry))
                {
                    return FilterResult.Exclude;
                }
            }

            var include_filters = Filters.Where(f => f.Enabled && f.Decision == FilterDecision.Include);
            if (!include_filters.Any())
            {
                return FilterResult.Include;
            }

            foreach (var filter in include_filters)
            {
                if (filter.IsMatch(entry))
                {
                    return FilterResult.Include;
                }
            }
        }

        return FilterResult.None;
    }

    public static Type GetTypeForFilter(FilterType type)
    {
        return type switch
        {
            FilterType.AppID => typeof(COMAppIDEntry),
            FilterType.Category => typeof(COMCategory),
            FilterType.CLSID => typeof(COMCLSIDEntry),
            FilterType.Interface => typeof(COMInterfaceEntry),
            FilterType.LowRights => typeof(COMIELowRightsElevationPolicy),
            FilterType.MimeType => typeof(COMMimeType),
            FilterType.ProgID => typeof(COMProgIDEntry),
            FilterType.Server => typeof(COMCLSIDServerEntry),
            FilterType.TypeLib => typeof(COMTypeLibVersionEntry),
            FilterType.Process => typeof(COMProcessEntry),
            FilterType.Ipid => typeof(COMIPIDEntry),
            FilterType.RuntimeClass => typeof(COMRuntimeClassEntry),
            FilterType.RuntimeServer => typeof(COMRuntimeServerEntry),
            FilterType.TypeLibTypeInfo => typeof(COMTypeLibTypeInfo),
            FilterType.ProxyTypeInfo => typeof(COMProxyTypeInfo),
            _ => throw new ArgumentException("Invalid filter type", nameof(type)),
        };
    }

    private static bool IsFilterProperty(PropertyInfo pi)
    {
        Type t = pi.PropertyType;
        return !pi.GetMethod.IsStatic && (t.IsPrimitive || t.IsEnum || t == typeof(Guid) || t == typeof(string));
    }

    public static IEnumerable<PropertyInfo> GetFieldsForType(FilterType type)
    {
        return GetTypeForFilter(type).GetProperties().Where(f => IsFilterProperty(f)).OrderBy(f => f.Name);
    }

    public static PropertyInfo GetFieldForTypeAndName(FilterType type, string name)
    {
        PropertyInfo pi = GetTypeForFilter(type).GetProperty(name);
        if (pi is null || !IsFilterProperty(pi))
        {
            throw new ArgumentException("Invalid field for filter", nameof(name));
        }
        return pi;
    }

    public FilterResult Filter(string text, object tag)
    {
        try
        {
            return Filter(tag);
        }
        catch
        {
            return FilterResult.None;
        }
    }

    public void Dispose()
    {
    }
}
