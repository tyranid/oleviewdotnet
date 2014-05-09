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
using Microsoft.Win32;
using OleViewDotNet.InterfaceViewers;

namespace OleViewDotNet
{
    public class COMInterfaceEntry : IComparable<COMInterfaceEntry>
    {
        private string m_name;
        private Guid m_iid;
        private Guid m_proxyclsid;
        private int m_nummethods;
        private string m_base;

        public int CompareTo(COMInterfaceEntry right)
        {
            return String.Compare(m_name, right.m_name);
        }

        private void LoadFromKey(RegistryKey key)
        {
            object name = key.GetValue(null);
            if ((name != null) && (name.ToString().Length > 0))
            {
                m_name = name.ToString();
            }
            else
            {
                m_name = String.Format("{{{0}}}", m_iid.ToString());
            }

            try
            {                
                m_proxyclsid = COMUtilities.ReadGuidFromKey(key, "ProxyStubCLSID32", null);
            }
            catch (FormatException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            string nummethods = COMUtilities.ReadStringFromKey(key, "NumMethods", null);

            if (!int.TryParse(nummethods, out m_nummethods) || m_nummethods < 3)
            {
                m_nummethods = 3;
            }

            m_base = COMUtilities.ReadStringFromKey(key, "BaseInterface", null);
            if (m_base.Length == 0)
            {
                m_base = "IUnknown";
            }
        }

        private COMInterfaceEntry()
        {
        }

        private COMInterfaceEntry(Guid iid, Guid proxyclsid, int nummethods, string baseName, string name)
        {
            m_iid = iid;
            m_proxyclsid = proxyclsid;
            m_nummethods = nummethods;
            m_base = baseName;
            m_name = name;
        }

        public COMInterfaceEntry(Guid iid, RegistryKey rootKey) 
            : this(iid, Guid.Empty, 3, "IUnknown", "")
        {            
            LoadFromKey(rootKey);
        }

        public COMInterfaceEntry(Guid iid)
            : this(iid, Guid.Empty, 3, "IUnknown", iid.ToString("B"))
        {            
        }

        public enum KnownInterfaces
        {
            IUnknown,
            IMarshal
        }

        public static Guid IID_IUnknown
        {
            get { return new Guid("{00000000-0000-0000-C000-000000000046}"); }
        }

        public static Guid IID_IMarshal
        {
            get { return new Guid("{00000003-0000-0000-C000-000000000046}"); }
        }

        public static Guid IID_IDispatch
        {
            get { return new Guid("00020400-0000-0000-c000-000000000046"); }
        }

        public static Guid IID_IOleControl
        {
            get { return new Guid("{b196b288-bab4-101a-b69c-00aa00341d07}"); }
        }

        public static Guid IID_IPersistStream
        {
            get { return typeof(IPersistStream).GUID; }
        }

        public static Guid IID_IPersistStreamInit
        {
            get { return typeof(IPersistStreamInit).GUID; }
        }

        public bool IsOleControl
        {
            get { return (m_iid == IID_IOleControl); }
        }

        public bool IsDispatch
        {
            get { return (m_iid == IID_IDispatch); }
        }

        public bool IsMarshal
        {
            get { return (m_iid == IID_IMarshal); }
        }

        public bool IsPersistStream
        {
            get { return (m_iid == IID_IPersistStream) || (m_iid == IID_IPersistStreamInit); }
        }

        public static COMInterfaceEntry CreateKnownInterface(KnownInterfaces known)
        {
            COMInterfaceEntry ent = null;
            switch (known)
            {
                case KnownInterfaces.IUnknown:
                    ent = new COMInterfaceEntry();
                    ent.m_base = "";
                    ent.m_iid = new Guid("{00000000-0000-0000-C000-000000000046}");
                    ent.m_proxyclsid = Guid.Empty;
                    ent.m_nummethods = 3;
                    ent.m_name = "IUnknown";
                    break;
                case KnownInterfaces.IMarshal:
                    ent = new COMInterfaceEntry();
                    ent.m_base = "";
                    ent.m_iid = new Guid("{00000003-0000-0000-C000-000000000046}");
                    ent.m_proxyclsid = Guid.Empty;
                    ent.m_nummethods = 9;
                    ent.m_name = "IMarshal";
                    break;
            }

            return ent;
        }

        public string Name
        {
            get { return m_name; }
        }

        public Guid Iid
        {
            get { return m_iid; }
        }

        public Guid ProxyClsid
        {
            get { return m_proxyclsid; }
        }

        public int NumMethods
        {
            get { return m_nummethods; }
        }

        public string Base
        {
            get { return m_base; }
        }

        public override string ToString()
        {
            return String.Format("COMInterfaceEntry: {0}", m_name);
        }
    }
}
