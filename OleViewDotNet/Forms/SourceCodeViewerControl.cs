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
using NtApiDotNet.Ndr;
using OleViewDotNet.Database;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

/* Added */
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
/* Added */

namespace OleViewDotNet.Forms;

internal partial class SourceCodeViewerControl : UserControl
{
    private COMRegistry m_registry;
    private object m_selected_obj;
    private ICOMSourceCodeFormattable m_formattable_obj;
    private COMSourceCodeBuilderType m_output_type;
    private bool m_hide_comments;
    private bool m_interfaces_only;
    private bool m_hide_parsing_options;

    /* Added */
    public bool m_isReally = true;
    /* Added */

    private class NdrTextMarker : TextMarker
    {
        public INdrNamedObject Tag { get; }

        public NdrTextMarker(int offset, int length, TextMarkerType textMarkerType, INdrNamedObject tag)
            : base(offset, length, textMarkerType)
        {
            Tag = tag;
        }
    }

    public SourceCodeViewerControl()
    {
        InitializeComponent();
        textEditor.SetHighlighting("C#");
        textEditor.IsReadOnly = true;
        m_hide_comments = true;
        toolStripMenuItemHideComments.Checked = true;
        m_interfaces_only = true;
        toolStripMenuItemInterfacesOnly.Checked = true;
        toolStripMenuItemIDLOutputType.Checked = true;
        m_output_type = COMSourceCodeBuilderType.Idl;
        SetText(string.Empty, Array.Empty<NdrFormatterNameTag>());
    }

    private void SetText(string text, IEnumerable<NdrFormatterNameTag> tags)
    {
        textEditor.Text = text.TrimEnd();
        foreach (var tag in tags)
        {
            textEditor.Document.MarkerStrategy.AddMarker(new NdrTextMarker(tag.Offset, tag.Length, TextMarkerType.Underlined, tag.Entry));
        }
        textEditor.Refresh();
    }

    /* Added */
    // This is for SetText without texteditor's tag. (from oleviewdotnet v1.14)
    private void SetText(string text)
    {
        textEditor.Text = text.TrimEnd();
        textEditor.Refresh();
    }
    /* Added */

    internal void SetRegistry(COMRegistry registry)
    {
        m_registry = registry;
    }

    private static bool IsParsed(ICOMSourceCodeFormattable obj)
    {
        if (obj is ICOMSourceCodeParsable parsable)
        {
            return parsable.IsSourceCodeParsed;
        }
        return true;
    }

    internal void Format(COMSourceCodeBuilder builder, ICOMSourceCodeFormattable formattable)
    {
        if (!IsParsed(formattable))
        {
            builder.AppendLine($"'{m_selected_obj}' needs to be parsed before it can be shown.");
        }
        else
        {
            formattable.Format(builder);
        }
    }

    /* Added */
    ResolvingForm resolvingForm = null;

    /* Added */


    //internal void Format()
    internal String Format()
    {
        COMSourceCodeBuilder builder = new(m_registry)
        {
            InterfacesOnly = m_interfaces_only,
            HideComments = m_hide_comments,
            OutputType = m_output_type
        };

        if (m_formattable_obj?.IsFormattable == true)
        {
            Format(builder, m_formattable_obj);
        }
        else
        {
            builder.AppendLine(m_selected_obj is null ?
                "No formattable object selected"
                : $"'{m_selected_obj}' is not formattable.");
            SetText(builder.ToString(), builder.Tags);
            return builder.ToString();
        }
        /* Added */

        // below code is for ResolveMethod.
        if (ResolveMethod.banList == null) ResolveMethod.BanListInit();

        if (builder.ToString().StartsWith("ERROR:") ||
            builder.ToString().Split('\n')[0].StartsWith("struct") || builder.ToString().Split('\n')[1].StartsWith("struct") ||
            builder.ToString().Split('\n')[0].StartsWith("union") || builder.ToString().Split('\n')[1].StartsWith("union") ||
            builder.ToString().Split('\n')[0].StartsWith("[switch_type") || builder.ToString().Split('\n')[1].StartsWith("[switch_type") ||
            builder.ToString().Split('\n')[0].Contains("needs to be parsed"))
        {
            SetText(builder.ToString(), builder.Tags);
            return builder.ToString();
        }

        if (!m_isReally || (!ProgramSettings.ResolveMethodNamesFromIDA && !ProgramSettings.ResolveMethodNamesFromIDAHard) ||
            GetIid() == "00000001-0000-0000-C000-000000000046")
        {
            SetText(builder.ToString(), builder.Tags);
            return builder.ToString();
        }

        String resultIDL = "";

        List<String> binaryPath = new List<string>();

        if (ProgramSettings.ResolveMethodDllFix)
        {
            binaryPath.Add(ProgramSettings.FixedDll);
        }
        else
        {
            String serviceName = GetServiceName();
            if (serviceName == null)
            {
                resultIDL = "// Resolve Failed. Failed to get service name.\n" + builder.ToString();
                SetText(resultIDL);
                return resultIDL;
            }
            String binary = ResolveMethod.GetBinaryPath(serviceName);
            if (binary == null)
            {
                resultIDL = "// Resolve Failed. Failed to get binary path.\n" + builder.ToString();
                SetText(resultIDL);
                return resultIDL;
            }
            binaryPath.Add(binary);
        }
        resultIDL = Resolve(builder.ToString(), binaryPath);

        if (resultIDL == null)
        {
            binaryPath.Clear();
            if (ProgramSettings.ResolveMethodNamesFromIDAHard)
            {

                String serviceName = GetServiceName();
                if (serviceName == null)
                {
                    resultIDL = "// Resolve Failed. Failed to get service name.\n" + builder.ToString();
                    SetText(resultIDL);
                    return resultIDL;
                }
                int pid = GetServicePid(serviceName);
                if (pid == -1)
                {
                    resultIDL = "// Resolve Hard Failed. Failed to find service pid.\n" + builder.ToString();
                    SetText(resultIDL);
                    return resultIDL;
                }

                Process process = Process.GetProcessById(pid);
                if (process == null)
                {
                    MessageBox.Show("process is null?");
                    return builder.ToString();
                }
                try
                {
                    for (int i = 0; i < process.Modules.Count; i++)
                    {
                        bool flag = true;
                        for (int j = 0; j < ResolveMethod.banList.Count; j++)
                        {
                            if (Path.GetFileName(ResolveMethod.banList[j]).ToLower()
                                == Path.GetFileName(process.Modules[i].FileName).ToLower())
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag) binaryPath.Add(process.Modules[i].FileName);
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    resultIDL = "// Resolve Hard Failed. Access Denied.\n" + builder.ToString();
                    SetText(resultIDL);
                    return resultIDL;
                }
                resultIDL = Resolve(builder.ToString(), binaryPath);
            }
        }

        if (resultIDL == null)
        {
            resultIDL = $"// Resolve Failed.\n";
            resultIDL += builder.ToString();
        }

        SetText(resultIDL);
        return resultIDL;
    }

    /* Added */
    // Finds method name with ResolveMethod, changes method name from Proc{n} to real method name and returns it.
    internal String Resolve(String idl, List<String> binaryPath)
    {
        if (binaryPath.Count == 0) return null;
        resolvingForm = new ResolvingForm(binaryPath);
        Thread uiThread = new Thread(StartResolvingForm);
        uiThread.Start();
        
        List<List<List<String>>> candidates = new List<List<List<String>>>();
        String resultIDL = "";

        for (int i = 0; i < binaryPath.Count; i++)
        {
            if (i < binaryPath.Count && resolvingForm.resolveDone)
            {
                return null;
            }
            resolvingForm.Update($"Trying to resolve from {Path.GetFileName(binaryPath[i])} ({i+1}/{binaryPath.Count})", $"Generating ASM File...");
            if (!ResolveMethod.GenerateAsmFile(binaryPath[i]))
            {
                resolvingForm.Update(null, null);
                resolvingForm.Update(null, null);
                Application.DoEvents();
                continue;
            }

            resolvingForm.Update(null, $"Searching VTables...");
            Application.DoEvents();

            List<List<String>> methods = ResolveMethod.GetMethodsFromIDA(binaryPath[i], idl);
            List<List<String>> methods2 = ResolveMethod.GetMethodsFromCandidates(binaryPath[i], idl);
            foreach (List<String> method in methods2) methods.Add(method);

            resolvingForm.Update(null, $"Converting Method Names...");
            Application.DoEvents();

            if (methods.Count > 0)
            {
                candidates.Add(methods);
                resultIDL += $"// {binaryPath[i]}\n";
                for (int j = 0; j < methods.Count; j++)
                {
                    resultIDL += $"// Candidates {j + 1}\n";
                    resultIDL += ResolveMethod.ConvertMethodName(idl, methods[j]);
                }
            }
            
        }
        resolvingForm.Close();

        if (candidates.Count == 0)
        {
            resultIDL = null;
        }
        return resultIDL;
    }

    // For UI thread.
    internal void StartResolvingForm()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(resolvingForm);
    }

    // Get iid from interface idl.
    internal String GetIid()
    {
        COMSourceCodeBuilder builder = new(m_registry)
        {
            InterfacesOnly = m_interfaces_only,
            HideComments = m_hide_comments,
            OutputType = m_output_type
        };

        if (m_formattable_obj?.IsFormattable == true)
        {
            Format(builder, m_formattable_obj);
        }
        else
        {
            return null;
        }

        String now = builder.ToString();
        if (now[0] == '[')
        {
            String[] nows = now.Split('\n');
            for (int i = 0; i < nows.Length; i++)
            {
                if (nows[i].Contains("uuid"))
                {
                    return nows[i].Split('(')[1].Split(')')[0];
                }
            }
        }
        else
        {
            return now.Split('\"')[1];
        }
        return null;
    }

    // Get iid by GetIid method and get service name from Registry.(HKEY_CLASSES_ROOT\CLSID)
    internal String GetServiceName()
    {
        string clsid = null;
        try
        {
            using (StreamReader reader = new StreamReader($"interfaces\\iids\\{this.GetIid()}.txt"))
            {
                clsid = reader.ReadToEnd();
            }
        }
        catch (Exception e) { return null; }
        string regKey = $"CLSID\\{{{clsid}}}";
        String appId = null;
        try
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(regKey))
            {
                if (key != null)
                {
                    object appIdValue = key.GetValue("AppID");
                    if (appIdValue != null)
                    {
                        appId = appIdValue.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return null;
        }

        if (appId == null) return null;
        regKey = $"AppID\\{appId}";
        try
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(regKey))
            {
                if (key != null)
                {
                    object serviceName = key.GetValue("LocalService");
                    if (serviceName != null)
                    {
                        return serviceName.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return null;
        }
        return null;
    }

    // Find pid of service.
    internal int GetServicePid(String serviceName)
    {

        string query = $"SELECT Name,ProcessId FROM Win32_Service WHERE Name LIKE '{serviceName}!_%'";
        using (var searcher1 = new System.Management.ManagementObjectSearcher(query))
        {
            foreach (var obj in searcher1.Get())
            {
                try
                {
                    ServiceController service = new ServiceController((String)obj["Name"]);

                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            using (var searcher2 = new System.Management.ManagementObjectSearcher(query)) {
                foreach (var obj in searcher2.Get())
                {
                    return Convert.ToInt32(obj["ProcessId"]);
                }
            }
        }
        query = $"SELECT Name,ProcessId FROM Win32_Service WHERE Name='{serviceName}'";
        using (var searcher1 = new System.Management.ManagementObjectSearcher(query))
        {
            foreach (var obj in searcher1.Get())
            {
                try
                {
                    ServiceController service = new ServiceController((String)obj["Name"]);

                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            using (var searcher2 = new System.Management.ManagementObjectSearcher(query))
            {
                foreach (var obj in searcher2.Get())
                {
                    return Convert.ToInt32(obj["ProcessId"]);
                }
            }
        }
        return -1;
    }

    /* Added */

    internal object SelectedObject
    {
        get => m_selected_obj;
        set
        {
            m_selected_obj = value;
            if (value is IEnumerable<ICOMSourceCodeFormattable> list)
            {
                m_formattable_obj = new SourceCodeFormattableList(list);
            }
            else if (value is ICOMSourceCodeFormattable formattable)
            {
                m_formattable_obj = formattable;
            }
            else
            {
                m_formattable_obj = null;
            }

            if (!IsParsed(m_formattable_obj) && AutoParse)
            {
                ParseSourceCode();
            }

            parseSourceCodeToolStripMenuItem.Enabled = m_formattable_obj is not null && !IsParsed(m_formattable_obj);
            Format();
        }
    }

    internal bool InterfacesOnly
    {
        get => m_interfaces_only;
        set => m_interfaces_only = value;
    }

    private void OnHideParsingOptionsChanged()
    {
        parseSourceCodeToolStripMenuItem.Visible = !m_hide_parsing_options;
        autoParseToolStripMenuItem.Visible = !m_hide_parsing_options;
    }

    internal bool AutoParse
    {
        get => autoParseToolStripMenuItem.Checked;
        set => autoParseToolStripMenuItem.Checked = value;
    }

    internal bool HideParsingOptions
    {
        get => m_hide_parsing_options;
        set
        {
            m_hide_parsing_options = value;
            OnHideParsingOptionsChanged();
        }
    }

    private void toolStripMenuItemIDLOutputType_Click(object sender, EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Idl;
        toolStripMenuItemIDLOutputType.Checked = true;
        toolStripMenuItemCppOutputType.Checked = false;
        toolStripMenuItemGenericOutputType.Checked = false;
        Format();
    }

    private void toolStripMenuItemCppOutputType_Click(object sender, EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Cpp;
        toolStripMenuItemIDLOutputType.Checked = false;
        toolStripMenuItemCppOutputType.Checked = true;
        toolStripMenuItemGenericOutputType.Checked = false;
        Format();
    }

    private void toolStripMenuItemGenericOutputType_Click(object sender, EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Generic;
        toolStripMenuItemIDLOutputType.Checked = false;
        toolStripMenuItemCppOutputType.Checked = false;
        toolStripMenuItemGenericOutputType.Checked = true;
        Format();
    }

    private void toolStripMenuItemHideComments_Click(object sender, EventArgs e)
    {
        m_hide_comments = !m_hide_comments;
        toolStripMenuItemHideComments.Checked = m_hide_comments;
        Format();
    }

    private void toolStripMenuItemInterfacesOnly_Click(object sender, EventArgs e)
    {
        m_interfaces_only = !m_interfaces_only;
        toolStripMenuItemInterfacesOnly.Checked = m_interfaces_only;
        Format();
    }

    private void exportToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using SaveFileDialog dlg = new();
        dlg.Filter = "All Files (*.*)|*.*";
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(dlg.FileName, textEditor.Text);
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }

    private void parseSourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ParseSourceCode();
        Format();
    }

    private void ParseSourceCode()
    {
        try
        {
            if (m_formattable_obj is ICOMSourceCodeParsable parsable && !parsable.IsSourceCodeParsed)
            {
                parsable.ParseSourceCode();
                parseSourceCodeToolStripMenuItem.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            m_formattable_obj = new SourceCodeFormattableText($"ERROR: {ex.Message}");
        }
    }

    private void autoParseToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (AutoParse)
        {
            SelectedObject = m_selected_obj;
        }
    }

    private NdrTextMarker GetTagAtCaret()
    {
        var tags = textEditor.Document.MarkerStrategy.GetMarkers(textEditor.ActiveTextAreaControl.Caret.Position);
        if (tags.Count > 0)
        {
            return (NdrTextMarker)tags[0];
        }
        return null;
    }

    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
        editNameToolStripMenuItem.Enabled = GetTagAtCaret() is not null;
    }

    private void editNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        NdrTextMarker tag = GetTagAtCaret();
        if (tag is null)
        {
            return;
        }

        using GetTextForm frm = new(tag.Tag.Name);
        frm.Text = "Edit Proxy Name";
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            tag.Tag.Name = frm.Data;
            textEditor.Document.ReadOnly = false;
            textEditor.Document.Replace(tag.Offset, tag.Length, frm.Data);
            textEditor.Document.MarkerStrategy.AddMarker(new NdrTextMarker(tag.Offset, frm.Data.Length, TextMarkerType.Underlined, tag.Tag));
            textEditor.Document.ReadOnly = true;
            textEditor.Refresh();
            if (m_selected_obj is ICOMSourceCodeEditable editable)
            {
                editable.Update();
            }
        }
    }

    private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Format();
    }
}
