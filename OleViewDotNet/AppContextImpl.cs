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
using System.Windows.Forms;

namespace OleViewDotNet
{
    /// <summary>
    /// Implementation of the application context
    /// </summary>
    class AppContextImpl : ApplicationContext
    {
        private COMRegistry m_comRegistry;
        private int m_formCount;

        private void OnFormClosed(object sender, EventArgs e)
        {
            m_formCount--;
            if (m_formCount == 0)
            {                
                ExitThread();
            }
        }

        public void CreateNewMainForm()
        {
            ++m_formCount;
            MainForm frm = new MainForm(m_comRegistry);
            frm.FormClosed += OnFormClosed;
            frm.Show();
        }

        public COMRegistry GetCOMRegistry()
        {
            return m_comRegistry;
        }

        public AppContextImpl(COMRegistry comRegistry)
        {
            m_comRegistry = comRegistry;
            CreateNewMainForm();
        }
    }
}
