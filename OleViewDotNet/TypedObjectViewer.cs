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

using ICSharpCode.TextEditor.Document;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    /// <summary>
    /// Generic interface viewer from a type
    /// </summary>
    partial class TypedObjectViewer : DocumentForm
    {
        private enum ColumnSort
        {
            None,
            Ascending,
            Descending,
        }

        private class ListItemComparer : IComparer
        {
            private int _column;
            private ColumnSort _sort;

            public ListItemComparer(int column, ColumnSort sort)
            {
                _column = column;
                _sort = sort;
            }

            public int Compare(object x, object y)
            {
                ListViewItem xi = (ListViewItem)x;
                ListViewItem yi = (ListViewItem)y;
                if (_sort == ColumnSort.Ascending)
                {
                    return String.Compare(xi.SubItems[_column].Text, yi.SubItems[_column].Text);
                }
                else
                {
                    return String.Compare(yi.SubItems[_column].Text, xi.SubItems[_column].Text);
                }
            }
        }

        private string m_objName;
        private ObjectEntry m_pEntry;
        private object m_pObject;
        private Type m_dispType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strObjName">Descriptive name of the object</param>
        /// <param name="pEntry">Instance of the object</param>
        /// <param name="dispType">Reflected type</param>
        public TypedObjectViewer(string strObjName, ObjectEntry pEntry, Type dispType)
        {
            m_pEntry = pEntry;
            m_pObject = pEntry.Instance;
            m_objName = strObjName;
            m_dispType = dispType;
            InitializeComponent();

            HighlightingManager.Manager.AddSyntaxModeFileProvider(new SimpleSyntaxModeProvider("Python.xshd", "Python", ".py", Properties.Resources.PythonHighlightingRules));

            textEditorControl.SetHighlighting("Python");
        }

        public TypedObjectViewer(string strObjName, object pObject, Type dispType) 
            : this(strObjName, new ObjectEntry(strObjName, pObject), dispType)
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

            lblName.Text = "Name: " + m_dispType.Name;
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

            listViewMethods.ListViewItemSorter = new ListItemComparer(0, ColumnSort.Ascending);
            listViewMethods.Sort();
            listViewProperties.ListViewItemSorter = new ListItemComparer(0, ColumnSort.Ascending);
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

        private void TypedObjectViewer_Load(object sender, EventArgs e)
        {
            if (m_dispType != null)
            {
                LoadDispatch();
                TabText = String.Format("{0} {1}", m_objName, m_dispType.Name);
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateProperties();
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
                        TypedObjectViewer view = new TypedObjectViewer(m_objName, val, pi.PropertyType);
                        view.ShowHint = DockState.Document;
                        view.Show(this.DockPanel);
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
                    scope.SetVariable("obj", new DynamicComObjectWrapper(m_dispType, m_pObject));

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
                using (InvokeForm frm = new InvokeForm((MethodInfo)listViewMethods.SelectedItems[0].Tag, m_pObject))
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

        private void listViewMethods_ColumnClick(object sender, ColumnClickEventArgs e)
        {

        }

        private void clearOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxOutput.Clear();
        }
    }
}
