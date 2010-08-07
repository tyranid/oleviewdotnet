using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;
using System.Windows.Forms;
using System.Reflection;

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
