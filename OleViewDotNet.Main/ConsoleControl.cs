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


using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class ConsoleControl : UserControl
    {
        private ConsoleImpl _console;
        private ConsoleTextWriter _error;
        private ConsoleTextWriter _output;
        private LockedQueue<string> _input;
        private CancellationTokenSource _tokenSource;
        private Thread _consoleThread;
        private bool _startedThread;
        private List<string> _history;
        private int _currHistory;

        internal sealed class PythonConsoleHost : ConsoleHost
        {
            ConsoleControl _control;

            internal PythonConsoleHost(ConsoleControl control)
            {
                _control = control;
            }

            protected override IConsole CreateConsole(ScriptEngine engine, CommandLine commandLine, ConsoleOptions options)
            {
                return _control.Console;
            }

            protected override CommandLine CreateCommandLine()
            {
                return new PythonCommandLine();
            }

            protected override OptionsParser CreateOptionsParser()
            {
                return new PythonOptionsParser();
            }

            protected override ScriptRuntimeSetup CreateRuntimeSetup()
            {
                ScriptRuntimeSetup setup = ScriptRuntimeSetup.ReadConfiguration();
                foreach (LanguageSetup setup2 in setup.LanguageSetups)
                {
                    if (setup2.FileExtensions.Contains(".py"))
                    {
                        setup2.Options["SearchPaths"] = new string[0];
                    }
                }

                return setup;
            }

            protected override void ExecuteInternal()
            {
                PythonContext ctx = HostingHelpers.GetLanguageContext(Engine) as PythonContext;

                Runtime.IO.SetOutput(new MemoryStream(), _control.Output);
                Runtime.IO.SetErrorOutput(new MemoryStream(), _control.Error);

                ctx.SetModuleState(typeof(ScriptEngine), Engine);
                Runtime.LoadAssembly(typeof(MainForm).Assembly);
                ICollection<string> searchPaths = Engine.GetSearchPaths();
                string item = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonLib");
                searchPaths.Add(item);
                Engine.SetSearchPaths(searchPaths);

                base.ExecuteInternal();
            }

            protected override void ParseHostOptions(string[] args)
            {
                foreach (string str in args)
                {
                    Options.IgnoredArgs.Add(str);
                }
            }

            protected override Type Provider
            {
                get
                {
                    return typeof(PythonContext);
                }
            }
        }

        private class ConsoleTextWriter : TextWriter
        {
            private StringBuilder _builder;
            private ConsoleControl _control;
            private bool _error;

            public ConsoleTextWriter(ConsoleControl control, bool error)
            {
                _builder = new StringBuilder();
                _control = control;
                _error = error;
            }

            public override void Write(char value)
            {
                _builder.Append(value);

                if (value == '\n')
                {
                    if (_error)
                    {
                        _control.WriteErrorString(_builder.ToString());
                    }
                    else
                    {
                        _control.WriteOutputString(_builder.ToString());
                    }
                    _builder.Clear();
                }
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        private sealed class ConsoleImpl : IConsole
        {
            private LockedQueue<string> _input;
            private ConsoleControl _control;

            internal ConsoleImpl(ConsoleControl control, LockedQueue<string> input)
            {
                _control = control;
                _input = input;
            }

            public TextWriter ErrorOutput
            {
                get;
                set;
            }

            public TextWriter Output
            {
                get;
                set;
            }

            public string ReadLine(int autoIndentSize)
            {
                try
                {
                    string line = _input.Dequeue();

                    return String.Empty.PadLeft(autoIndentSize) + line;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }

            public void Write(string text, Style style)
            {
                switch (style)
                {
                    case Style.Error:
                        _control.WriteErrorString(text);
                        break;
                    case Style.Out:
                        _control.WriteOutputString(text);
                        break;
                    case Style.Warning:
                        _control.WriteErrorString(text);
                        break;
                    case Style.Prompt:
                        _control.SetPrompt(text);
                        break;
                }
            }

            public void WriteLine()
            {
                _control.WriteOutputString(Environment.NewLine);
            }

            public void WriteLine(string text, Style style)
            {
                Write(text + Environment.NewLine, style);
            }
        }

        private void AddText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddText), text);
            }
            else
            {
                if (!richTextBoxOutput.IsDisposed)
                {
                    richTextBoxOutput.Text += text;
                    richTextBoxOutput.SelectionStart = richTextBoxOutput.Text.Length;
                    richTextBoxOutput.ScrollToCaret();
                }
            }
        }

        private void SetPrompt(string prompt)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetPrompt), prompt);
            }
            else
            {
                lblPrompt.Text = prompt;
            }
        }

        private void WriteErrorString(string error)
        {
            AddText(error);
        }

        private void WriteOutputString(string output)
        {
            AddText(output);
        }

        public IConsole Console
        {
            get
            {
                return _console;
            }
        }

        public TextWriter Error
        {
            get { return _error; }
        }

        public TextWriter Output
        {
            get { return _output; }
        }

        public ConsoleControl()
        {
            InitializeComponent();
            _tokenSource = new CancellationTokenSource();
            _input = new LockedQueue<string>(-1, _tokenSource.Token);
            _error = new ConsoleTextWriter(this, true);
            _output = new ConsoleTextWriter(this, false);
            _console = new ConsoleImpl(this, _input);
            _history = new List<string>();
            this.Disposed += ConsoleControl_Disposed;
            _consoleThread = new Thread(consoleThread_DoWork);
        }

        void ConsoleControl_Disposed(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            try
            {
                _consoleThread.Abort(new KeyboardInterruptException());
            }
            catch (ThreadStateException)
            {
            }
        }

        private void UpdateCommandFromHistory()
        {
            textBoxCommand.Text = _history[_currHistory];
            textBoxCommand.SelectionStart = _history[_currHistory].Length;
        }

        private void AddToHistory(string line)
        {
            if ((_history.Count == 0) || (_history[_history.Count - 1] != line))
            {
                _history.Add(line);
                _currHistory = _history.Count;
            }
        }

        private void textBoxCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
            {
                WriteOutputString(lblPrompt.Text + " " + textBoxCommand.Text + Environment.NewLine);
                _input.Enqueue(textBoxCommand.Text);
                AddToHistory(textBoxCommand.Text);
                textBoxCommand.Text = String.Empty;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_currHistory > 0)
                {
                    _currHistory--;
                    UpdateCommandFromHistory();
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (_currHistory < _history.Count - 1)
                {
                    _currHistory++;
                    UpdateCommandFromHistory();
                }
                e.Handled = true;
            }
        }


        [STAThread]
        private void consoleThread_DoWork()
        {
            try
            {
                if (Environment.GetEnvironmentVariable("TERM") == null)
                {
                    Environment.SetEnvironmentVariable("TERM", "dumb");
                }
                new PythonConsoleHost(this).Run(new string[0]);
            }
            catch (Exception)
            {
            }
        }

        private void textBoxCommand_TextPasted(object sender, ClipboardEventArgs e)
        {
            if (e.ClipboardText.Contains("\n"))
            {
                e.Handled = true;

                // Get current text, insert at caret and process, leave any trailing line unpasted
                StringBuilder currText = new StringBuilder(textBoxCommand.Text);

                currText.Insert(textBoxCommand.SelectionStart, e.ClipboardText);

                string[] parts = Regex.Split(currText.ToString(), @"(?<=[\n])");
                foreach (string line in parts)
                {
                    if (line.EndsWith("\n"))
                    {
                        WriteOutputString(lblPrompt.Text + " " + line.TrimEnd('\r', '\n') + Environment.NewLine);
                        _input.Enqueue(line.TrimEnd('\r', '\n'));
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(line))
                        {
                            _input.Enqueue(String.Empty);
                            textBoxCommand.Text = String.Empty;
                        }
                        else
                        {
                            textBoxCommand.Text = line;
                            textBoxCommand.SelectionStart = line.Length;
                        }
                    }
                }
            }
        }

        private void toolStripButtonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                _consoleThread.Abort(new KeyboardInterruptException());
            }
            catch (ThreadStateException)
            {
            }
        }

        private void ConsoleControl_Load(object sender, EventArgs e)
        {
            if (!_startedThread)
            {
                _startedThread = true;
                _consoleThread.Start();
            }
        }
    }
}
