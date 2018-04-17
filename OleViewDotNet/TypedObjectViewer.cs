//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

using ICSharpCode.TextEditor.Document;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet
{
    /// <summary>
    /// Generic interface viewer from a type
    /// </summary>
    partial class TypedObjectViewer : UserControl
    {        
        private string m_objName;
        private ObjectEntry m_pEntry;
        private object m_pObject;
        private Type m_dispType;
        private COMRegistry m_registry;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strObjName">Descriptive name of the object</param>
        /// <param name="pEntry">Instance of the object</param>
        /// <param name="dispType">Reflected type</param>
        public TypedObjectViewer(COMRegistry registry, string strObjName, ObjectEntry pEntry, Type dispType)
        {
            m_pEntry = pEntry;
            m_pObject = pEntry.Instance;
            m_objName = strObjName;
            m_dispType = dispType;
            m_registry = registry;
            InitializeComponent();

            HighlightingManager.Manager.AddSyntaxModeFileProvider(new SimpleSyntaxModeProvider("Python.xshd", "Python", ".py", Properties.Resources.PythonHighlightingRules));

            textEditorControl.SetHighlighting("Python");

            LoadDispatch();
            Text = String.Format("{0} {1}", m_objName, m_dispType.Name);
        }

        public TypedObjectViewer(COMRegistry registry, string strObjName, object pObject, Type dispType) 
            : this(registry, strObjName, new ObjectEntry(registry, strObjName, pObject), dispType)
        {

        }

        private void LoadDispatch()
        {
            listViewMethods.Columns.Add("Name");
            listViewMethods.Columns.Add("Return");
            listViewMethods.Columns.Add("Params");
            listViewProperties.Columns.Add("Name");
            listViewProperties.Columns.Add("Type");
            listViewProperties.Columns.Add("Value");
            listViewProperties.Columns.Add("Writeable");

            lblName.Text = m_dispType.Name;
            MemberInfo[] members = m_dispType.GetMembers();
            foreach (MemberInfo info in members)
            {
                if (info.MemberType == MemberTypes.Method)
                {
                    MethodInfo mi = (MethodInfo)info;

                    if (!mi.IsSpecialName)
                    {
                        ListViewItem item = listViewMethods.Items.Add(mi.Name);
                        item.Tag = mi;
                        List<string> pars = new List<string>();
                        item.SubItems.Add(mi.ReturnType.ToString());
                        ParameterInfo[] pis = mi.GetParameters();

                        foreach (ParameterInfo pi in pis)
                        {
                            string strDir = "";

                            if (pi.IsIn)
                            {
                                strDir += "in";
                            }

                            if (pi.IsOut)
                            {
                                strDir += "out";
                            }

                            if (strDir.Length == 0)
                            {
                                strDir = "in";
                            }

                            if (pi.IsRetval)
                            {
                                strDir += " retval";
                            }

                            if (pi.IsOptional)
                            {
                                strDir += " optional";
                            }

                            pars.Add(String.Format("[{0}] {1} {2}", strDir, pi.ParameterType.Name, pi.Name));
                        }

                        item.SubItems.Add(String.Join(", ", pars));
                    }                    
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo pi = (PropertyInfo)info;
                    ListViewItem item = listViewProperties.Items.Add(pi.Name);
                    item.Tag = pi;
                    item.SubItems.Add(pi.PropertyType.ToString());
                    
                    object val = null;

                    try
                    {
                        if (pi.CanRead)
                        {
                            val = pi.GetValue(m_pObject, null);
                        }
                    }
                    catch (Exception)
                    {                        
                        val = null;
                    }

                    if (val != null)
                    {
                        item.SubItems.Add(val.ToString());
                    }
                    else
                    {
                        item.SubItems.Add("<null>");
                    }

                    item.SubItems.Add(pi.CanWrite.ToString());
                }
            }

            listViewMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            listViewMethods.ListViewItemSorter = new ListItemComparer(0);
            listViewMethods.Sort();
            listViewProperties.ListViewItemSorter = new ListItemComparer(0);
            listViewProperties.Sort();
        }

        private void UpdateProperties()
        {
            foreach (ListViewItem item in listViewProperties.Items)
            {
                PropertyInfo pi = (PropertyInfo)item.Tag;
                object val = null;

                try
                {
                    if (pi.CanRead)
                    {
                        val = pi.GetValue(m_pObject, null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    val = null;
                }

                if (val != null)
                {
                    item.SubItems[2].Text = val.ToString();
                }
                else
                {
                    item.SubItems[2].Text = "<null>";
                }
            }
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateProperties();
        }

        private void OpenObjectViewer(DynamicComObjectWrapper wrapper)
        {
            Program.GetMainForm(m_registry).HostControl(new TypedObjectViewer(m_registry, m_objName, wrapper.Instance, wrapper.InstanceType));
        }

        private void OpenObject(ListView lv)
        {
            if (lv.SelectedItems.Count > 0)
            {
                ListViewItem item = lv.SelectedItems[0];
                if (item.Tag is PropertyInfo)
                {
                    PropertyInfo pi = (PropertyInfo)item.Tag;
                    object val = null;

                    try
                    {
                        if (pi.CanRead)
                        {
                            val = pi.GetValue(m_pObject, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        val = null;
                    }

                    if (val != null)
                    {
                        Program.GetMainForm(m_registry).HostControl(new TypedObjectViewer(m_registry, m_objName, val, pi.PropertyType));
                    }
                }
            }
        }

        private void openObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenObject(listViewProperties);
        }

        private void openObjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenObject(listViewMethods);
        }

        private void AddText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddText), text);
            }
            else
            {
                if (!richTextBoxOutput.IsDisposed)
                {
                    richTextBoxOutput.Text += text;
                    richTextBoxOutput.SelectionStart = richTextBoxOutput.Text.Length;
                    richTextBoxOutput.ScrollToCaret();
                }
            }
        }

        private void WriteOutputString(string s)
        {
            AddText(s);
        }

        private void WriteErrorString(string s)
        {
            AddText(s);
        }

        private class ConsoleTextWriter : TextWriter
        {
            private StringBuilder _builder;
            private TypedObjectViewer _control;
            private bool _error;

            public ConsoleTextWriter(TypedObjectViewer control, bool error)
            {
                _builder = new StringBuilder();
                _control = control;
                _error = error;
            }

            public override void Write(char value)
            {
                _builder.Append(value);

                if (value == '\n')
                {
                    if (_error)
                    {
                        _control.WriteErrorString(_builder.ToString());
                    }
                    else
                    {
                        _control.WriteOutputString(_builder.ToString());
                    }
                    _builder.Clear();
                }
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        internal class ScriptErrorListener : ErrorListener
        {
            public List<string> Errors { get; private set; }

            public ScriptErrorListener()
            {
                Errors = new List<string>();
            }

            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity)
            {                
                Errors.Add(String.Format("{0}: {1}/{2} - {3}", severity, span.Start.Line, span.Start.Column, message));
            }
        }

        private void toolStripButtonRun_Click(object sender, EventArgs e)
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                
                engine.Runtime.LoadAssembly(Assembly.GetExecutingAssembly());
                engine.Runtime.IO.SetOutput(new MemoryStream(), new ConsoleTextWriter(this, false));
                engine.Runtime.IO.SetErrorOutput(new MemoryStream(), new ConsoleTextWriter(this, true));

                ICollection<string> paths = engine.GetSearchPaths();

                paths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonLib"));
                
                engine.SetSearchPaths(paths);

                ScriptErrorListener listener = new ScriptErrorListener();
                ScriptSource source = engine.CreateScriptSourceFromString(textEditorControl.Text);

                CompiledCode code = source.Compile(listener);

                if (listener.Errors.Count == 0)
                {
                    // Just create the global scope, don't execute it yet
                    ScriptScope scope = engine.CreateScope();
                   
                    scope.SetVariable("obj", COMUtilities.IsComImport(m_dispType) ? new DynamicComObjectWrapper(m_registry, m_dispType, m_pObject) : m_pObject);
                    scope.SetVariable("disp", m_pObject);

                    dynamic host = new ExpandoObject();

                    host.openobj = new Action<DynamicComObjectWrapper>(o => { OpenObjectViewer(o); });

                    scope.SetVariable("host", host);

                    code.Execute(scope);
                }
            }
            catch (Exception ex)
            {
                TargetInvocationException tex = ex as TargetInvocationException;

                if (tex != null)
                {
                    ex = tex.InnerException;
                }

                AddText(ex.Message + Environment.NewLine);
            }
        }

        private void listViewMethods_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewMethods.SelectedItems.Count > 0)
            {
                using (InvokeForm frm = new InvokeForm(m_registry, (MethodInfo)listViewMethods.SelectedItems[0].Tag, m_pObject, m_objName))
                {
                    frm.ShowDialog();
                }

                UpdateProperties();
            }
        }

        private void listViewProperties_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewProperties.SelectedItems.Count > 0)
            {
                PropertyInfo pi = (PropertyInfo)listViewProperties.SelectedItems[0].Tag;

                if (pi.CanWrite)
                {
                    object val = null;

                    try
                    {
                        if (pi.CanRead)
                        {
                            val = pi.GetValue(m_pObject, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        val = null;
                    }

                    using (GetTypeForm frm = new GetTypeForm(pi.PropertyType, val))
                    {
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                pi.SetValue(m_pObject, frm.Data, null);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex.ToString());
                            }
                            UpdateProperties();
                        }
                    }
                }
            }
        }

        private void clearOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxOutput.Clear();
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
        }

        private void toolStripButtonExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Filter = "Python Scripts (*.py)|*.py|All Files (*.*)|*.*";

                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        File.WriteAllText(dlg.FileName, textEditorControl.Text);
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButtonImport_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Python Scripts (*.py)|*.py|All Files (*.*)|*.*";

                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        textEditorControl.Text = File.ReadAllText(dlg.FileName);
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
