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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OleViewDotNet.Utilities.Format;

internal sealed class SourceCodeBuilder
{
    private readonly Stack<string> _indent = new();
    private readonly StringBuilder _builder = new();

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
        foreach (var s in _indent)
        {
            _builder.Append(s);
        }
    }

    public IDisposable PushIndent(int count)
    {
        return new StackPopper(_indent, count);
    }

    public void AppendLine(string line)
    {
        AddIndent();
        _builder.AppendLine(line);
    }

    public void AppendLine()
    {
        _builder.AppendLine();
    }

    public void AppendList(IEnumerable<string> lines)
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

    public void AppendAttributes(IEnumerable<string> lines, int indent = 4)
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

    public override string ToString()
    {
        return _builder.ToString();
    }
}
