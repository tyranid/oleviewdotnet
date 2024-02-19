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

namespace OleViewDotNet.Utilities.Format;

internal sealed class COMObjectFormatterInternal : COMObjectFormatter
{
    private readonly IDictionary<Guid, string> _iids_to_names;

    internal COMObjectFormatterInternal(COMRegistry registry)
    {
        _iids_to_names = registry.IidNameCache;
    }

    public INdrFormatter GetNdrFormatter()
    {
        static string demangle(string s) => COMUtilities.DemangleWinRTName(s);
        DefaultNdrFormatterFlags flags = Flags.HasFlag(COMObjectFormatterFlags.RemoveComments) ? DefaultNdrFormatterFlags.RemoveComments : 0;
        return Type switch
        {
            COMObjectFormatterType.Generic => DefaultNdrFormatter.Create(_iids_to_names, demangle, flags),
            COMObjectFormatterType.Cpp => CppNdrFormatter.Create(_iids_to_names, demangle, flags),
            COMObjectFormatterType.Idl => IdlNdrFormatter.Create(_iids_to_names, demangle, flags),
            _ => throw new ArgumentException("Unsupported formatting type."),
        };
    }
}
