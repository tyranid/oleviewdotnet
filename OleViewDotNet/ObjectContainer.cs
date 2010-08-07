using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    public partial class ObjectContainer : DockContent
    {
        private string m_objName;
        private object m_pObject;
        private GenericAxHost m_axControl;

        public ObjectContainer(string strObjName, object pObject)
        {
            m_objName = strObjName;
            m_pObject = pObject;
            InitializeComponent();

            try
            {
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectContainer));
                m_axControl = new GenericAxHost(pObject);
                ((System.ComponentModel.ISupportInitialize)(m_axControl)).BeginInit();
                SuspendLayout();

                m_axControl.Enabled = true;
                m_axControl.Location = new System.Drawing.Point(50, 39);
                m_axControl.Name = "axControl";
                m_axControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axControl.OcxState")));
                m_axControl.Dock = DockStyle.Fill;
                m_axControl.TabIndex = 0;
                Controls.Add(m_axControl);
                ((System.ComponentModel.ISupportInitialize)(m_axControl)).EndInit();
                ResumeLayout(false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Close();
            }
        }

        private void ObjectContainer_Load(object sender, EventArgs e)
        {
            TabText = String.Format("{0} Container", m_objName);
        }
    }

    class GenericAxHost : AxHost
    {
        private object m_pObject;

        public GenericAxHost(object pObject) : base(Guid.Empty.ToString())
        {
            m_pObject = pObject;
        }

        protected override object CreateInstanceCore(Guid clsid)
        {
            return m_pObject;
        }
    }
}
