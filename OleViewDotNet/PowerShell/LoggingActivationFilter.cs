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
using System;
using System.ComponentModel;
using System.IO;

namespace OleViewDotNet.PowerShell
{
    public class LoggingActivationFilter : IActivationFilter
    {
        static LoggingActivationFilter CreateActivationFilter()
        {
            LoggingActivationFilter filter = new LoggingActivationFilter();
            int hr = COMUtilities.CoRegisterActivationFilter(filter);
            if (hr != 0)
            {
                throw new Win32Exception(hr);
            }
            return filter;
        }

        static Lazy<LoggingActivationFilter> _instance = new Lazy<LoggingActivationFilter>(CreateActivationFilter, true);

        private COMRegistry _registry;
        private TextWriter _writer;

        public static LoggingActivationFilter Instance
        {
            get
            {
                return _instance.Value;
            }
        }

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

        void IActivationFilter.HandleActivation(FILTER_ACTIVATIONTYPE dwActivationType, ref Guid rclsid, out Guid pReplacementClsId)
        {
            pReplacementClsId = rclsid;
            lock (this)
            {
                if (_writer == null)
                {
                    return;
                }

                COMCLSIDEntry entry = _registry?.MapClsidToEntry(rclsid);
                if (entry == null)
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
}
