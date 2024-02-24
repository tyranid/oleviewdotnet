//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

using NtApiDotNet.Ndr;
using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OleViewDotNet.Utilities.Format;

public sealed class COMSourceCodeBuilder
{
    #region Private Members
    private readonly Stack<string> m_indent = new();
    private readonly StringBuilder m_builder = new();
    private readonly COMRegistry m_registry;

    private class StackPopper : IDisposable
    {
        private readonly Stack<string> _indent;

        public StackPopper(Stack<string> indent, int count)
        {
            _indent = indent;
            _indent.Push(new string(' ', count));
        }

        void IDisposable.Dispose()
        {
            _indent.Pop();
        }
    }

    private void AddIndent()
    {
        foreach (var s in m_indent)
        {
            m_builder.Append(s);
        }
    }
    #endregion

    #region Public Properties
    public COMSourceCodeBuilderType OutputType { get; set; }
    public bool RemoveComments { get; set; }
    public bool RemoveComplexTypes { get; set; }
    #endregion

    #region Constructors
    public COMSourceCodeBuilder(
        COMRegistry registry = null)
    {
        m_registry = registry;
    }
    #endregion

    #region Internal Methods
    internal INdrFormatter GetNdrFormatter()
    {
        DefaultNdrFormatterFlags flags = 0;

        if (RemoveComments)
        {
            flags |= DefaultNdrFormatterFlags.RemoveComments;
        }

        return OutputType switch
        {
            COMSourceCodeBuilderType.Idl => IdlNdrFormatter.Create(m_registry?.IidNameCache, s => COMUtilities.DemangleWinRTName(s), flags),
            COMSourceCodeBuilderType.Generic => DefaultNdrFormatter.Create(m_registry?.IidNameCache, s => COMUtilities.DemangleWinRTName(s), flags),
            COMSourceCodeBuilderType.Cpp => CppNdrFormatter.Create(m_registry?.IidNameCache, s => COMUtilities.DemangleWinRTName(s), flags),
            _ => throw new ArgumentException("Invalid output type."),
        };
    }

    internal IDisposable PushIndent(int count)
    {
        return new StackPopper(m_indent, count);
    }

    internal void AppendLine(string line)
    {
        AddIndent();
        m_builder.AppendLine(line);
    }

    internal void AppendLine()
    {
        m_builder.AppendLine();
    }

    internal void AppendList(IEnumerable<string> lines)
    {
        var ls = lines.ToArray();
        if (ls.Length == 0)
            return;
        for (int i = 0; i < ls.Length - 1; ++i)
        {
            AppendLine($"{ls[i]},");
        }
        AppendLine(ls[ls.Length - 1]);
    }

    internal void AppendAttributes(IEnumerable<string> lines, int indent = 4)
    {
        if (!lines.Any())
            return;
        AppendLine("[");
        using (PushIndent(indent))
        {
            AppendList(lines);
        }
        AppendLine("]");
    }

    internal void AppendObjects(IEnumerable<ICOMSourceCodeFormattable> list)
    {
        foreach (var obj in list)
        {
            obj.Format(this);
            m_builder.AppendLine();
        }
    }

    internal void AppendTypes(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            if (type is not ICOMSourceCodeFormattable formattable)
            {
                formattable = new SourceCodeFormattableType(type);
            }
            formattable.Format(this);
            m_builder.AppendLine();
        }
    }

    internal void AppendCommentLine(string comment)
    {
        if (RemoveComments)
            return;
        AppendLine(comment);
    }
    #endregion

    #region Public Methods
    public void Reset()
    {
        m_builder.Clear();
        m_indent.Clear();
    }

    public override string ToString()
    {
        return m_builder.ToString();
    }
    #endregion
}
