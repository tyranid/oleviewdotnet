using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.IO;

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
