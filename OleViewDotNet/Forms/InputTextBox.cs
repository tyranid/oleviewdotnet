//    CANAPE Network Testing Tool
//    Copyright (C) 2014 Context Information Security
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal class InputTextBox : TextBox
{
    public event EventHandler<ClipboardEventArgs> TextPasted;

    private const int WM_PASTE = 0x0302;
    protected override void WndProc(ref Message m)
    {
        bool handled = false;

        if (m.Msg == WM_PASTE)
        {
            EventHandler<ClipboardEventArgs> evt = TextPasted;
            if (evt is not null && Clipboard.ContainsText())
            {
                ClipboardEventArgs args = new(Clipboard.GetText());

                evt(this, args);
                handled = args.Handled;
            }
        }

        if (!handled)
        {
            base.WndProc(ref m);
        }
    }
}

public class ClipboardEventArgs : EventArgs
{
    public string ClipboardText { get; set; }
    public bool Handled { get; set; }
    public ClipboardEventArgs(string clipboardText)
    {
        ClipboardText = clipboardText;
    }
}
