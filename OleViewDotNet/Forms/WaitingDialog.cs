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

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class WaitingDialog : Form
{
    private class ReportProgress : IProgress<Tuple<string, int>>
    {
        private CancellationToken _token;
        private readonly Action<string, int> _report;
        private readonly Func<string, string> _format;

        public void Report(Tuple<string, int> data)
        {
            _token.ThrowIfCancellationRequested();
            _report(_format(data.Item1), data.Item2);
        }

        public ReportProgress(Action<string, int> report, CancellationToken token, Func<string, string> format)
        {
            _token = token;
            _format = format;
            _report = report;
        }
    }

    private readonly ReportProgress m_progress;
    private readonly BackgroundWorker m_worker;
    private readonly CancellationTokenSource m_cancellation;

    public WaitingDialog(Func<IProgress<Tuple<string, int>>, CancellationToken, object> worker_func, Func<string, string> format_label)
    {
        format_label ??= s => $"Currently Processing {s}. Please Wait.";
        m_cancellation = new CancellationTokenSource();
        CancellationToken token = m_cancellation.Token;
        m_progress = new ReportProgress(SetProgress, token, format_label);
        m_worker = new BackgroundWorker();
        m_worker.DoWork += (sender, e) => Result = worker_func(m_progress, token);
        m_worker.RunWorkerCompleted += RunWorkerCompletedCallback;
        InitializeComponent();
        SetProgress(string.Empty, 0);
    }

    public WaitingDialog(Func<IProgress<Tuple<string, int>>, CancellationToken, object> worker_func) 
        : this(worker_func, null)
    {
    }

    private void SetProgress(string value, int percent)
    {
        Action set = () => SetProgress(value, percent);
        if (InvokeRequired)
        {
            Invoke(set);
        }
        else
        {
            if (percent < 0)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = percent;
            }

            lblProgress.Text = value;
        }
    }

    private void LoadingDialog_Load(object sender, EventArgs e)
    {                        
        m_worker.RunWorkerAsync();
    }

    private void RunWorkerCompletedCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error is not null)
        {
            DialogResult = DialogResult.Cancel;
            Error = e.Error;
        }
        else
        {
            DialogResult = DialogResult.OK;
        }
        Close();
    }

    public Exception Error { get; private set; }
    public object Result { get; private set; }

    public bool CancelEnabled
    {
        get { return btnCancel.Enabled; }
        set { btnCancel.Enabled = value; }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        m_cancellation.Cancel();
    }
}
