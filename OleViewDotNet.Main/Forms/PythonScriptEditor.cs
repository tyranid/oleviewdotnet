//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using System;
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class PythonScriptEditor : UserControl
    {
        public class RunScriptEventArgs : EventArgs
        {
            public string ScriptText { get; }
            internal RunScriptEventArgs(string script_text)
            {
                ScriptText = script_text;
            }
        }

        public event EventHandler<RunScriptEventArgs> RunScript;

        public PythonScriptEditor()
        {
            InitializeComponent();
            HighlightingManager.Manager.AddSyntaxModeFileProvider(
                    new SimpleSyntaxModeProvider("Python.xshd", "Python", ".py",
                    Properties.Resources.PythonHighlightingRules));
            textEditorControl.SetHighlighting("Python");
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

        private void toolStripButtonRun_Click(object sender, EventArgs e)
        {
            RunScript?.Invoke(this, new RunScriptEventArgs(textEditorControl.Text));
        }
    }
}
