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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Interop;

internal class TypeLibCallback : ITypeLibImporterNotifySink
{
    public Assembly ResolveRef(object tl)
    {
        return COMUtilities.ConvertTypeLibToAssembly((ITypeLib)tl, _progress);
    }

    public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
    {
        if (eventKind == ImporterEventKind.NOTIF_TYPECONVERTED && _progress is not null)
        {
            _progress.Report(new Tuple<string, int>(eventMsg, -1));
        }
    }

    public TypeLibCallback(IProgress<Tuple<string, int>> progress)
    {
        _progress = progress;
    }

    private readonly IProgress<Tuple<string, int>> _progress;
}
