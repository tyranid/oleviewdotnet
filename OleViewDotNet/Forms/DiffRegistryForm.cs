//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014. 2016
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

using OleViewDotNet.Database;
using OleViewDotNet.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class DiffRegistryForm : Form
{
    public DiffRegistryForm(COMRegistry current_registry)
    {
        InitializeComponent();
        PopulateComboBox(comboBoxLeft);
        PopulateComboBox(comboBoxRight);
        comboBoxLeft.SelectedItem = current_registry;
    }

    private void PopulateComboBox(ComboBox comboxBox)
    {
        foreach (MainForm form in Application.OpenForms.OfType<MainForm>())
        {
            comboxBox.Items.Add(form.Registry);
        }
    }

    private void OpenRegistry(ComboBox comboBox)
    {
        using OpenFileDialog dlg = new();
        dlg.Filter = "OleViewDotNet DB File (*.ovdb)|*.ovdb|All Files (*.*)|*.*";
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                COMRegistry registry = COMUtilities.LoadRegistry(this, dlg.FileName);
                comboBox.Items.Add(registry);
                comboBox.SelectedItem = registry;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private COMRegistryDiffMode GetDiffMode()
    {
        if (radioLeftOnly.Checked)
        {
            return COMRegistryDiffMode.LeftOnly;
        }

        return COMRegistryDiffMode.RightOnly;
    }

    private void btnAddLeft_Click(object sender, EventArgs e)
    {
        OpenRegistry(comboBoxLeft);
    }

    private void btnAddRight_Click(object sender, EventArgs e)
    {
        OpenRegistry(comboBoxRight);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (comboBoxLeft.SelectedItem is not COMRegistry left || comboBoxRight.SelectedItem is not COMRegistry right)
        {
            MessageBox.Show(this, "Please Select Two Registries", "Select Registries", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            try
            {
                DiffRegistry = COMUtilities.DiffRegistry(this, left, right, GetDiffMode());
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public COMRegistry DiffRegistry
    {
        get; private set;
    }
}
