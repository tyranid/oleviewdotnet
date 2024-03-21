//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2020
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

using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet.Forms;

internal static class DockPanelHelper
{
    public enum Direction { Left = -1, Right = 1 };
    /// <summary>
    /// Helper method to retrieve the array representation of the currently docked documents.
    /// </summary>
    /// <param name="dockPanel">The panel itself</param>
    /// <returns>DockPanelDocumentInfo about the docks and the selected index</returns>
    public static DockPanelDocumentInfo GetDocumentInfo(this DockPanel dockPanel)
    {
        DockPanelDocumentInfo re = new()
        {
            DockContents = new IDockContent[dockPanel.DocumentsCount],
            SelectedDockContentIndex = -1
        };

        var i = 0;
        foreach (var idc in dockPanel.Documents)
        {
            if (idc == dockPanel.ActiveDocument)
            {
                re.SelectedDockContentIndex = i;
                re.SelectedDockContent = idc;
            }
            re.DockContents[i++] = idc;
        }

        return re;
    }

    /// <summary>
    /// Changes the active document of the panel. If reaches the border, delegates the navigation request to the parent dock, if any.
    /// </summary>
    /// <param name="dockPanel">The panel itself</param>
    /// <param name="direction">Direction of the navigation</param>
    public static void NavigateDocument(this DockPanel dockPanel, Direction direction)
    {
        DockPanelDocumentInfo info = dockPanel.GetDocumentInfo();
        if (info.SelectedDockContent is not null)
        {
            var newIndex = info.SelectedDockContentIndex + (int)direction;
            if (newIndex > -1 && newIndex < info.DockContents.Length)
            {
                // the desired new index belongs to the current dock panel, we can navigate internally.
                info.DockContents[newIndex].DockHandler.Activate();
                return;
            }
        }
    }
}
