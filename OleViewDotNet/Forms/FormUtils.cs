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

using OleViewDotNet.Database;
using OleViewDotNet.Processes;
using OleViewDotNet.TypeManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal static class FormUtils
{
    public static string FormatObject(object obj, bool is_return)
    {
        if (obj is null)
        {
            return "<null>";
        }

        if (is_return && obj is int hr)
        {
            return $"0x{hr:X08} - {new Win32Exception(hr).Message}";
        }
        else if (obj is string s)
        {
            return s;
        }
        else if (obj is IEnumerable list)
        {
            return string.Join(", ", list.OfType<object>().ToArray());
        }
        return obj.ToString();
    }

    public static Type GetDispatchTypeInfo(IWin32Window parent, object comObj)
    {
        Type ret = null;

        try
        {
            using WaitingDialog dlg = new((progress, token) => COMTypeManager.GetDispatchTypeInfo(comObj, progress), s => s);
            dlg.Text = "Loading TypeLib";
            dlg.CancelEnabled = false;
            if (dlg.ShowDialog(parent) == DialogResult.OK)
            {
                return (Type)dlg.Result;
            }
            else if (dlg.Error is not null && dlg.Error is not OperationCanceledException)
            {
                EntryPoint.ShowError(parent, dlg.Error);
            }
            return null;
        }
        catch (Exception)
        {
        }

        return ret;
    }

    internal static COMRegistry LoadRegistry(IWin32Window window,
        Func<IProgress<Tuple<string, int>>, CancellationToken, object> worker)
    {
        using WaitingDialog loader = new(worker);
        if (loader.ShowDialog(window) == DialogResult.OK)
        {
            return loader.Result as COMRegistry;
        }
        else
        {
            throw loader.Error;
        }
    }

    internal static COMRegistry LoadRegistry(IWin32Window window, COMRegistryMode mode)
    {
        if (mode == COMRegistryMode.Diff)
        {
            throw new ArgumentException("Can't load a diff registry");
        }

        return LoadRegistry(window, (progress, token) => COMRegistry.Load(mode, null, progress));
    }

    internal static COMRegistry LoadRegistry(IWin32Window window, string database_file)
    {
        return LoadRegistry(window, (progress, token) => COMRegistry.Load(database_file, progress));
    }

    internal static COMRegistry DiffRegistry(IWin32Window window, COMRegistry left, COMRegistry right, COMRegistryDiffMode mode)
    {
        return LoadRegistry(window, (progress, token) => COMRegistry.Diff(left, right, mode, progress));
    }

    internal static IEnumerable<COMProcessEntry> LoadProcesses(IEnumerable<Process> procs, IWin32Window window, COMRegistry registry)
    {
        using WaitingDialog dlg = new((progress, token) => COMProcessParser.GetProcesses(procs, COMProcessParserConfig.Default, progress, registry), s => s);
        dlg.Text = "Loading Processes";
        if (dlg.ShowDialog(window) == DialogResult.OK)
        {
            return (IEnumerable<COMProcessEntry>)dlg.Result;
        }
        else if (dlg.Error is not null && dlg.Error is not OperationCanceledException)
        {
            EntryPoint.ShowError(window, dlg.Error);
        }
        return null;
    }

    internal static IEnumerable<COMProcessEntry> LoadProcesses(IEnumerable<int> pids, IWin32Window window, COMRegistry registry)
    {
        return LoadProcesses(pids.Select(p => Process.GetProcessById(p)), window, registry);
    }

    internal static IEnumerable<COMProcessEntry> LoadProcesses(IWin32Window window, COMRegistry registry)
    {
        int current_pid = Process.GetCurrentProcess().Id;
        var procs = Process.GetProcesses().Where(p => p.Id != current_pid).OrderBy(p => p.ProcessName);
        return LoadProcesses(procs, window, registry);
    }

    private class ReportQueryProgress
    {
        private readonly int _total_count;
        private int _current;
        private readonly IProgress<Tuple<string, int>> _progress;
        private const int MINIMUM_REPORT_SIZE = 25;

        public ReportQueryProgress(IProgress<Tuple<string, int>> progress, int total_count)
        {
            _total_count = total_count;
            _progress = progress;
        }

        public void Report()
        {
            int current = Interlocked.Increment(ref _current);
            if (current % MINIMUM_REPORT_SIZE == 1)
            {
                _progress.Report(new Tuple<string, int>($"Querying Interfaces: {current} of {_total_count}",
                    100 * current / _total_count));
            }
        }
    }

    private static bool QueryAllInterfaces(IEnumerable<ICOMClassEntry> clsids, IProgress<Tuple<string, int>> progress, CancellationToken token, int concurrent_queries)
    {
        ParallelOptions po = new();
        po.CancellationToken = token;
        po.MaxDegreeOfParallelism = concurrent_queries;

        ReportQueryProgress query_progress = new(progress, clsids.Count());

        Parallel.ForEach(clsids, po, clsid =>
        {
            po.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                query_progress.Report();
                clsid.LoadSupportedInterfaces(false, null);
            }
            catch
            {
            }
        });

        return true;
    }

    internal static bool QueryAllInterfaces(IWin32Window parent, IEnumerable<ICOMClassEntry> clsids, IEnumerable<COMServerType> server_types, int concurrent_queries, bool refresh_interfaces)
    {
        using WaitingDialog dlg = new(
            (p, t) => QueryAllInterfaces(clsids.Where(c => (refresh_interfaces || !c.InterfacesLoaded) && server_types.Contains(c.DefaultServerType)),
                        p, t, concurrent_queries),
            s => s);
        dlg.Text = "Querying Interfaces";
        return dlg.ShowDialog(parent) == DialogResult.OK;
    }

}