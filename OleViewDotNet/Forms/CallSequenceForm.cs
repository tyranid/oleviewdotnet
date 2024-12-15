using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// When we wants to get WalletService's IWalletCustomProperty interface,
// we must get it by IWallet → IWalletTransactionManager → IWalletItem → IWalletCustomProperty. (not a unique path)
// This form will analyze call sequence like above and show it.

namespace OleViewDotNet.Forms
{
    public partial class CallSequenceForm : Form
    {
        Dictionary<String, List<String>> interfaces = null;
        List<List<String>> sequence;

        public CallSequenceForm()
        {
            String[] fileNames = Directory.GetFiles("interfaces\\sequence");

            if (fileNames.Length == 0)
            {
                MessageBox.Show("View Proxy Library First.");
                return;
            }
            InitializeComponent();
            if (interfaces == null)
            {
                interfaces = new Dictionary<String, List<String>>();
                var stringList = new List<String>();
                foreach (String file in fileNames)
                {
                    String idl = null;
                    using (StreamReader sr = new StreamReader(file))
                    {
                        idl = sr.ReadToEnd();
                    }
                    if (Path.GetFileName(file) == "now")
                    {
                        textBox1.Text = idl;
                        continue;
                    }
                    String[] lines = idl.Split('\n');
                    String interfaceName = null;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        String line = lines[i].Trim();
                        if (line.StartsWith("interface"))
                        {
                            interfaceName = line.Split(' ')[1];
                            break;
                        }
                    }
                    stringList.Add(interfaceName);
                    interfaces[interfaceName] = new List<String>();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        String line = lines[i].Trim();
                        if (line.StartsWith("HRESULT"))
                        {
                            List<String> parameters = GetParameters(line);
                            for (int j = 1; j < parameters.Count; j++)
                            {
                                String[] param = parameters[j].Trim().Split(' ');
                                if (param[0].Contains("out") && !param[param.Length - 2].StartsWith("int") &&
                                    !param[param.Length - 2].StartsWith("HSTRING") && !param[param.Length - 2].StartsWith("wchar") &&
                                    !param[param.Length - 2].StartsWith("GUID") && !param[param.Length - 2].StartsWith("byte") &&
                                    !param[param.Length - 2].StartsWith("__int") && !param[param.Length - 2].StartsWith("uint") &&
                                    !param[param.Length - 2].StartsWith("Struct") && !param[param.Length - 2].ToLower().StartsWith("handle") &&
                                    !param[param.Length - 2].StartsWith("float") && !param[param.Length - 2].StartsWith("double") &&
                                    !param[param.Length - 2].StartsWith("char") && !param[param.Length - 2].StartsWith("HWND") &&
                                    !param[param.Length - 2].StartsWith("handle") && !param[param.Length - 2].StartsWith("BSTR") &&
                                    !param[param.Length - 2].StartsWith("short") && !param[param.Length - 2].StartsWith("VARIANT"))
                                {
                                    param[param.Length - 2] = param[param.Length - 2].Replace("*", "");
                                    if (!interfaces.Keys.Contains(param[param.Length - 2]))
                                    {
                                        interfaces[param[param.Length - 2]] = new List<string>();
                                    }
                                    interfaces[interfaceName].Add(param[param.Length - 2]);
                                }
                            }
                        }
                    }
                }
                stringList.Sort();
                comboBox1.DataSource = stringList;
            }

        }

        // Get parameters from idl methods.
        public List<String> GetParameters(string functionDeclaration)
        {
            if (functionDeclaration.Trim().EndsWith("(void)"))
                return new List<string>();
            string functionDefinition = Regex.Replace(functionDeclaration, @"<.*?>", "");
            Match match = Regex.Match(functionDefinition, @"([\w:~]+|`.*?')\s*\((.*?)\)\s*(?:const)?(?:override)?;?$");
            if (!match.Success)
                return new List<string>();

            string functionName = match.Groups[1].Value;
            string parameters = match.Groups[2].Value;

            if (string.IsNullOrWhiteSpace(parameters))
                return new List<string>();
            parameters = Regex.Replace(parameters, @"\[([^\]]*)\]", m => m.Groups[0].Value.Replace(",", "<COMMA>"));

            parameters = Regex.Replace(parameters, @"<([^>]*)>", m => m.Groups[0].Value.Replace(",", "<COMMA>"));

            var paramList = parameters.Split(',').Select(p => p.Replace("<COMMA>", ",")).ToList();
            paramList.Insert(0, functionName);
            return paramList;
        }
        
        // Find sequence by Search method and store with Dictionary structure.
        public void GetSequence(String baseInterface)
        {
            if (!interfaces.Keys.Contains(baseInterface))
            {
                textBox2.Text = $"No Interface \"{baseInterface}\" in Proxy.";
                return;
            }
            sequence = new List<List<string>>();
            for (int i = 0; i < interfaces[baseInterface].Count; i++)
            {
                List<String> nowSequence = new List<String>();
                nowSequence.Add(baseInterface);
                Search(nowSequence, interfaces[baseInterface][i]);
            }
            if (sequence.Count == 0)
            {
                textBox2.Text = "No Results.";
                return;
            }
            textBox2.Text = "";
            foreach (List<String> now in sequence)
            {
                textBox2.Text += String.Join(" → ", now) + "\r\n";
            }
        }

        // Parse interface's each method and find interface parameters recursively.
        public void Search(List<String> nowSequence, String next)
        {

            if (interfaces[next].Count == 0)
            {
                nowSequence.Add(next);
                bool flag = true;
                foreach (List<String> now in sequence)
                {
                    if (now.SequenceEqual(nowSequence))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) sequence.Add(nowSequence);
                return;
            }

            if (nowSequence.Contains(next))
            {
                bool flag = true;
                foreach (List<String> now in sequence)
                {
                    if (now.SequenceEqual(nowSequence))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) sequence.Add(nowSequence);
                return;
            }
            nowSequence.Add(next);
            foreach (String now in interfaces[next])
            {
                List<String> newSequence = nowSequence.ToList();
                Search(newSequence, now);
            }
            return;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            String interfaceName = this.comboBox1.Text;
            GetSequence(interfaceName);
        }
    }
}
