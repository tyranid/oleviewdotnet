using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

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
                //m_proxyclsid = new Guid(COMUtilities.ReadStringFromKey(key, "ProxyStubCLSID32", null));
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

        public COMInterfaceEntry(Guid iid, RegistryKey rootKey)
        {
            m_iid = iid;
            m_proxyclsid = Guid.Empty;
            m_nummethods = 3;
            m_base = "IUnknown";
            m_name = "";
            LoadFromKey(rootKey);
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

        public bool IsOleControl
        {
            get { return (m_iid == IID_IOleControl); }
        }

        public bool IsDispatch
        {
            get { return (m_iid == IID_IDispatch); }
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
    }
}
