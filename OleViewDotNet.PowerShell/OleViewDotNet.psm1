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

function New-CallbackProgress {
    Param(
        [parameter(Mandatory)]
        [string]$Activity
    )

    $callback = [Action[string, string, int]]{ Write-Progress -Activity $args[0] -Status "Processing $($args[1])" -PercentComplete $args[2] }

    [OleViewDotNet.PowerShell.CallbackProgress]::new($Activity, $callback)
}

<#
.SYNOPSIS
Get a COM database from the registry or a file.
.DESCRIPTION
This cmdlet loads a COM registration information database from the current registry or a file and returns an object which can be inspected or passed to other methods.
.PARAMETER LoadMode
Specify what to load from the registry.
.PARAMETER User
Specify a user to load when loading user-specific COM registration information.
.PARAMETER Path
Specify a path to load a saved COM database.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMRegistry
.EXAMPLE
Get-ComDatabase
Load a default, merged COM database.
.EXAMPLE
Get-ComDatabase -LoadMode UserOnly
Load a user-only database for the current user.
.EXAMPLE
Get-ComDatabase -User S-1-5-X-Y-Z
Load a merged COM database including user-only information from the user SID.
#>
function Get-ComDatabase {
    [CmdletBinding(DefaultParameterSetName = "FromRegistry")]
    Param(
        [Parameter(ParameterSetName = "FromRegistry")]
        [OleViewDotNet.COMRegistryMode]$LoadMode = "Merged",
        [Parameter(ParameterSetName = "FromRegistry")]
        [NtApiDotNet.Sid]$User,
        [Parameter(Mandatory, ParameterSetName = "FromFile")]
        [string]$Path
    )
    $callback = New-CallbackProgress -Activity "Loading COM Registry"

    switch($PSCmdlet.ParameterSetName) {
        "FromRegistry" {
            [OleViewDotNet.COMRegistry]::Load($LoadMode, $User, $callback)
        }
        "FromFile" {
            $Path = Resolve-Path $Path
            [OleViewDotNet.COMRegistry]::Load($Path, $callback)
        }
    }
}

<#
.SYNOPSIS
Save a COM database to a file.
.DESCRIPTION
This cmdlet saves a COM registration database to a file.
.PARAMETER Path
The path to save the database to.
.PARAMETER Database
The database to save.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Set-ComRegistry -Database $db -Path output.db
Save a database to the file output.db
#>
function Set-ComDatabase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, Position = 1)]
        [string]$Path
    )
    $callback = New-CallbackProgress -Activity "Saving COM Registry"
    $Path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
    $Database.Save($Path, $callback)
}

<#
.SYNOPSIS
Compares two COM databases and returns the difference.
.DESCRIPTION
The cmdlet compares two COM database, generates the difference and returns a new database with only the differences.
.PARAMETER Left
The database to the left of the comparison.
.PARAMETER Right
The database to the right of the comparison.
.PARAMETER DiffMode
Specify which database information to preserve in the diff, choice between left (default) or right.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMRegistry
.EXAMPLE
Compare-ComRegistry -Left $db1 -Right $db2
Compare two databases, returning the differences in the left database.
.EXAMPLE
Compare-ComRegistry -Left $db1 -Right $db2 -DiffMode RightOnly
Compare two databases, returning the differences in the right database.
#>
function Compare-ComDatabase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Left,
        [Parameter(Mandatory, Position = 1)]
        [OleViewDotNet.COMRegistry]$Right,
        [OleViewDotNet.COMRegistryDiffMode]$DiffMode = "LeftOnly"
    )
    $callback = New-CallbackProgress -Activity "Comparing COM Registries"
    [OleViewDotNet.COMRegistry]::Diff($Left, $Right, $DiffMode, $callback)
}

function Where-HasComServer {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [OleViewDotNet.COMCLSIDEntry]$ClassEntry,
        [string]$ServerName,
        [OleViewDotNet.COMServerType]$ServerType
    )

    PROCESS {
        $write_to_output = $false
        if ($ServerType -eq "UnknownServer") {
            foreach($server in $ClassEntry.Servers.Values) {
                if ($server.Server -match $ServerName) {
                    $write_to_output = $true
                    break
                }
            }
        } else {
            $write_to_output = $ClassEntry.Servers.ContainsKey($ServerType) -and $ClassEntry.Servers[$ServerType].Server -match $ServerName
        }

        if ($write_to_output) {
            Write-Output $ClassEntry
        }
    }
}

<#
.SYNOPSIS
Get COM classes from a database.
.DESCRIPTION
This cmdlet gets COM classes from the database based on a set of criteria. The default is to return all registered classes.
.PARAMETER Database
The database to use.
.PARAMETER Clsid
Specify a CLSID to lookup.
.PARAMETER Name
Specify a name to match against the class name.
.PARAMETER ServerName
Specify a server name to match against.
.PARAMETER ServerType
Specify a type of server to match against. If specified as UnknownServer will search all servers.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMCLSIDEntry
.EXAMPLE
Get-ComClass -Database $db
Get all COM classes from a database.
.EXAMPLE
Get-ComClass -Database $db -Clsid "ffe1df5f-9f06-46d3-af27-f1fc10d63892"
Get a COM class with a specified CLSID.
.EXAMPLE
Get-ComClass -Database $db -Name "TestClass"
Get COM classes which contain TestClass in their name.
.EXAMPLE
Get-ComClass -Database $db -ServerName "obj.ocx"
Get COM classes which are implemented in a server containing the string "obj.ocx"
.EXAMPLE
Get-ComClass -Database $db -ServerType InProcServer32
Get COM classes which are registered with an in-process server.
.EXAMPLE
Get-ComClass -Database $db -Iid "00000001-0000-0000-C000-000000000046"
Get COM class registered as an interface proxy.

#>
function Get-ComClass {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(ParameterSetName = "FromServer")]
        [string]$ServerName = "",
        [Parameter(ParameterSetName = "FromServer")]
        [OleViewDotNet.COMServerType]$ServerType = "UnknownServer",
        [Parameter(Mandatory, ParameterSetName = "FromIid")]
        [Guid]$Iid
    )
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Clsids.Values
        }
        "FromClsid" {
            if ($Database.Clsids.ContainsKey($Clsid)) {
                Write-Output $Database.Clsids[$Clsid]
            }
        }
        "FromName" {
            Get-ComClass $Database | ? Name -Match $Name | Write-Output
        }
        "FromServer" {
            Get-ComClass $Database | Where-HasComServer -ServerName $ServerName -ServerType $ServerType | Write-Output
        }
        "FromIid" {
            Write-Output $Database.MapIidToInterface($Iid).ProxyClassEntry
        }
    }
}