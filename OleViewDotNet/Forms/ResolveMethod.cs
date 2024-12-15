using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace OleViewDotNet.Forms
{
    internal class ResolveMethod
    {
        public static String IDAPath = null;
        public static List<String> banList = null;

        // Find IDA path from Registry(Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache).
        // If failed to find, create IDAPathForm to get IDA Path.
        public static String GetIDAT()
        {

            if (File.Exists("IDAPath"))
            {
                using (StreamReader reader = new StreamReader("IDAPath"))
                {
                    IDAPath = reader.ReadToEnd();
                }
                return IDAPath;
            }

            string regKey = "Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache";
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(regKey))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            object valueData = key.GetValue(valueName);
                            if (valueData is string stringValue && stringValue.Contains("Hex-Rays SA"))
                            {
                                String[] parts = valueName.Split('\\');
                                String fileName = String.Join("\\", parts, 0, parts.Length - 1) + "\\idat64.exe";
                                if (File.Exists(fileName))
                                {
                                    IDAPath = fileName;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (IDAPath == null)
            {
                MessageBox.Show("Failed to find idat64.exe.");

                IDAPathForm iDAPathForm = new IDAPathForm();
                iDAPathForm.ShowDialog();
            }

            using (StreamWriter writer = new StreamWriter("IDAPath"))
            {
                writer.Write(IDAPath);
            }

            return IDAPath;
        }

        // Find service DLL/EXE from Registry.
        public static String GetBinaryPath(String serviceName)
        {
            String regKey = $"SYSTEM\\ControlSet001\\Services\\{serviceName}\\Parameters";
            object value = null;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey))
                {
                    if (key != null)
                    {
                        value = key.GetValue("ServiceDLL");
                    }
                }
                if (value == null)
                {
                    regKey = $"SYSTEM\\ControlSet001\\Services\\{serviceName}";
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey))
                    {
                        if (key != null)
                        {
                            value = key.GetValue("ServiceDLL");
                        }
                    }
                }
                if (value == null)
                {
                    regKey = $"SYSTEM\\ControlSet001\\Services\\{serviceName}";
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey))
                    {
                        if (key != null)
                        {
                            value = key.GetValue("ImagePath");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (value == null)
            {
                return null;
            }
            else
            {
                String binaryName = value.ToString().ToLower();
                binaryName = binaryName.Replace("%systemroot%", "C:\\Windows");
                binaryName = binaryName.Replace("\"", "");
                binaryName = binaryName.Replace("%programfiles%", "C:\\Program Files");
                binaryName = binaryName.Replace("%windir%", "C:\\Windows");
                binaryName = binaryName.Replace("\n", "");

                String[] parts = binaryName.Split(' ');
                if (parts[parts.Length - 1].EndsWith(".exe") || parts[parts.Length - 1].EndsWith(".dll"))
                {
                    binaryName = String.Join(" ", parts);
                }
                else
                {
                    binaryName = String.Join(" ", parts, 0, parts.Length - 1);
                }

                return binaryName;
            }
        }

        // Copy service DLL/EXE to DLLs directory.
        public static void CopyDLL(String binaryPath)
        {
            if (!Directory.Exists("DLLs")) Directory.CreateDirectory("DLLs");
            String binaryName = Path.GetFileName(binaryPath);
            if (!File.Exists($"DLLs\\{binaryName}"))
            {

                File.Copy(binaryPath, $"DLLs\\{binaryName}");
            }
        }

        // Create .asm file for DLL/EXE using idat64.exe.
        public static bool GenerateAsmFile(String binaryPath)
        {
            String binaryName = Path.GetFileName(binaryPath);
            if (File.Exists($"DLLs\\{binaryName}" + ".asm")) return true;
            CopyDLL(binaryPath);
            Process process = new Process();
            if (IDAPath == null) process.StartInfo.FileName = GetIDAT();
            process.StartInfo.FileName = IDAPath;
            process.StartInfo.Arguments = $"-A -B DLLs\\{binaryName}";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                process.Start();
                process.WaitForExit();
                int exitCode = process.ExitCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to resolve interfaces.");
            }
            process.Dispose();
            return true;
        }

        // Get interface name from idl.
        public static String GetInterfaceName(String idl)
        {
            String[] lines = idl.Split('\n');
            if (lines[0].StartsWith("["))
            {
                foreach (String line in lines)
                {
                    if (line.StartsWith("interface")) return line.Split(' ')[1];
                }
            }
            else
            {
                return lines[0].Split(' ')[2];
            }
            return null;
        }

        // Get last proc number(Proc{m}) from idl.
        public static int GetLastProcNumFromIdl(String idl)
        {
            String[] lines = idl.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("}"))
                {
                    string pattern = @"(?<=Proc)\d+";

                    Match match = Regex.Match(lines[i - 1], pattern);
                    if (match.Success)
                    {
                        return int.Parse(match.Value);
                    }
                    else return -1;
                }
            }
            return -1;
        }

        // Get first proc number(Proc{n}) from idl.
        public static int GetFirstProcNumFromIdl(String idl)
        {
            String[] lines = idl.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("HRESULT"))
                {
                    string pattern = @"(?<=Proc)\d+";

                    Match match = Regex.Match(lines[i], pattern);
                    if (match.Success)
                    {
                        return int.Parse(match.Value);
                    }
                    else return -1;
                }
            }
            return -1;
        }

        // parsing .asm file to find COM vtables and get method names.
        // This method will find vtables with interface name.
        public static List<List<String>> GetMethodsFromIDA(String binaryPath, String idl)
        {
            List<List<String>> ret = new List<List<String>>();
            String[] asmLines = null;
            String[] idlLines = idl.Split('\n');

            int firstProcNum = GetFirstProcNumFromIdl(idl);
            int lastProcNum = GetLastProcNumFromIdl(idl);
            int idlMethodCnt;
            if (firstProcNum == -1 || lastProcNum == -1)
            {
                return ret;
            }
            else idlMethodCnt = lastProcNum - firstProcNum + 1;

            String binaryName = Path.GetFileName(binaryPath);
            String interfaceName = GetInterfaceName(idl);
            if (!File.Exists($"DLLs\\{binaryName}.asm")) return ret;
            using (StreamReader reader = new StreamReader($"DLLs\\{binaryName}.asm"))
            {
                String asm = reader.ReadToEnd();

                if (!asm.Contains("QueryInterface") && !banList.Contains(binaryPath))
                {
                    using (StreamWriter writer = new StreamWriter("BanList", true))
                    {
                        writer.WriteLine(binaryPath);
                    }
                    ResolveMethod.banList.Add(binaryPath);
                }
                asmLines = asm.Split('\n');
            }
            int lineIndex = 0;
            while (true)
            {
                List<String> methods = new List<String> { "QueryInterface", "AddRef", "Release" };
                if (lineIndex >= asmLines.Length) break;
                String line = asmLines[lineIndex++];

                if (line.Contains($"`vftable'{{for `{interfaceName}'}}"))
                {
                    int nowIndex = lineIndex;
                    bool flag = false;
                    while (true)
                    {
                        line = asmLines[lineIndex++];
                        if (line.Contains("?Release")) break;
                        if (lineIndex > nowIndex + 8)
                        {
                            lineIndex = nowIndex;
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                    while (true)
                    {
                        nowIndex = lineIndex;
                        line = asmLines[lineIndex++].Trim(' ');
                        if (line.Contains("?Release")) continue;
                        if (line.StartsWith("dq offset ??"))
                        {
                            break;
                        }
                        else if (line.StartsWith("dq offset ?"))
                        {
                            String[] parts;
                            String className = "", methodName = "";
                            try
                            {
                                parts = line.Split(new String[] { " ; " }, StringSplitOptions.None)[1].
                                    Split(new String[] { "::" }, StringSplitOptions.None);
                                className = String.Join("::", parts, 0, parts.Length - 1);
                                methodName = line.Split(new String[] { "dq offset ?" }, StringSplitOptions.None)[1].Split('@')[0];
                            }
                            catch (IndexOutOfRangeException ex)
                            {
                                methodName = line.Split('?')[1].Split('@')[0];
                            }
                            methods.Add($"/*{className}*/{methodName}");
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (methods.Count >= 4 && (methods.Count - (lastProcNum - firstProcNum + 1) - 3) >= 0 && (methods.Count - (lastProcNum - firstProcNum + 1) - 3) <= 1)
                    {
                        ret.Add(methods);
                    }
                }
            }
            return ret;
        }

        // Parsing .asm file to find COM vtables and get method names.
        // This method will search all vtables, and compare method count, each method's parameter count with idl to find appropriate interface.
        public static List<List<String>> GetMethodsFromCandidates(String binaryPath, String idl)
        {

            List<List<String>> ret = new List<List<String>>();
            List<String> methodsFromIdl = new List<String>();
            String[] asmLines = null;
            String[] idlLines = idl.Split('\n');

            int firstProcNum = GetFirstProcNumFromIdl(idl);
            int lastProcNum = GetLastProcNumFromIdl(idl);
            int idlMethodCnt;
            if (firstProcNum == -1 || lastProcNum == -1)
            {
                return ret;
            }
            else idlMethodCnt = lastProcNum - firstProcNum + 1;

            foreach (String line in idlLines)
            {
                if (line.Contains("HRESULT")) methodsFromIdl.Add(line);
            }

            String binaryName = Path.GetFileName(binaryPath);
            String interfaceName = GetInterfaceName(idl);
            if (!File.Exists($"DLLs\\{binaryName}.asm")) return ret;
            using (StreamReader reader = new StreamReader($"DLLs\\{binaryName}.asm"))
            {
                asmLines = reader.ReadToEnd().Split('\n');
            }

            int lineIndex = 0;
            while (true)
            {
                List<String> methods = new List<String> { "QueryInterface", "AddRef", "Release" };
                if (lineIndex >= asmLines.Length) break;
                String line = asmLines[lineIndex++];

                if (line.Contains($"`vftable'"))
                {
                    line = asmLines[lineIndex++];
                    if (line.Contains("QueryInterface"))
                    {
                        int idlIndex = 0;
                        int startIndex = 3;
                        int diffCnt = 0;
                        int nowIndex = lineIndex;
                        int diffBase = 0;
                        bool flag = false;
                        while (true)
                        {
                            line = asmLines[lineIndex++];
                            if (line.Contains("?Release")) break;
                            if (lineIndex > nowIndex + 8)
                            {
                                lineIndex = nowIndex;
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            continue;
                        }
                        while (true)
                        {
                            nowIndex = lineIndex;
                            line = asmLines[lineIndex++].Trim(' ');
                            if (line.Contains("?Release")) continue;
                            if (line.StartsWith("dq offset ??"))
                            {
                                lineIndex = nowIndex;
                                break;
                            }
                            else if (line.StartsWith("dq offset ?"))
                            {
                                if (startIndex < firstProcNum)
                                {
                                    startIndex++;
                                    continue;
                                }
                                if (idlIndex == methodsFromIdl.Count)
                                {
                                    flag = true;
                                    break;
                                }
                                int pCnt1 = 0, pCnt2 = 0;
                                try
                                {
                                    pCnt2 = CountParameters(methodsFromIdl[idlIndex].Trim());
                                    pCnt1 = CountParameters(line.Split(new String[] { " ; " }, StringSplitOptions.None)[1].Trim());
                                }
                                catch (IndexOutOfRangeException ex)
                                {
                                    pCnt1 = pCnt2;
                                }

                                if (pCnt1 != pCnt2)
                                {
                                    if (methodsFromIdl.Count == 1)
                                    {
                                        flag = true;
                                        break;
                                    }
                                    diffCnt++;
                                    if (methodsFromIdl.Count < 10) diffBase = 1;
                                    else diffBase = methodsFromIdl.Count / 10;
                                    if (diffCnt > diffBase)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }

                                String[] parts;
                                String className = "", methodName = "";
                                try
                                {
                                    parts = line.Split(new String[] { " ; " }, StringSplitOptions.None)[1].
                                        Split(new String[] { "::" }, StringSplitOptions.None);
                                    className = String.Join("::", parts, 0, parts.Length - 1);
                                    methodName = line.Split(new String[] { "dq offset ?" }, StringSplitOptions.None)[1].Split('@')[0];
                                }
                                catch (IndexOutOfRangeException ex)
                                {
                                    methodName = line.Split('?')[1].Split('@')[0];
                                }
                                methods.Add($"/*{className}*/{methodName}");
                                idlIndex++;
                            }
                            else if (line.Contains("purecall"))
                            {
                                lineIndex = nowIndex;
                                break;
                            }
                            else
                            {
                                lineIndex = nowIndex;
                                break;
                            }
                        }
                        if (methods.Count - methodsFromIdl.Count - 3 > 1 || methods.Count - methodsFromIdl.Count - 3 < 0) flag = true;
                        if (methodsFromIdl.Count >= 4 && ((methods.Count - methodsFromIdl.Count - 3) >= 0 && (methods.Count - methodsFromIdl.Count - 3) <= 1)) flag = false;
                        if (methods.Count == 5 && methods[3].EndsWith("CreateInstance") && methods[4].EndsWith("LockServer")) flag = true;
                        if (methods.Count >= 20 && ((methods.Count - methodsFromIdl.Count - 3) >= 0 && (methods.Count - methodsFromIdl.Count - 3) <= 5)) flag = true;
                        if (!flag)
                        {
                            ret.Add(methods);
                        }
                    }
                }
            }
            return ret;
        }

        // Counts parameters from each methods in idl.
        public static int CountParameters(string functionDeclaration)
        {
            if (functionDeclaration.Trim().EndsWith("(void)"))
                return 0;

            string functionDefinition = Regex.Replace(functionDeclaration, @"<.*?>", "");

            Match match = Regex.Match(functionDefinition, @"([\w:~]+|`.*?')\s*\((.*?)\)\s*(?:const)?(?:override)?;?$");
            if (!match.Success)
                return 0;

            string functionName = match.Groups[1].Value;
            string parameters = match.Groups[2].Value;

            if (string.IsNullOrWhiteSpace(parameters))
                return 0;

            parameters = Regex.Replace(parameters, @"\[([^\]]*)\]", m => m.Groups[0].Value.Replace(",", "<COMMA>"));

            parameters = Regex.Replace(parameters, @"<([^>]*)>", m => m.Groups[0].Value.Replace(",", "<COMMA>"));

            var paramList = parameters.Split(',').Select(p => p.Replace("<COMMA>", ",")).ToList();

            return paramList.Count(p => !string.IsNullOrWhiteSpace(p));
        }

        // Changes idl's method name from Proc{n} to real method name and returns it.
        public static String ConvertMethodName(String idl, List<String> methods)
        {
            if (methods.Count == 0) return idl + "\n";
            String result = "";
            int index = 3;
            String[] lines = idl.Split('\n');

            String funcStyle;

            if (lines[0][0] == '[')
            {
                funcStyle = "HRESULT Proc";
            }
            else
            {
                funcStyle = "__stdcall Proc";
            }
            foreach (String line in lines)
            {
                String now = line + "\n";
                if (line.Contains(funcStyle))
                {
                    if (index == methods.Count) continue;
                    now = line.Split(new String[] { " Proc" }, StringSplitOptions.None)[0] + " ";
                    now += methods[index++];
                    foreach (String tmp in line.Split('(').Skip(1).ToArray())
                    {
                        now += "(" + tmp;
                    }
                    now += "\n";
                }
                result += now;
            }
            return result;
        }

        // Initialize banList. BanList is a DLL/EXE list that will not be parsed.
        public static void BanListInit()
        {
            if (!File.Exists("BanList"))
            {
                String template = "C:\\WINDOWS\\SYSTEM32\\MSASN1.dll\r\nC:\\WINDOWS\\SYSTEM32\\dxcore.dll\r\nC:\\WINDOWS\\SYSTEM32\\twinapi.appcore.dll\r\nC:\\WINDOWS\\SYSTEM32\\wevtapi.dll\r\nC:\\WINDOWS\\SYSTEM32\\winsta.dll\r\nC:\\WINDOWS\\System32\\WINSTA.dll\r\nC:\\WINDOWS\\System32\\profapi.dll\r\nC:\\WINDOWS\\system32\\MSASN1.dll\r\nC:\\WINDOWS\\system32\\ncryptprov.dll\r\nC:\\Windows\\System32\\Microsoft.Bluetooth.Proxy.dll\r\nC:\\Windows\\System32\\SspiCli.dll\r\nC:\\Windows\\System32\\Windows.Security.Authentication.OnlineId.dll\r\nC:\\Windows\\System32\\XmlLite.dll\r\nC:\\Windows\\System32\\msxml6.dll\r\nC:\\Windows\\System32\\vaultcli.dll\r\nc:\\windows\\system32\\BrokerLib.dll\r\nc:\\windows\\system32\\PROPSYS.dll\r\nc:\\windows\\system32\\WMICLNT.dll\r\nc:\\windows\\system32\\fwbase.dll\r\nc:\\windows\\system32\\wlanapi.dll\r\nC:\\WINDOWS\\SYSTEM32\\IPHLPAPI.DLL\r\nC:\\WINDOWS\\SYSTEM32\\bi.dll\r\nC:\\WINDOWS\\SYSTEM32\\capauthz.dll\r\nC:\\WINDOWS\\System32\\CRYPTBASE.DLL\r\nC:\\WINDOWS\\System32\\NTASN1.dll\r\nC:\\WINDOWS\\System32\\SETUPAPI.dll\r\nC:\\WINDOWS\\System32\\SHLWAPI.dll\r\nC:\\WINDOWS\\System32\\SspiCli.dll\r\nC:\\WINDOWS\\System32\\fwpuclnt.dll\r\nC:\\WINDOWS\\System32\\ncrypt.dll\r\nC:\\WINDOWS\\System32\\netutils.dll\r\nC:\\WINDOWS\\System32\\sspicli.dll\r\nC:\\WINDOWS\\System32\\wkscli.dll\r\nC:\\Windows\\System32\\AppXDeploymentClient.dll\r\nc:\\windows\\system32\\WppRecorderUM.dll\r\nc:\\windows\\system32\\netutils.dll\r\nC:\\WINDOWS\\SYSTEM32\\netjoin.dll\r\nC:\\WINDOWS\\SYSTEM32\\netutils.dll\r\nC:\\WINDOWS\\System32\\CRYPTBASE.dll\r\nC:\\WINDOWS\\System32\\DPAPI.DLL\r\nC:\\WINDOWS\\System32\\SHCORE.dll\r\nC:\\WINDOWS\\System32\\SHELL32.dll\r\nC:\\WINDOWS\\System32\\netprofm.dll\r\nC:\\WINDOWS\\system32\\execmodelproxy.dll\r\nC:\\Windows\\System32\\Windows.Networking.Connectivity.dll\r\nC:\\Windows\\System32\\Windows.Web.dll\r\nC:\\Windows\\System32\\iertutil.dll\r\nC:\\Windows\\System32\\msvcp110_win.dll\r\nC:\\Windows\\System32\\netutils.dll\r\nC:\\Windows\\System32\\srvcli.dll\r\nC:\\Windows\\System32\\usermgrproxy.dll\r\nc:\\windows\\system32\\DNSAPI.dll\r\nc:\\windows\\system32\\DSROLE.dll\r\nc:\\windows\\system32\\MobileNetworking.dll\r\nc:\\windows\\system32\\SYSNTFY.dll\r\nc:\\windows\\system32\\WINSTA.dll\r\nc:\\windows\\system32\\WTSAPI32.dll\r\nc:\\windows\\system32\\fwpuclnt.dll\r\nc:\\windows\\system32\\AUTHZ.dll\r\nc:\\windows\\system32\\NTASN1.dll\r\nc:\\windows\\system32\\ncrypt.dll\r\nC:\\WINDOWS\\SYSTEM32\\wlanapi.dll\r\nC:\\WINDOWS\\System32\\DEVOBJ.dll\r\nC:\\WINDOWS\\system32\\ncryptsslp.dll\r\nC:\\WINDOWS\\system32\\schannel.DLL\r\nC:\\WINDOWS\\system32\\sspicli.dll\r\nC:\\Windows\\System32\\taskschd.dll\r\nc:\\windows\\system32\\WINNSI.DLL\r\nC:\\WINDOWS\\SYSTEM32\\SspiCli.dll\r\nC:\\WINDOWS\\SYSTEM32\\profapi.dll\r\nC:\\WINDOWS\\System32\\IMM32.DLL\r\nC:\\WINDOWS\\System32\\coml2.dll\r\nC:\\Windows\\System32\\CapabilityAccessManagerClient.dll\r\nC:\\Windows\\System32\\Windows.StateRepositoryPS.dll\r\nC:\\WINDOWS\\SYSTEM32\\windows.staterepositoryclient.dll\r\nC:\\WINDOWS\\System32\\WINTRUST.dll\r\nC:\\WINDOWS\\system32\\CRYPTBASE.dll\r\nC:\\Windows\\System32\\WinTypes.dll\r\nc:\\windows\\system32\\webio.dll\r\nC:\\WINDOWS\\SYSTEM32\\MobileNetworking.dll\r\nC:\\WINDOWS\\System32\\ADVAPI32.dll\r\nC:\\Windows\\System32\\twinapi.appcore.dll\r\nc:\\windows\\system32\\UMPDC.dll\r\nC:\\WINDOWS\\SYSTEM32\\cryptsp.dll\r\nC:\\Windows\\System32\\OneCoreCommonProxyStub.dll\r\nc:\\windows\\system32\\WINHTTP.dll\r\nc:\\windows\\system32\\profapi.dll\r\nC:\\WINDOWS\\System32\\npmproxy.dll\r\nc:\\windows\\system32\\SspiCli.dll\r\nc:\\windows\\system32\\msvcp110_win.dll\r\nC:\\WINDOWS\\System32\\MSASN1.dll\r\nc:\\windows\\system32\\DEVOBJ.dll\r\nC:\\WINDOWS\\SYSTEM32\\WINSTA.dll\r\nC:\\Windows\\System32\\OneCoreUAPCommonProxyStub.dll\r\nC:\\Windows\\System32\\rasadhlp.dll\r\nC:\\WINDOWS\\SYSTEM32\\windows.staterepositorycore.dll\r\nC:\\WINDOWS\\System32\\shlwapi.dll\r\nC:\\WINDOWS\\system32\\rsaenh.dll\r\nc:\\windows\\system32\\USERENV.dll\r\nC:\\WINDOWS\\SYSTEM32\\DNSAPI.dll\r\nC:\\WINDOWS\\SYSTEM32\\WINNSI.DLL\r\nC:\\WINDOWS\\SYSTEM32\\policymanager.dll\r\nC:\\WINDOWS\\SYSTEM32\\rmclient.dll\r\nC:\\WINDOWS\\SYSTEM32\\windows.storage.dll\r\nC:\\WINDOWS\\SYSTEM32\\dhcpcsvc.DLL\r\nC:\\WINDOWS\\SYSTEM32\\dhcpcsvc6.DLL\r\nC:\\WINDOWS\\SYSTEM32\\gpapi.dll\r\nC:\\WINDOWS\\SYSTEM32\\usermgrcli.dll\r\nC:\\WINDOWS\\SYSTEM32\\wtsapi32.dll\r\nC:\\WINDOWS\\SYSTEM32\\wintypes.dll\r\nC:\\WINDOWS\\System32\\ole32.dll\r\nc:\\windows\\system32\\IPHLPAPI.DLL\r\nC:\\WINDOWS\\SYSTEM32\\UMPDC.dll\r\nC:\\WINDOWS\\SYSTEM32\\ntmarta.dll\r\nC:\\WINDOWS\\system32\\mswsock.dll\r\nC:\\WINDOWS\\System32\\svchost.exe\r\nC:\\WINDOWS\\System32\\CRYPT32.dll\r\nC:\\WINDOWS\\SYSTEM32\\powrprof.dll\r\nC:\\WINDOWS\\System32\\NSI.dll\r\nC:\\WINDOWS\\SYSTEM32\\cfgmgr32.dll\r\nC:\\WINDOWS\\System32\\WS2_32.dll\r\nC:\\WINDOWS\\System32\\shcore.dll\r\nC:\\WINDOWS\\System32\\advapi32.dll\r\nC:\\WINDOWS\\system32\\svchost.exe\r\nC:\\WINDOWS\\System32\\OLEAUT32.dll\r\nC:\\WINDOWS\\System32\\clbcatq.dll\r\nC:\\WINDOWS\\System32\\user32.dll\r\nC:\\WINDOWS\\System32\\GDI32.dll\r\nC:\\WINDOWS\\System32\\gdi32full.dll\r\nC:\\WINDOWS\\System32\\win32u.dll\r\nC:\\WINDOWS\\SYSTEM32\\kernel.appcore.dll\r\nC:\\WINDOWS\\System32\\bcryptPrimitives.dll\r\nC:\\WINDOWS\\SYSTEM32\\ntdll.dll\r\nC:\\WINDOWS\\System32\\KERNEL32.DLL\r\nC:\\WINDOWS\\System32\\KERNELBASE.dll\r\nC:\\WINDOWS\\System32\\RPCRT4.dll\r\nC:\\WINDOWS\\System32\\bcrypt.dll\r\nC:\\WINDOWS\\System32\\combase.dll\r\nC:\\WINDOWS\\System32\\msvcp_win.dll\r\nC:\\WINDOWS\\System32\\msvcrt.dll\r\nC:\\WINDOWS\\System32\\sechost.dll\r\nC:\\WINDOWS\\System32\\ucrtbase.dll\r\nC:\\WINDOWS\\System32\\d3d11.dll\r\nC:\\WINDOWS\\System32\\d2d1.dll\r\nc:\\windows\\system32\\CLIPC.dll\r\nC:\\Windows\\System32\\wuapi.dll\r\nC:\\WINDOWS\\uus\\AMD64\\uusbrain.dll\r\nC:\\Windows\\System32\\wups.dll\r\nC:\\WINDOWS\\SYSTEM32\\gamestreamingext.dll\r\nc:\\windows\\system32\\VERSION.dll\r\nC:\\WINDOWS\\SYSTEM32\\wiatrace.dll\r\nC:\\WINDOWS\\system32\\msv1_0.DLL\r\nC:\\WINDOWS\\system32\\NtlmShared.dll\r\nC:\\WINDOWS\\SYSTEM32\\deviceassociation.dll\r\nC:\\WINDOWS\\SYSTEM32\\webservices.dll\r\nC:\\WINDOWS\\SYSTEM32\\HTTPAPI.dll\r\nC:\\WINDOWS\\System32\\verifier.dll\r\nc:\\windows\\system32\\logoncli.dll\r\nc:\\windows\\system32\\NETAPI32.dll\r\nC:\\WINDOWS\\SYSTEM32\\PeerDist.dll\r\nc:\\windows\\system32\\VirtDisk.dll\r\nc:\\windows\\system32\\SXSHARED.dll\r\nC:\\WINDOWS\\system32\\defragproxy.dll\r\nC:\\WINDOWS\\system32\\ESENT.dll\r\nC:\\Windows\\System32\\TieringEngineProxy.dll\r\nc:\\windows\\system32\\profsvc.dll\r\nC:\\WINDOWS\\System32\\SAMLIB.dll\r\nC:\\WINDOWS\\SYSTEM32\\profext.dll\r\nC:\\WINDOWS\\SYSTEM32\\AcLayers.DLL\r\nC:\\WINDOWS\\SYSTEM32\\HID.DLL\r\nC:\\WINDOWS\\SYSTEM32\\directxdatabasehelper.dll\r\nC:\\Windows\\System32\\PerceptionSimulation\\SixDofControllerManager.ProxyStubs.dll\r\nC:\\Windows\\System32\\PerceptionSimulation\\VirtualDisplayManager.ProxyStubs.dll\r\nC:\\WINDOWS\\system32\\sxproxy.dll\r\nC:\\WINDOWS\\System32\\bcd.dll\r\nC:\\WINDOWS\\System32\\VssTrace.DLL\r\nc:\\windows\\system32\\securityhealthservice.exe\r\nc:\\windows\\system32\\ESENT.dll\r\nc:\\windows\\system32\\dsclient.dll\r\nC:\\Windows\\System32\\WalletProxy.dll\r\nc:\\windows\\system32\\OneX.DLL\r\nc:\\windows\\system32\\eappprxy.dll\r\nc:\\windows\\system32\\WLANSEC.dll\r\nc:\\windows\\system32\\WMI.dll\r\nC:\\WINDOWS\\System32\\wlansvcpal.dll\r\nC:\\WINDOWS\\System32\\TetheringIeProvider.dll\r\nC:\\WINDOWS\\SYSTEM32\\wlgpclnt.dll\r\nC:\\WINDOWS\\system32\\kerberos.DLL\r\nC:\\WINDOWS\\system32\\Kerb3961.dll\r\nC:\\WINDOWS\\SYSTEM32\\SystemEventsBrokerClient.dll\r\nc:\\windows\\system32\\nvagent.dll\r\nc:\\windows\\system32\\NetSetupApi.dll\r\nC:\\WINDOWS\\System32\\vmsifproxystub.dll\r\nc:\\windows\\system32\\lfsvc.dll\r\nC:\\Windows\\System32\\LocationFrameworkPS.dll\r\nC:\\WINDOWS\\system32\\mi.dll\r\nC:\\WINDOWS\\system32\\fveapi.dll\r\nC:\\WINDOWS\\system32\\cscapi.dll\r\nC:\\WINDOWS\\SYSTEM32\\samcli.dll\r\nC:\\WINDOWS\\SYSTEM32\\NCObjAPI.DLL\r\nC:\\WINDOWS\\System32\\EventAggregation.dll\r\nC:\\WINDOWS\\system32\\keepaliveprovider.dll\r\nC:\\WINDOWS\\System32\\winsqlite3.dll\r\nC:\\WINDOWS\\System32\\LINKINFO.dll\r\nC:\\WINDOWS\\system32\\SFC.DLL\r\nC:\\WINDOWS\\system32\\sfc_os.DLL\r\nc:\\windows\\system32\\dsclient.dllc:\\windows\\system32\\wpnuserservice.dll\r\nC:\\Windows\\System32\\StateRepository.Core.dll\r\nc:\\windows\\system32\\wpnuserservice.dll\r\nC:\\WINDOWS\\SYSTEM32\\edputil.dll\r\nC:\\WINDOWS\\system32\\Secur32.dll\r\nC:\\WINDOWS\\SYSTEM32\\bcp47mrm.dll\r\nC:\\WINDOWS\\System32\\TimeBrokerClient.dll\r\nC:\\Windows\\System32\\ShellCommonCommonProxyStub.dll";
                using (StreamWriter writer = new StreamWriter("BanList"))
                {
                    writer.Write(template);
                    writer.Flush();
                }
            }
            banList = new List<string>();
            using (StreamReader reader = new StreamReader("BanList"))
            {
                while (true)
                {
                    String nowLine = reader.ReadLine();
                    if (nowLine == null) break;
                    banList.Add(nowLine);
                }
            }
        }
    }
}
