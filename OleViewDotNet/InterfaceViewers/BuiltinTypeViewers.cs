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

using OleViewDotNet.Database;
using OleViewDotNet.Forms;
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet.InterfaceViewers;

internal class MonikerViewerFactory : GenericTypeViewerFactory<IMoniker>
{
}

internal class PersistFileViewerFactory : GenericTypeViewerFactory<IPersistFile>
{
}

internal class PersistStorageViewerFactory : GenericTypeViewerFactory<IPersistStorage>
{
}

internal class PersistPropertyBagViewerFactory : GenericTypeViewerFactory<IPersistPropertyBag>
{
}

internal class PersistMonikerViewerFactory : GenericTypeViewerFactory<IPersistMoniker>
{
}

internal class ClassFactoryViewerFactory : BaseTypeViewerFactory
{
    public ClassFactoryViewerFactory() : base(typeof(IClassFactory))
    {
    }

    public override Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        return new ClassFactoryTypeViewer(registry, entry, strObjName, pObject.Instance);
    }
}

internal class ElevatedFactoryServerViewerFactory : BaseTypeViewerFactory
{
    public ElevatedFactoryServerViewerFactory() : base(typeof(IElevatedFactoryServer))
    {
    }

    public override Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        if (entry is not COMCLSIDEntry clsid_entry)
        {
            throw new ArgumentException("Entry must be a COM class", "entry");
        }
        return new ElevatedFactoryServerTypeViewer(registry, clsid_entry, strObjName, pObject.Instance);
    }
}

internal class PersistStreamViewerFactory : ITypeViewerFactory
{
    public Guid Iid => COMKnownGuids.IID_IPersistStream;

    public string IidName => "IPersistStream";

    public Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        return new PersistStreamTypeViewer(strObjName, pObject.Instance);
    }
}

internal class PersistStreamInitViewerFactory : ITypeViewerFactory
{
    public Guid Iid => COMKnownGuids.IID_IPersistStreamInit;

    public string IidName => "IPersistStreamInit";

    public Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        return new PersistStreamTypeViewer(strObjName, pObject.Instance);
    }
}
