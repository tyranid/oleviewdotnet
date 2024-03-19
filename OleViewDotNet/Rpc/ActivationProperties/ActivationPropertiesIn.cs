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

using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ActivationPropertiesIn : ActivationProperties
{
    public ActivationPropertiesIn()
    {
        Properties.Add(new InstantiationInfo());
        Properties.Add(new ScmRequestInfo());
        Properties.Add(new LocationInfo());
    }

    internal ActivationPropertiesIn(COMObjRefCustom objref)
        : base(objref)
    {
    }

    public void SetActivationFlags(ACTIVATION_FLAGS flags)
    {
        FindOrCreateProperty<InstantiationInfo>().ActivationFlags = flags;
    }

    public void SetClass(Guid clsid, CLSCTX clsctx = CLSCTX.LOCAL_SERVER)
    {
        var info = FindOrCreateProperty<InstantiationInfo>();
        info.ClassId = clsid;
        info.ClassCtx = clsctx;
    }

    public InstantiationInfo InstantiationInfo => FindProperty<InstantiationInfo>();
    public ScmRequestInfo ScmRequestInfo => FindProperty<ScmRequestInfo>();
    public LocationInfo LocationInfo => FindProperty<LocationInfo>();
}
