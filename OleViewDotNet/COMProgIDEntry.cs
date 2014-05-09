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
