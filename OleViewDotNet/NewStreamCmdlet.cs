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

using System.IO;
using System.Management.Automation;

namespace OleViewDotNet
{
    [Cmdlet(VerbsCommon.New, "IStream")]
    class NewStreamCmdlet : Cmdlet
    {
        public NewStreamCmdlet()
        {
            ReadOnly = true;
            Append = false;
        }

        [Parameter(Position=0, Mandatory=true)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get; set;
        }

        [Parameter]
        public bool ReadOnly
        {
            get;
            set;
        }

        [Parameter]
        public bool Append
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            if (Name != null)
            {
                FileMode mode = FileMode.Open;
                FileAccess access = FileAccess.Read;
                FileShare share = FileShare.Read;

                if (!ReadOnly)
                {
                    if(Append)
                    {
                        mode = FileMode.Append;
                    }
                    else
                    {
                        mode = FileMode.CreateNew;
                    }

                    access = FileAccess.ReadWrite;
                }

                try
                {
                    WriteObject(new IStreamImpl(Name, mode, access, share));
                }
                catch (FileNotFoundException e)
                {
                    WriteError(new ErrorRecord(e, "CannotCreateFile", ErrorCategory.InvalidArgument, Name));
                }
            }
        }
    }
}
