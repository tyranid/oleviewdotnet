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

namespace OleViewDotNet.Utilities.Format;

internal class COMSourceCodeEditableObject : ICOMSourceCodeEditable
{
    private readonly Func<string> m_get_name;
    private readonly Action<string> m_set_name;
    private readonly IReadOnlyList<ICOMSourceCodeEditable> m_members;

    public COMSourceCodeEditableObject(Func<string> get_name, Action<string> set_name, IEnumerable<ICOMSourceCodeEditable> members = null)
    {
        m_get_name = get_name;
        m_set_name = set_name;
        m_members = new List<ICOMSourceCodeEditable>(members ?? Array.Empty<ICOMSourceCodeEditable>()).AsReadOnly();
    }

    string ICOMSourceCodeEditable.Name { get =>  m_get_name(); set => m_set_name(value); }

    IReadOnlyList<ICOMSourceCodeEditable> ICOMSourceCodeEditable.Members => m_members;
}