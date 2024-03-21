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

using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.IO;

namespace OleViewDotNetPS.Utils;

public class LoggingActivationFilter : IActivationFilter
{
    private static LoggingActivationFilter CreateActivationFilter()
    {
        LoggingActivationFilter filter = new();
        COMUtilities.RegisterActivationFilter(filter);
        return filter;
    }

    private static readonly Lazy<LoggingActivationFilter> _instance = new(CreateActivationFilter, true);

    private COMRegistry _registry;
    private TextWriter _writer;

    public static LoggingActivationFilter Instance => _instance.Value;

    public void Stop()
    {
        lock (this)
        {
            _registry = null;
            _writer?.Dispose();
            _writer = null;
        }
    }

    public void Start(string path, bool append, COMRegistry registry)
    {
        lock (this)
        {
            Stop();
            _writer = new StreamWriter(path, append);
            _registry = registry;
        }
    }

    void IActivationFilter.HandleActivation(FILTER_ACTIVATIONTYPE dwActivationType, in Guid rclsid, out Guid pReplacementClsId)
    {
        pReplacementClsId = rclsid;
        lock (this)
        {
            if (_writer is null)
            {
                return;
            }

            COMCLSIDEntry entry = _registry?.MapClsidToEntry(rclsid);
            if (entry is null)
            {
                _writer.WriteLine("dwActivationType: {0} rclsid: {1}",
                    dwActivationType, rclsid);
            }
            else
            {
                _writer.WriteLine("dwActivationType: {0} rclsid: {1} name '{2}'",
                    dwActivationType, rclsid, entry.Name);
            }
        }
    }
}
