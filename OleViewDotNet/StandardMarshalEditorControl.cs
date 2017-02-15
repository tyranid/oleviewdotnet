//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using System.Diagnostics;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class StandardMarshalEditorControl : UserControl
    {
        private COMRegistry m_registry;
        private COMObjRefStandard m_objref;

        public StandardMarshalEditorControl(COMRegistry registry, COMObjRefStandard objref)
        {
            m_objref = objref;
            m_registry = registry;
            InitializeComponent();
            textBoxStandardFlags.Text = String.Format("0x{0:X}", objref.StdObjRef.Flags);
            textBoxPublicRefs.Text = objref.StdObjRef.PublicRefs.ToString();
            textBoxOxid.Text = String.Format("0x{0:X016}", objref.StdObjRef.Oxid);
            textBoxOid.Text = String.Format("0x{0:X016}", objref.StdObjRef.Oid);
            textBoxIpid.Text = objref.StdObjRef.Ipid.FormatGuid();
            textBoxApartmentId.Text = COMUtilities.GetApartmentIdFromIPid(objref.StdObjRef.Ipid).ToString();
            int pid = COMUtilities.GetProcessIdFromIPid(objref.StdObjRef.Ipid);
            textBoxProcessId.Text = COMUtilities.GetProcessIdFromIPid(objref.StdObjRef.Ipid).ToString();
            try
            {
                Process p = Process.GetProcessById(pid);
                textBoxProcessName.Text = p.ProcessName;
            }
            catch (ArgumentException)
            {
                textBoxProcessName.Text = "N/A";
            }

            COMObjRefHandler handler = objref as COMObjRefHandler;
            if (handler != null)
            {
                textBoxHandlerClsid.Text = handler.Clsid.FormatGuid();
                COMCLSIDEntry ent = registry.MapClsidToEntry(handler.Clsid);
                if (ent != null)
                {
                    textBoxHandlerName.Text = ent.Name;
                }
            }
            else
            {
                tableLayoutPanel.Controls.Remove(lblHandlerClsid);
                tableLayoutPanel.Controls.Remove(lblHandlerName);
                tableLayoutPanel.Controls.Remove(textBoxHandlerClsid);
                tableLayoutPanel.Controls.Remove(textBoxHandlerName);
            }

            foreach (COMStringBinding str in objref.StringArray.StringBindings)
            {
                ListViewItem item = listViewStringBindings.Items.Add(str.TowerId.ToString());
                item.SubItems.Add(str.NetworkAddr);
                item.Tag = str;
            }
            listViewStringBindings.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewStringBindings.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            foreach (COMSecurityBinding sec in objref.StringArray.SecurityBindings)
            {
                ListViewItem item = listViewSecurityBindings.Items.Add(sec.AuthnSvc.ToString());
                item.SubItems.Add(sec.PrincName);
                item.Tag = sec;
            }
            listViewSecurityBindings.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewSecurityBindings.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}
