//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014. 2016
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

using Microsoft.Win32;
using System;

namespace OleViewDotNet
{
    [Serializable]
    public class COMMimeType
    {
        public string MimeType { get; private set; }
        public Guid Clsid { get; private set; }
        public string Extension { get; private set; }

        public COMMimeType(string mime_type, RegistryKey key)
        {
            string clsid = key.GetValue("CLSID") as string;
            string extension = key.GetValue("Extension") as string;

            if ((clsid != null) && (COMUtilities.IsValidGUID(clsid)))
            {
                Clsid = Guid.Parse(clsid);
            }
            Extension = extension;
            MimeType = mime_type;
        }
    }
}
