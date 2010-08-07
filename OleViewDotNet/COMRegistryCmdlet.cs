using System;
using System.Management.Automation;

namespace OleViewDotNet
{
    [Cmdlet(VerbsCommon.Get, "ComRegistry")]
    class COMRegistryCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            COMRegistry reg = Program.GetCOMRegistry();
            if (reg != null)
            {
                WriteObject(reg);
            }
        }
    }
}
