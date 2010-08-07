using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.IO;

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
