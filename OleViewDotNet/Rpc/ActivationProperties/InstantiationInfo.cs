//    Copyright (C) James Forshaw 2024
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
using OleViewDotNet.Interop;
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class InstantiationInfo : IActivationProperty
{
    private readonly List<Guid> m_iids = new();
    private InstantiationInfoData m_inner = new();

    public Guid PropertyClsid => new("{000001ab-0000-0000-c000-000000000046}");

    public Guid ClassId
    {
        get => m_inner.classId;
        set => m_inner.classId = value;
    }
    public CLSCTX ClassCtx
    {
        get => (CLSCTX)m_inner.classCtx;
        set => m_inner.classCtx = (int)value;
    }
    public ACTIVATION_FLAGS ActivationFlags
    {
        get => (ACTIVATION_FLAGS)m_inner.actvflags;
        set => m_inner.actvflags = (int)value;
    }

    public bool IsSurrogate
    {
        get => m_inner.fIsSurrogate != 0;
        set => m_inner.fIsSurrogate = value ? 1 : 0;
    }
    public int InstFlag
    {
        get => m_inner.instFlag;
        set => m_inner.instFlag = value;
    }
    public List<Guid> IIds => m_iids;
    public COMVersion ClientCOMVersion
    {
        get => m_inner.clientCOMVersion.ToVersion();
        set => m_inner.clientCOMVersion = value.ToVersion();
    }
}
