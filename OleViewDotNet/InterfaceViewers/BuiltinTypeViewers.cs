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
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet.InterfaceViewers
{
    class MonikerViewerFactory : GenericTypeViewerFactory<IMoniker>
    {
    }

    class PersistFileViewerFactory : GenericTypeViewerFactory<IPersistFile>
    {
    }

    class PersistStorageViewerFactory : GenericTypeViewerFactory<IPersistStorage>
    {
    }

    class PersistPropertyBagViewerFactory : GenericTypeViewerFactory<IPersistPropertyBag>
    {
    }

    class PersistMonikerViewerFactory : GenericTypeViewerFactory<IPersistMoniker>
    {
    }

    class ClassFactoryViewerFactory : BaseTypeViewerFactory
    {
        public ClassFactoryViewerFactory() : base(typeof(IClassFactory))
        {
        }

        public override Control CreateInstance(COMRegistry registry, string strObjName, ObjectEntry pObject)
        {
            return new ClassFactoryTypeViewer(registry, strObjName, pObject.Instance);
        }
    }

    class PersistStreamViewerFactory : ITypeViewerFactory
    {
        public Guid Iid
        {
            get { return COMInterfaceEntry.IID_IPersistStream; }
        }

        public string IidName
        {
            get { return "IPersistStream"; }
        }

        public Control CreateInstance(COMRegistry registry, string strObjName, ObjectEntry pObject)
        {
            return new PersistStreamTypeViewer(strObjName, pObject.Instance);
        }
    }

    class PersistStreamInitViewerFactory : ITypeViewerFactory
    {
        public Guid Iid
        {
            get { return COMInterfaceEntry.IID_IPersistStreamInit; }
        }

        public string IidName
        {
            get { return "IPersistStreamInit"; }
        }

        public Control CreateInstance(COMRegistry registry, string strObjName, ObjectEntry pObject)
        {
            return new PersistStreamTypeViewer(strObjName, pObject.Instance);
        }
    }
}
