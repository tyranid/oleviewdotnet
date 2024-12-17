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
        public static List<String> banList = null;

        // Find IDA path from Registry(Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache).
        // If failed to find, create IDAPathForm to get IDA Path.
        public static String GetIDAT()
        {

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
                                    ProgramSettings.IDAPath = fileName;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (ProgramSettings.IDAPath == null)
            {

                IDAPathForm iDAPathForm = new IDAPathForm();
                iDAPathForm.ShowDialog();
            }

            return ProgramSettings.IDAPath;
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
            if (File.Exists($"DLLs\\{binaryName}.asm")) return true;
            CopyDLL(binaryPath);
            Process process = new Process();
            if (ProgramSettings.IDAPath == null || !File.Exists(ProgramSettings.IDAPath)) GetIDAT();
            process.StartInfo.FileName = ProgramSettings.IDAPath;
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
                process.Dispose();
                File.Delete($"DLLs\\{binaryName}");
                File.Delete($"DLLs\\{binaryName}.i64");
                return false;
            }
            process.Dispose();
            File.Delete($"DLLs\\{binaryName}");
            File.Delete($"DLLs\\{binaryName}.i64");
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
                    using (StreamWriter writer = new StreamWriter("BanList.txt", true))
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
            banList = new List<string>();
            using (StreamReader reader = new StreamReader("BanList.txt"))
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
