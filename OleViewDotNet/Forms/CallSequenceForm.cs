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
                            List<String> parameters = GetParameters(line.Substring(line.IndexOf('(') + 1));
                            for (int j = 0; j < parameters.Count; j++)
                            {
                                String[] param = parameters[j].Trim().Split(' ');
                                if (!param[0].StartsWith("[out") || !param[1].Contains("**")) continue;
                                if (param[1].Contains("wchar")) continue;
                                param[1] = param[1].Replace("*", "");
                                param[1] = param[1].Replace("<COMMA>", ",").Replace("<SPACE>", " ");
                                if (!interfaces.Keys.Contains(param[1]))
                                {
                                    interfaces[param[1]] = new List<string>();
                                }
                                interfaces[interfaceName].Add(param[1]);
                            }
                        }
                    }
                }
                stringList.Sort();
                comboBox1.DataSource = stringList;
            }
            this.Show();

        }

        static string ReplaceCommasInBrackets(string text)
        {
            Stack<int> stack = new Stack<int>();
            StringBuilder result = new StringBuilder();
            StringBuilder temp = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (ch == '[')
                {
                    if (stack.Count > 0)
                    {
                        temp.Append(ch);
                    }
                    stack.Push(i);
                }
                else if (ch == ']')
                {
                    if (stack.Count > 1)
                    {
                        temp.Append(ch);
                    }
                    stack.Pop();

                    if (stack.Count == 0)
                    {
                        result.Append("[");
                        result.Append(temp.ToString().Replace(",", "<COMMA>").Replace(" ","<SPACE>"));
                        result.Append("]");
                        temp.Clear();
                    }
                }
                else if (stack.Count > 0)
                {
                    temp.Append(ch);
                }
                else
                {
                    result.Append(ch);
                }
            }

            return result.ToString();
        }

        // Get parameters from idl methods.
        public List<String> GetParameters(string functionDeclaration)
        {
            if (functionDeclaration.Trim().EndsWith("(void)"))
                return new List<string>();

            String functionDefinition = ReplaceCommasInBrackets(functionDeclaration);
            return functionDefinition.Split(',').ToList();
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
