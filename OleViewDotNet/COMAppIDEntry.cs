using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OleViewDotNet
{
    public class COMAppIDEntry : IComparable<COMAppIDEntry>
    {
        private Guid m_appId;

        private void LoadFromKey(RegistryKey key)
        {
            //object name = key.GetValue(null);
            //if ((name != null) && (name.ToString().Length > 0))
            //{
            //    m_name = name.ToString();
            //}
            //else
            //{
            //    m_name = String.Format("{{{0}}}", m_iid.ToString());
            //}

            //try
            //{
            //    m_proxyclsid = COMUtilities.ReadGuidFromKey(key, "ProxyStubCLSID32", null);
            //}
            //catch (FormatException e)
            //{
            //    System.Diagnostics.Debug.WriteLine(e.ToString());
            //}

            //string nummethods = COMUtilities.ReadStringFromKey(key, "NumMethods", null);

            //if (!int.TryParse(nummethods, out m_nummethods) || m_nummethods < 3)
            //{
            //    m_nummethods = 3;
            //}

            //m_base = COMUtilities.ReadStringFromKey(key, "BaseInterface", null);
            //if (m_base.Length == 0)
            //{
            //    m_base = "IUnknown";
            //}
        }

        public int CompareTo(COMAppIDEntry other)
        {
            return m_appId.CompareTo(other.m_appId);
        }

        public Guid AppId
        {
            get
            {
                return m_appId;
            }
        }
    }
}
