using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    /// <summary>
    /// Form to view the COM registration information
    /// </summary>
    public partial class COMRegistryViewer : DockContent
    {
        /// <summary>
        /// Current registry
        /// </summary>
        COMRegistry m_reg;

        /// <summary>
        /// Enumeration to indicate what to display
        /// </summary>
        public enum DisplayMode
        {
            CLSIDs,
            ProgIDs,
            CLSIDsByName,
            CLSIDsByServer,
            Interfaces,
            InterfacesByName,
            ImplementedCategories,
            PreApproved
        }        

        /// <summary>
        /// Current display mode
        /// </summary>
        private DisplayMode m_mode;

        /// <summary>
        /// Constants for the ImageList icons
        /// </summary>
        private const int FolderIcon = 0;
        private const int InterfaceIcon = 1;
        private const int ClassIcon = 2;        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reg">The COM registry</param>
        /// <param name="mode">The display mode</param>
        public COMRegistryViewer(COMRegistry reg, DisplayMode mode)
        {
            InitializeComponent();
            m_reg = reg;
            m_mode = mode;
        }

        /// <summary>
        /// Build a tooltip for a CLSID entry
        /// </summary>
        /// <param name="ent">The CLSID entry to build the tool tip from</param>
        /// <returns>A string tooltip</returns>
        private string BuildCLSIDToolTip(COMCLSIDEntry ent)
        {
            StringBuilder strRet = new StringBuilder();

            strRet.AppendFormat("CLSID: {0}\n", ent.Clsid.ToString("B"));
            strRet.AppendFormat("Name: {0}\n", ent.Name);
            strRet.AppendFormat("{0}: {1}\n", ent.Type.ToString(), ent.Server);
            string[] progids = ent.ProgIDs;
            if (progids.Length > 0)
            {
                strRet.Append("ProgIDs:\n");
                foreach (string progid in progids)
                {
                    strRet.AppendFormat("{0}\n", progid);
                }
            }
            if (ent.AppID != Guid.Empty)
            {
                strRet.AppendFormat("AppID: {0}\n", ent.AppID.ToString("B"));            
            }
            if (ent.TypeLib != Guid.Empty)
            {
                strRet.AppendFormat("TypeLib: {0}\n", ent.TypeLib.ToString("B"));
            }
            
            COMInterfaceEntry[] proxies = ent.Proxies;
            if (proxies.Length > 0)
            {
                strRet.Append("Interface Proxies:\n");
                foreach (COMInterfaceEntry intEnt in proxies)
                {
                    strRet.AppendFormat("{0} - {1}\n", intEnt.Iid.ToString(), intEnt.Name);
                }
            } 

            return strRet.ToString();
        }

        /// <summary>
        /// Build a ProgID entry tooltip
        /// </summary>
        /// <param name="ent">The ProgID entry</param>
        /// <returns>The ProgID tooltip</returns>
        private string BuildProgIDToolTip(COMProgIDEntry ent)
        {
            string strRet;
            if (ent.Entry != null)
            {
                strRet = BuildCLSIDToolTip(ent.Entry);
            }
            else
            {
                strRet = String.Format("CLSID: {0}\n", ent.Clsid.ToString("B"));
            }

            return strRet;
        }

        private string BuildInterfaceToolTip(COMInterfaceEntry ent)
        {
            string strRet;

            strRet = String.Format("Name: {0}\n", ent.Name);
            strRet += String.Format("IID: {0}\n", ent.Iid.ToString("B"));
            if (ent.ProxyClsid != Guid.Empty)
            {
                strRet += String.Format("ProxyCLSID: {0}\n", ent.ProxyClsid.ToString("B"));
            }

            return strRet;
        }

        private TreeNode CreateCLSIDNode(COMCLSIDEntry ent)
        {
            TreeNode nodeRet = new TreeNode(String.Format("{0} - {1}", ent.Clsid.ToString(), ent.Name), ClassIcon, ClassIcon);
            nodeRet.ToolTipText = BuildCLSIDToolTip(ent);
            nodeRet.Tag = ent;
            nodeRet.Nodes.Add("IUnknown");

            return nodeRet;
        }

        private TreeNode CreateInterfaceNode(COMInterfaceEntry ent)
        {
            TreeNode nodeRet = new TreeNode(String.Format("{0} - {1}", ent.Iid.ToString(), ent.Name), InterfaceIcon, InterfaceIcon);
            nodeRet.ToolTipText = BuildInterfaceToolTip(ent);
            nodeRet.Tag = ent;

            return nodeRet;
        }

        private TreeNode CreateInterfaceNameNode(COMInterfaceEntry ent)
        {
            TreeNode nodeRet = new TreeNode(ent.Name, InterfaceIcon, InterfaceIcon);
            nodeRet.ToolTipText = BuildInterfaceToolTip(ent);
            nodeRet.Tag = ent;

            return nodeRet;
        }

        private void LoadCLSIDs()
        {
            int i = 0;
            TreeNode[] clsidNodes = new TreeNode[m_reg.Clsids.Count];
            foreach (COMCLSIDEntry ent in m_reg.Clsids.Values)
            {
                clsidNodes[i] = CreateCLSIDNode(ent);                
                i++;
            }
            TreeNode clsidRoot = treeComRegistry.Nodes.Add("CLSID");
            clsidRoot.Nodes.AddRange(clsidNodes);
            TabText = "CLSIDs";            
        }

        private void LoadProgIDs()
        {
            int i = 0;
            TreeNode[] progidNodes = new TreeNode[m_reg.Progids.Count];
            foreach (COMProgIDEntry ent in m_reg.Progids.Values)
            {
                progidNodes[i] = new TreeNode(ent.ProgID, ClassIcon, ClassIcon);
                progidNodes[i].ToolTipText = BuildProgIDToolTip(ent);
                progidNodes[i].Tag = ent;
                if (ent.Entry != null)
                {
                    progidNodes[i].Nodes.Add("IUnknown");
                }
                i++;
            }
            TreeNode progidRoot = treeComRegistry.Nodes.Add("ProgID");
            progidRoot.Nodes.AddRange(progidNodes);
            TabText = "ProgIDs";
        }

        private void LoadCLSIDsByNames()
        {
            int i = 0;
            TreeNode[] clsidNameNodes = new TreeNode[m_reg.ClsidsByName.Length];
            foreach (COMCLSIDEntry ent in m_reg.ClsidsByName)
            {
                clsidNameNodes[i] = new TreeNode(ent.Name, ClassIcon, ClassIcon);
                clsidNameNodes[i].ToolTipText = BuildCLSIDToolTip(ent);
                clsidNameNodes[i].Tag = ent;
                clsidNameNodes[i].Nodes.Add("IUnknown");
                i++;
            }
            TreeNode clsidNameRoot = treeComRegistry.Nodes.Add("CLSID Names");
            clsidNameRoot.Nodes.AddRange(clsidNameNodes);
            TabText = "CLSIDs by Name";
        }

        private void LoadCLSIDByServer()
        {            
            int i = 0;
            SortedDictionary<string, List<COMCLSIDEntry>> dict = m_reg.ClsidsByServer;            
            
            TreeNode[] serverNodes = new TreeNode[dict.Keys.Count];
            foreach (KeyValuePair<string, List<COMCLSIDEntry>> pair in dict)
            {                
                serverNodes[i] = new TreeNode(pair.Key);
                serverNodes[i].ToolTipText = pair.Key;
            
                TreeNode[] clsidNodes = new TreeNode[pair.Value.Count];
                string[] nodeNames = new string[pair.Value.Count];
                int j = 0;

                foreach(COMCLSIDEntry ent in pair.Value)
                {
                    TreeNode currNode = new TreeNode(ent.Name, ClassIcon, ClassIcon);
                    currNode.ToolTipText = BuildCLSIDToolTip(ent);
                    currNode.Tag = ent;
                    currNode.Nodes.Add("IUnknown");
                    clsidNodes[j] = currNode;
                    nodeNames[j] = ent.Name;
                    j++;
                }

                Array.Sort(nodeNames, clsidNodes);
                serverNodes[i].Nodes.AddRange(clsidNodes);
                
                i++;
            }
            
            TreeNode clsidRoot = treeComRegistry.Nodes.Add("CLSID By Server");
            clsidRoot.Nodes.AddRange(serverNodes);
            TabText = "CLSIDs by Server";
        }

        private void LoadInterfaces()
        {
            int i = 0;
            TreeNode[] iidNodes = new TreeNode[m_reg.Interfaces.Count];
            foreach (COMInterfaceEntry ent in m_reg.Interfaces.Values)
            {
                iidNodes[i] = CreateInterfaceNode(ent);
                i++;
            }
            TreeNode clsidRoot = treeComRegistry.Nodes.Add("Interfaces");
            clsidRoot.Nodes.AddRange(iidNodes);
            TabText = "Interfaces";
        }

        private void LoadInterfacesByName()
        {                  
            int i = 0;
            TreeNode[] iidNameNodes = new TreeNode[m_reg.InterfacesByName.Length];
            foreach (COMInterfaceEntry ent in m_reg.InterfacesByName)
            {
                iidNameNodes[i] = CreateInterfaceNameNode(ent);                
                i++;
            }
            TreeNode clsidNameRoot = treeComRegistry.Nodes.Add("Interface Names");
            clsidNameRoot.Nodes.AddRange(iidNameNodes);
            TabText = "Interfaces by Name";        
        }

        private void LoadImplementedCategories()
        {
            int i = 0;
            Dictionary<Guid, List<COMCLSIDEntry>> dict = m_reg.ImplementedCategories;
            SortedDictionary<string, TreeNode> sortedNodes = new SortedDictionary<string, TreeNode>();
            
            foreach (KeyValuePair<Guid, List<COMCLSIDEntry>> pair in dict)
            {               
                TreeNode currNode = new TreeNode(COMUtilities.GetCategoryName(pair.Key));
                currNode.Tag = pair.Key;
                currNode.ToolTipText = String.Format("CATID: {0}", pair.Key.ToString("B"));
                sortedNodes.Add(currNode.Text, currNode);

                TreeNode[] clsidNodes = new TreeNode[pair.Value.Count];
                COMCLSIDEntry[] entries = pair.Value.ToArray();
                Array.Sort(entries);
                i = 0;
                foreach (COMCLSIDEntry ent in entries)
                {
                    clsidNodes[i] = new TreeNode(ent.Name, ClassIcon, ClassIcon);
                    clsidNodes[i].ToolTipText = BuildCLSIDToolTip(ent);
                    clsidNodes[i].Tag = ent;
                    clsidNodes[i].Nodes.Add("IUnknown");
                    i++;
                }
                currNode.Nodes.AddRange(clsidNodes);
            }

            TreeNode clsidRoot = treeComRegistry.Nodes.Add("Implemented Categories");
            TreeNode[] catNodes = new TreeNode[sortedNodes.Count];
            i = 0;
            foreach (KeyValuePair<string, TreeNode> pair in sortedNodes)
            {
                catNodes[i++] = pair.Value;
            }

            clsidRoot.Nodes.AddRange(catNodes);
            TabText = "Implemented Categories";            
        }

        private void LoadPreApproved()
        {
            int i = 0;
            TreeNode[] clsidNodes = new TreeNode[m_reg.PreApproved.Length];
            foreach (COMCLSIDEntry ent in m_reg.PreApproved)
            {
                clsidNodes[i] = CreateCLSIDNode(ent);
                i++;
            }
            TreeNode clsidRoot = treeComRegistry.Nodes.Add("PreApproved");
            clsidRoot.Nodes.AddRange(clsidNodes);
            TabText = "Explorer PreApproved";   
        }

        private void COMRegisterViewer_Load(object sender, EventArgs e)
        {
            Cursor currCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                switch (m_mode)
                {
                    case DisplayMode.CLSIDsByName:
                        LoadCLSIDsByNames();
                        break;
                    case DisplayMode.CLSIDs:
                        LoadCLSIDs();
                        break;
                    case DisplayMode.ProgIDs:
                        LoadProgIDs();
                        break;
                    case DisplayMode.CLSIDsByServer:
                        LoadCLSIDByServer();
                        break;
                    case DisplayMode.Interfaces:
                        LoadInterfaces();
                        break;
                    case DisplayMode.InterfacesByName:
                        LoadInterfacesByName();
                        break;
                    case DisplayMode.ImplementedCategories:
                        LoadImplementedCategories();
                        break;
                    case DisplayMode.PreApproved:
                        LoadPreApproved();
                        break;
                    default:
                        break;
                }
                if (treeComRegistry.TopNode != null)
                {
                    treeComRegistry.TopNode.Expand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Cursor.Current = currCursor;
        }

        private void SetupCLSIDNodeTree(TreeNode node, bool bRefresh)
        {
            try
            {
                COMCLSIDEntry clsid = null;

                if (node.Tag is COMCLSIDEntry)
                {
                    clsid = (COMCLSIDEntry)node.Tag;

                }
                else if (node.Tag is COMProgIDEntry)
                {
                    clsid = ((COMProgIDEntry)node.Tag).Entry;
                }

                if (clsid != null)
                {
                    node.Nodes.Clear();
                    COMInterfaceEntry[] intEntries = m_reg.GetSupportedInterfaces(clsid, bRefresh);

                    foreach (COMInterfaceEntry ent in intEntries)
                    {
                        node.Nodes.Add(CreateInterfaceNameNode(ent));
                    }
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(String.Format("Error querying COM interfaces\n{0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void treeComRegistry_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {            
            Cursor currCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            SetupCLSIDNodeTree(e.Node, false);

            Cursor.Current = currCursor;
        }

        enum CopyGuidType
        {
            CopyAsString,
            CopyAsStructure,
            CopyAsObject,
            CopyAsHexString
        }

        private void CopyGuidToClipboard(Guid guid, CopyGuidType copyType)
        {
            string strCopy = null;

            switch (copyType)
            {
                case CopyGuidType.CopyAsObject:
                    strCopy = String.Format("<object id=\"obj\" classid=\"clsid:{0}\">NO OBJECT</object>",
                        guid.ToString());
                    break;
                case CopyGuidType.CopyAsString:
                    strCopy = guid.ToString("B");
                    break;
                case CopyGuidType.CopyAsStructure:
                    {
                        MemoryStream ms = new MemoryStream(guid.ToByteArray());
                        BinaryReader reader = new BinaryReader(ms);
                        strCopy = "struct GUID guidObject = { ";
                        strCopy += String.Format("0x{0:X08}, 0x{1:X04}, 0x{2:X04}, {{", reader.ReadUInt32(),
                            reader.ReadUInt16(), reader.ReadUInt16());
                        for (int i = 0; i < 8; i++)
                        {
                            strCopy += String.Format("0x{0:X02}, ", reader.ReadByte());
                        }
                        strCopy += "}};";
                    }
                    break;
                case CopyGuidType.CopyAsHexString:
                    byte[] data = guid.ToByteArray();
                    strCopy = "";
                    foreach (byte b in data)
                    {
                        strCopy += String.Format("{0:X02}", b);
                    }
                    break;
            }

            if (strCopy != null)
            {
                Clipboard.SetText(strCopy);
            }
        }

        private void copyGUIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            Guid guid = Guid.Empty;

            if (node != null)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    guid = ((COMCLSIDEntry)node.Tag).Clsid;
                }
                else if (node.Tag is COMInterfaceEntry)
                {
                    guid = ((COMInterfaceEntry)node.Tag).Iid;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    COMProgIDEntry ent = (COMProgIDEntry)node.Tag;
                    if (ent.Entry != null)
                    {
                        guid = ent.Entry.Clsid;
                    }
                }
                else if (node.Tag is Guid)
                {
                    guid = (Guid)node.Tag;
                }

                if (guid != Guid.Empty)
                {
                    CopyGuidToClipboard(guid, CopyGuidType.CopyAsString);
                }
            }
        }

        private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            Guid guid = Guid.Empty;

            if (node != null)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    guid = ((COMCLSIDEntry)node.Tag).Clsid;
                }
                else if (node.Tag is COMInterfaceEntry)
                {
                    guid = ((COMInterfaceEntry)node.Tag).Iid;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    COMProgIDEntry ent = (COMProgIDEntry)node.Tag;
                    if (ent.Entry != null)
                    {
                        guid = ent.Entry.Clsid;
                    }
                }
                else if (node.Tag is Guid)
                {
                    guid = (Guid)node.Tag;
                }

                if (guid != Guid.Empty)
                {
                    CopyGuidToClipboard(guid, CopyGuidType.CopyAsStructure);
                }
            }
        }

        private void copyGUIDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            Guid guid = Guid.Empty;

            if (node != null)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    guid = ((COMCLSIDEntry)node.Tag).Clsid;
                }
                else if (node.Tag is COMInterfaceEntry)
                {
                    guid = ((COMInterfaceEntry)node.Tag).Iid;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    COMProgIDEntry ent = (COMProgIDEntry)node.Tag;
                    if (ent.Entry != null)
                    {
                        guid = ent.Entry.Clsid;
                    }
                }
                else if (node.Tag is Guid)
                {
                    guid = (Guid)node.Tag;
                }

                if (guid != Guid.Empty)
                {
                    CopyGuidToClipboard(guid, CopyGuidType.CopyAsHexString);
                }
            }
        }

        private void copyObjectTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            Guid guid = Guid.Empty;

            if (node != null)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    guid = ((COMCLSIDEntry)node.Tag).Clsid;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    COMProgIDEntry ent = (COMProgIDEntry)node.Tag;
                    if (ent.Entry != null)
                    {
                        guid = ent.Entry.Clsid;
                    }
                }

                if (guid != Guid.Empty)
                {
                    CopyGuidToClipboard(guid, CopyGuidType.CopyAsObject);
                }
            }
        }

        private void createInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;

            if (node != null)
            {
                COMCLSIDEntry ent = null;
                if (node.Tag is COMCLSIDEntry)
                {
                    ent = (COMCLSIDEntry)node.Tag;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    ent = ((COMProgIDEntry)node.Tag).Entry;
                }
                
                if(ent != null)
                {                    
                    Dictionary<string, string> props = new Dictionary<string,string>();
                    try
                    {
                        object comObj = ent.CreateInstanceAsObject();
                        if (comObj != null)
                        {                            
                            props.Add("CLSID", ent.Clsid.ToString("B"));
                            props.Add("Name", ent.Name);
                            props.Add("Server", ent.Server);
                            
                            /* Need to implement a type library reader */
                            Type dispType = COMUtilities.GetDispatchTypeInfo(comObj);

                            ObjectInformation view = new ObjectInformation(ent.Name, comObj, props, m_reg.GetSupportedInterfaces(ent, false));
                            view.ShowHint = DockState.Document;
                            view.Show(this.DockPanel);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }    

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;

            if ((node != null) && (node.Tag != null))
            {
                contextMenuStrip.Items.Clear();
                contextMenuStrip.Items.Add(copyGUIDToolStripMenuItem);
                contextMenuStrip.Items.Add(copyGUIDHexStringToolStripMenuItem);
                contextMenuStrip.Items.Add(copyGUIDCStructureToolStripMenuItem);
                if ((node.Tag is COMCLSIDEntry) || (node.Tag is COMProgIDEntry))
                {
                    contextMenuStrip.Items.Add(copyObjectTagToolStripMenuItem);
                    contextMenuStrip.Items.Add(createInstanceToolStripMenuItem);
                    contextMenuStrip.Items.Add(refreshInterfacesToolStripMenuItem);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void refreshInterfacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            if ((node != null) && (node.Tag != null))
            {
                SetupCLSIDNodeTree(node, true);
            }
        }
    }
}
