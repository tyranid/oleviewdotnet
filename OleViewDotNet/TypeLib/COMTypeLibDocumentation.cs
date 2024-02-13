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

using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

internal readonly struct COMTypeLibDocumentation
{
    internal delegate void GetDocumentationDel(int index, out string name, 
        out string doc_string, out int help_context, out string help_file);

    public readonly string Name;
    public readonly string DocString;
    public readonly int HelpContext;
    public readonly string HelpFile;

    public COMTypeLibDocumentation(ITypeLib type_lib, int index = -1) 
    {
        type_lib.GetDocumentation(index, out Name, out DocString, out HelpContext, out HelpFile);
    }

    public COMTypeLibDocumentation(ITypeInfo type_info, int index = -1)
    {
        type_info.GetDocumentation(index, out Name, out DocString, out HelpContext, out HelpFile);
    }

    public IEnumerable<string> GetAttrs()
    {
        List<string> attrs = new();
        if (!string.IsNullOrEmpty(DocString))
            attrs.Add(COMTypeLibUtils.FormatAttr("helpstring", DocString));
        if (HelpContext != 0)
            attrs.Add(COMTypeLibUtils.FormatAttr("helpcontext", HelpContext));
        if (!string.IsNullOrEmpty(HelpFile))
            attrs.Add(COMTypeLibUtils.FormatAttr("helpfile", HelpFile));
        return attrs;
    }
}
