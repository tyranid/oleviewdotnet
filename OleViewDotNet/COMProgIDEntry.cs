using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace OleViewDotNet
{
    public class COMProgIDEntry : IComparable<COMProgIDEntry>
    {
        private string m_progid;
        private Guid m_clsid;
        private string m_name;
        private COMCLSIDEntry m_entry;

        public COMProgIDEntry(string progid, Guid clsid, COMCLSIDEntry entry, RegistryKey rootKey)
        {
            m_clsid = clsid;
            m_progid = progid;
            m_entry = entry;
            m_name = "";
            if (entry != null)
            {
                entry.AddProgID(progid);
            }
        }

        public int CompareTo(COMProgIDEntry right)
        {
            return String.Compare(m_progid, right.m_progid);
        }

        public string ProgID
        {
            get { return m_progid; }
        }

        public Guid Clsid
        {
            get { return m_clsid; }
        }

        public COMCLSIDEntry Entry
        {
            get { return m_entry; }
        }

        public string Name
        {
            get { return m_name; }
        }
    }
}
