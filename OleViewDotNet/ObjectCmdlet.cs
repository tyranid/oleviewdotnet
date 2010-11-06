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
using System.Management.Automation;

namespace OleViewDotNet
{
    [Cmdlet(VerbsCommon.Get, "ComObjects")]
    class ObjectCmdlet : Cmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        protected override void ProcessRecord()
        {
            if ((Name == null) && (Id == null))
            {
                WriteObject(ObjectCache.Objects);
            }
            else if (Id != null)
            {
                if (COMUtilities.IsValidGUID(Id))
                {
                    object o = ObjectCache.GetObjectByGuid(new Guid(Id));
                    if (o != null)
                    {
                        WriteObject(o);
                    }
                    else
                    {
                        WriteVerbose(String.Format("Could not find object from Guid {0}", Id));
                    }
                }
            }
            else
            {
                object o = ObjectCache.GetObjectByName(Name);

                if (o != null)
                {
                    WriteObject(o);
                }
                else
                {
                    WriteVerbose(String.Format("Could not find object from Name {0}", Name));
                }
            }
        }
    }
}
