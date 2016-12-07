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
using System.Xml;
using Microsoft.Win32;

namespace OleViewDotNet
{
    [Serializable]
    public class COMProgIDEntry : IComparable<COMProgIDEntry>, IXmlSerialize
    {
        public COMProgIDEntry(string progid, Guid clsid, RegistryKey rootKey)
        {
            Clsid = clsid;
            ProgID = progid;
            Name = rootKey.GetValue(null, String.Empty).ToString();
        }

        public int CompareTo(COMProgIDEntry right)
        {
            return String.Compare(ProgID, right.ProgID);
        }

        public string ProgID { get; private set; }

        public Guid Clsid { get; private set; }

        public string Name { get; private set; }

        public override string ToString()
        {
            return String.Format("COMProgIDEntry: {0}", Name);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMProgIDEntry right = obj as COMProgIDEntry;
            if (right == null)
            {
                return false;
            }

            return ProgID == right.ProgID && Name == right.Name && Clsid == right.Clsid;
        }

        public override int GetHashCode()
        {
            return ProgID.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Clsid.GetHashCode();
        }

        public COMProgIDEntry(XmlReader reader)
        {
            ProgID = reader.GetAttribute("progid");
            Clsid = new Guid(reader.GetAttribute("clsid"));
            string name = reader.GetAttribute("name");
            if (name != null)
            {
                Name = name;
            }
        }

        void IXmlSerialize.Serialize(XmlWriter writer)
        {            
            writer.WriteAttributeString("progid", ProgID);
            writer.WriteAttributeString("clsid", Clsid.ToString());
            if (Name != null)
            {
                writer.WriteAttributeString("name", Name);
            }
        }
    }
}
