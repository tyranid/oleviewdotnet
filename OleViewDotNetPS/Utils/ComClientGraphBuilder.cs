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

using NtApiDotNet;
using OleViewDotNet.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OleViewDotNetPS.Utils;

public class ComClientGraphBuilder
{
    #region Private Members
    private readonly List<COMIPIDEntry> _clients;

    private bool RemoveUnknownProcess(COMIPIDEntry ipid)
    {
        if (!RemoveUnknownProcesses)
        {
            return false;
        }

        if (ipid.Process.Token.UserSid is null)
        {
            return true;
        }

        using var result = NtProcess.Open(ipid.ProcessId, ProcessAccessRights.QueryLimitedInformation, false);
        if (!result.IsSuccess)
        {
            return true;
        }

        COMProcessToken token = new(result.Result);
        if (token.UserSid is null)
        {
            return true;
        }

        return false;
    }

    private bool IncludeProcess(int process_id, string name)
    {
        if (IncludePids.Count == 0 && IncludeNames.Count == 0)
        {
            return true;
        }

        return IncludePids.Contains(process_id) || IncludeNames.Contains(name);
    }

    private string BuildDotGraph()
    {
        Dictionary<int, string> procs = new();
        Dictionary<int, COMProcessToken> tokens = new();
        Dictionary<Tuple<int, int>, int> set = new();

        foreach (var client in _clients)
        {
            if (RemoveUnknownProcess(client) ||
                !IncludeProcess(client.ProcessId, client.ProcessName) &&
                !IncludeProcess(client.Process.ProcessId, client.Process.Name))
            {
                continue;
            }

            Tuple<int, int> entry = new(client.Process.ProcessId, client.ProcessId);
            if (!set.ContainsKey(entry))
            {
                set[entry] = 1;
            }
            else
            {
                set[entry]++;
            }

            if (!procs.ContainsKey(client.ProcessId))
            {
                procs[client.ProcessId] = client.ProcessName;
            }
            if (!procs.ContainsKey(client.Process.ProcessId))
            {
                procs[client.Process.ProcessId] = client.Process.Name;
            }
            if (!tokens.ContainsKey(client.Process.ProcessId))
            {
                tokens[client.Process.ProcessId] = client.Process.Token;
            }
        }

        foreach (var pair in procs)
        {
            if (!tokens.ContainsKey(pair.Key))
            {
                using var result = NtProcess.Open(pair.Key, ProcessAccessRights.QueryLimitedInformation, false);
                if (result.IsSuccess)
                {
                    tokens[pair.Key] = new COMProcessToken(result.Result);
                }
                else
                {
                    tokens[pair.Key] = new COMProcessToken();
                }
            }
        }

        StringBuilder builder = new();
        builder.AppendLine("digraph comclients {");

        int index = 0;
        foreach (var entry in tokens.GroupBy(t => Tuple.Create(t.Value.User, t.Value.Sandboxed)))
        {
            builder.AppendLine($"subgraph cluster_{index++} {{");
            builder.AppendLine("style = filled;");
            builder.AppendLine("color = lightgrey;");
            builder.AppendLine("node[style = filled, color = white];");
            if (entry.Key.Item2)
            {
                builder.AppendLine($"\tlabel = \"{entry.Key.Item1.Replace("\\", "\\\\")} (SANDBOXED)\";");
            }
            else
            {
                builder.AppendLine($"\tlabel = \"{entry.Key.Item1.Replace("\\", "\\\\")}\";");
            }
            foreach (var pair in entry)
            {
                builder.AppendLine($"\tpid_{pair.Key} [label=\"{pair.Key}\\n{procs[pair.Key]}\"];");
            }
            builder.AppendLine("}");
        }

        HashSet<Tuple<int, int>> emitted = new();

        foreach (var pair in set)
        {
            if (!emitted.Add(pair.Key))
            {
                continue;
            }

            var rev_entry = Tuple.Create(pair.Key.Item2, pair.Key.Item1);
            if (set.ContainsKey(rev_entry) && emitted.Add(rev_entry))
            {
                builder.AppendLine($"\tpid_{pair.Key.Item1} -> pid_{pair.Key.Item2} [dir=\"both\" color=blue];");
            }
            else
            {
                builder.AppendLine($"\tpid_{pair.Key.Item1} -> pid_{pair.Key.Item2};");
            }
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    #endregion

    #region Public Constructors

    public ComClientGraphBuilder(IEnumerable<int> include_pids, IEnumerable<string> include_names, bool remove_unknown_procs)
    {
        _clients = new List<COMIPIDEntry>();
        IncludePids = new HashSet<int>(include_pids ?? Enumerable.Empty<int>());
        IncludeNames = new HashSet<string>(include_names ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        RemoveUnknownProcesses = remove_unknown_procs;
    }

    public ComClientGraphBuilder()
        : this(Enumerable.Empty<int>(), Enumerable.Empty<string>(), false)
    {
    }
    #endregion

    #region Public Properties
    public HashSet<int> IncludePids { get; }
    public HashSet<string> IncludeNames { get; }
    public bool RemoveUnknownProcesses { get; set; }
    #endregion

    #region Public Methods
    public void Add(COMProcessEntry process)
    {
        _clients.AddRange(process.Clients);
    }

    public void AddRange(IEnumerable<COMProcessEntry> processes)
    {
        _clients.AddRange(processes.SelectMany(p => p.Clients));
    }

    public override string ToString()
    {
        return BuildDotGraph();
    }

    #endregion
}
