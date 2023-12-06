//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2019
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

using OleViewDotNet.Database;
using System;
using System.IO;

namespace OleViewDotNet.Interop.SxS;

public class ActCtxComServerRedirection
{
    public Guid Clsid { get; }
    public Guid ReferenceClsid { get; }
    public Guid ConfiguredClsid { get; }
    public Guid ImplementedClsid { get; }
    public Guid TypeLibraryId { get; }
    public string Module { get; }
    public string FullPath { get; }
    public string ProgId { get; }
    public COMThreadingModel ThreadingModel { get; }

    private static COMThreadingModel FromActCtxThreadingModel(ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL threading_model)
    {
        switch (threading_model)
        {
            case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.FREE:
                return COMThreadingModel.Free;
            case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.BOTH:
                return COMThreadingModel.Both;
            case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.INVALID:
                return COMThreadingModel.None;
            case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.NEUTRAL:
                return COMThreadingModel.Neutral;
            default:
                return COMThreadingModel.Apartment;
        }
    }

    internal ActCtxComServerRedirection(GuidSectionEntry<ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION> entry, ReadHandle handle, int base_offset, int struct_offset)
    {
        Clsid = entry.Key;
        ReferenceClsid = entry.Entry.ReferenceClsid;
        ConfiguredClsid = entry.Entry.ConfiguredClsid;
        ImplementedClsid = entry.Entry.ImplementedClsid;
        TypeLibraryId = entry.Entry.TypeLibraryId;
        Module = handle.ReadString(base_offset + entry.Entry.ModuleOffset, entry.Entry.ModuleLength);
        ProgId = handle.ReadString(struct_offset + entry.Entry.ProgIdOffset, entry.Entry.ProgIdLength);
        ThreadingModel = FromActCtxThreadingModel(entry.Entry.ThreadingModel);
        if (!string.IsNullOrWhiteSpace(entry.RosterEntry.FullPath))
        {
            FullPath = Path.Combine(entry.RosterEntry.FullPath, Module);
        }
        else
        {
            FullPath = Module;
        }
    }
}
