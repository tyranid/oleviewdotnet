using System;
using System.Collections.Generic;
using System.Text;
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
