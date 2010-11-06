//    This file is part of OleViewDotNet.
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
using System.IO;
using System.Management.Automation;

namespace OleViewDotNet
{
    [Cmdlet(VerbsCommon.New, "IStream")]
    class IStreamCmdlet : Cmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string FileName { get; set; }
        [Parameter]
        [ValidateNotNullOrEmpty]
        public bool Writable { get; set; }

        protected override void ProcessRecord()
        {
            Stream s = null;
            if (String.IsNullOrEmpty(FileName))
            {
                s = new MemoryStream();
            }
            else
            {
                if (Writable)
                {
                    s = File.Create(FileName);
                }
                else
                {
                    s = File.OpenRead(FileName);
                }
            }

            WriteObject(new IStreamImpl(s));
        }
    }
}
