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

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.PowerShell;

namespace OleViewDotNet
{
    class PowerShellInstance
    {
        [DllImport("kernel32.dll")]
        private static extern Boolean AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern Boolean FreeConsole();

        private enum ConsoleStdHandle
        {
            STD_INPUT_HANDLE = -10,
            STD_OUTPUT_HANDLE = -11,
            STD_ERROR_HANDLE = -12,
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(ConsoleStdHandle nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(String lpConsoleTitle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, CharacterAttributes wAttributes);

        //And here is the enumeration CharacterAttributes

        private enum CharacterAttributes
        {
            FOREGROUND_BLUE = 0x0001,
            FOREGROUND_GREEN = 0x0002,
            FOREGROUND_RED = 0x0004,
            FOREGROUND_INTENSITY = 0x0008,
            BACKGROUND_BLUE = 0x0010,
            BACKGROUND_GREEN = 0x0020,
            BACKGROUND_RED = 0x0040,
            BACKGROUND_INTENSITY = 0x0080,
            COMMON_LVB_LEADING_BYTE = 0x0100,
            COMMON_LVB_TRAILING_BYTE = 0x0200,
            COMMON_LVB_GRID_HORIZONTAL = 0x0400,
            COMMON_LVB_GRID_LVERTICAL = 0x0800,
            COMMON_LVB_GRID_RVERTICAL = 0x1000,
            COMMON_LVB_REVERSE_VIDEO = 0x4000,
            COMMON_LVB_UNDERSCORE = 0x8000
        }
        
        private static Thread m_shellThread = null;

        public static bool Started
        {
            get { return m_shellThread != null ? true : false; }
        }

        public static void CreateConsoleWindow()
        {
            AllocConsole();
            SetConsoleTitle("OleViewDotNet Powershell Console");
        }

        private static void ThreadEntry()
        {            
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                Type[] types = asm.GetTypes();

                CreateConsoleWindow();
                RunspaceConfiguration config = RunspaceConfiguration.Create();
                
                config.Assemblies.Append(new AssemblyConfigurationEntry(asm.FullName, asm.Location));

                /* Load all cmdlets from the assembly */
                foreach (Type t in types)
                {
                    object[] attrs = t.GetCustomAttributes(typeof(CmdletAttribute), true);
                    if (attrs.Length > 0)
                    {
                        foreach (CmdletAttribute attr in attrs)
                        {                            
                            config.Cmdlets.Append(new CmdletConfigurationEntry(attr.VerbName + "-" + attr.NounName, t, ""));
                        }
                    }
                }
                ConsoleShell.Start(config, "OleViewDotNet Powershell", "", new string[0]);
                FreeConsole();
                m_shellThread = null;
            }
            catch (Exception)
            {                
            }
        }

        public static void Open()
        {
            if (m_shellThread == null)
            {                
                m_shellThread = new Thread(ThreadEntry);
                m_shellThread.Start();
            }
        }

        public static void Close()
        {
            if (m_shellThread != null)
            {
                FreeConsole();
                m_shellThread.Join();
                m_shellThread = null;
            }
        }
    }
}
