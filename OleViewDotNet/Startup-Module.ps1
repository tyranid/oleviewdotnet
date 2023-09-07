# This file is part of OleViewDotNet.
# Copyright (C) James Forshaw 2018
#
# OleViewDotNet is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# OleViewDotNet is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

# This script is used to bootstrap the PowerShell module from the viewer.

Import-Module "$PSScriptRoot\OleViewDotNet.psd1"

if ($args.Count -gt 0) {
    Get-ComDatabase $args[0] -NoProgress
    if ($args.Count -gt 1) {
        Remove-Item $args[0]
    }
    Write-Host -ForegroundColor Yellow "Loaded Database and set as current."
}