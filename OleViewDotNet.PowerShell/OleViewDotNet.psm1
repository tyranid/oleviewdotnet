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

[OleViewDotNet.COMUtilities]::SetupCachedSymbols()

function New-CallbackProgress {
    Param(
        [parameter(Mandatory)]
        [string]$Activity,
        [switch]$NoProgress
    )

    if ($NoProgress) {
        $callback = {}
    } else {
        $callback = { Write-Progress -Activity $args[0] -Status "Processing $($args[1])" -PercentComplete $args[2] }
    }

    [OleViewDotNet.PowerShell.CallbackProgress]::new($Activity, [Action[string, string, int]]$callback)
}

function Resolve-LocalPath {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [string]$Path
    )
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
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
.PARAMETER NoProgress
Don't show progress for load.
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
        [Parameter(Mandatory, ParameterSetName = "FromFile", Position = 0)]
        [string]$Path,
        [switch]$NoProgress
    )
    $callback = New-CallbackProgress -Activity "Loading COM Registry" -NoProgress:$NoProgress

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
.PARAMETER NoProgress
Don't show progress for save.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Set-ComRegistry -Database $comdb -Path output.db
Save a database to the file output.db
#>
function Set-ComDatabase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, Position = 1)]
        [string]$Path,
        [switch]$NoProgress
    )
    $callback = New-CallbackProgress -Activity "Saving COM Registry" -NoProgress:$NoProgress
    $Path = Resolve-LocalPath $Path
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
.PARAMETER NoProgress
Don't show progress for compare.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMRegistry
.EXAMPLE
Compare-ComRegistry -Left $comdb1 -Right $comdb2
Compare two databases, returning the differences in the left database.
.EXAMPLE
Compare-ComRegistry -Left $comdb1 -Right $comdb2 -DiffMode RightOnly
Compare two databases, returning the differences in the right database.
#>
function Compare-ComDatabase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Left,
        [Parameter(Mandatory, Position = 1)]
        [OleViewDotNet.COMRegistry]$Right,
        [OleViewDotNet.COMRegistryDiffMode]$DiffMode = "LeftOnly",
        [switch]$NoProgresss
    )
    $callback = New-CallbackProgress -Activity "Comparing COM Registries" -NoProgress:$NoProgress
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
.PARAMETER InteractiveUser
Specify that the COM classes should be configured to run as the Interactive User.
.PARAMETER ProgId
Specify looking up the COM class from a ProgID.
.PARAMETER Iid
Specify looking up a COM class based on it's proxy IID.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMCLSIDEntry
.EXAMPLE
Get-ComClass -Database $comdb
Get all COM classes from a database.
.EXAMPLE
Get-ComClass -Database $comdb -Clsid "ffe1df5f-9f06-46d3-af27-f1fc10d63892"
Get a COM class with a specified CLSID.
.EXAMPLE
Get-ComClass -Database $comdb -Name "TestClass"
Get COM classes which contain TestClass in their name.
.EXAMPLE
Get-ComClass -Database $comdb -ServerName "obj.ocx"
Get COM classes which are implemented in a server containing the string "obj.ocx"
.EXAMPLE
Get-ComClass -Database $comdb -ServerType InProcServer32
Get COM classes which are registered with an in-process server.
.EXAMPLE
Get-ComClass -Database $comdb -Iid "00000001-0000-0000-C000-000000000046"
Get COM class registered as an interface proxy.
.EXAMPLE
Get-ComClass -Database $comdb -ProgId htafile
Get COM class from a Prog ID.
.EXAMPLE
Get-ComClass -Database $comdb -InteractiveUser
Get COM classes registered to run as the interactive user.
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
        [Guid]$Iid,
        [Parameter(Mandatory, ParameterSetName = "FromProgId")]
        [string]$ProgId,
        [Parameter(Mandatory, ParameterSetName = "FromIU")]
        [switch]$InteractiveUser
    )
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Clsids.Values
        }
        "FromClsid" {
            Write-Output $Database.Clsids[$Clsid]
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
        "FromProgId" {
            Write-Output $Database.MapProgIdToClsid($ProgId)
        }
        "FromIU" {
            Get-ComClass $Database | ? { $_.HasAppID -and $_.AppIDEntry.RunAs -eq  "Interactive User" } | Write-Output
        }
    }
}

<#
.SYNOPSIS
Get COM process information.
.DESCRIPTION
This cmdlet opens a specified set of processes and extracts the COM information from them. For this to work you need symbol support.
.PARAMETER Database
The database to use to lookup information.
.PARAMETER Process
Specify a list of process objects to parse. You can get these from Get-Process cmdlet.
.PARAMETER DbgHelpPath
Specify location of DBGHELP.DLL file. For remote symbol support use one from Debugging Tools for Windows.
.PARAMETER SymbolPath
Specify the location of symbols for the resolver.
.PARAMETER ParseStubMethods
Specify to parse the method parameter information on a process stub.
.PARAMETER ResolveMethodNames
Specify to try and resolve method names for interfaces.
.PARAMETER ParseRegisteredClasses
Specify to parse classes registered by the process.
.PARAMETER NoProgress
Don't show progress for process parsing.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMProcessEntry
.EXAMPLE
Get-ComProcess -Database $comdb
Get all COM processes.
.EXAMPLE
Get-Process notepad | Get-ComProcess -Database $comdb
Get COM process from a list of processes.
#>
function Get-ComProcess {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [string]$comdbgHelpPath = "dbghelp.dll",
        [string]$SymbolPath = "srv*https://msdl.microsoft.com/download/symbols",
        [switch]$ParseStubMethods,
        [switch]$ResolveMethodNames,
        [switch]$ParseRegisteredClasses,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromProcess")]
        [System.Diagnostics.Process[]]$Process,
        [switch]$NoProgress
    )

    BEGIN {
        if ($comdbgHelpPath -eq "") {
            $comdbgHelpPath = "dbghelp.dll"
        }
        if ($SymbolPath -eq "") {
            $SymbolPath = $env:_NT_SYMBOL_PATH
            if ($SymbolPath -eq "") {
                $SymbolPath = 'srv*https://msdl.microsoft.com/download/symbols'
            }
        }
        $procs = @()
    }

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "All" {
                $procs = Get-Process
            }
            "FromProcess" {
                $procs += $Process
            }
        }
    }

    END {
        $callback = New-CallbackProgress -Activity "Parsing COM Processes" -NoProgress:$NoProgress
        $config = [OleViewDotNet.COMProcessParserConfig]::new($comdbgHelpPath, $SymbolPath, `
                    $ParseStubMethods, $ResolveMethodNames, $ParseRegisteredClasses)
        [OleViewDotNet.COMProcessParser]::GetProcesses([System.Diagnostics.Process[]]$procs, $config, $callback, $Database) | Write-Output
    }
}

<#
.SYNOPSIS
Start a log of COM activations in the current process.
.DESCRIPTION
This cmdlet starts a COM activation log for the current process. It will write out all 
COM classes created until Stop-ComActivationLog is called.
.PARAMETER Database
Optional database to lookup names for activated objects.
.PARAMETER Path
Specify a path for the log file.
.PARAMETER Append
If specified then new entries will be appended to the log rather than replacing the log file.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Start-ComActivationLog activations.log
Start COM activation log to activations.log.
.EXAMPLE
Start-ComActivationLog activations.log -Database $comdb
Start COM activation log to activations.log with a database for name lookup.
.EXAMPLE
Start-ComActivationLog activations.log -Append
Start COM activation log to activations.log appending new entries to the end of the file.
#>
function Start-ComActivationLog {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [string]$Path,
        [switch]$Append,
        [OleViewDotNet.COMRegistry]$Database
    )

    $Path = Resolve-LocalPath $Path
    [OleViewDotNet.PowerShell.LoggingActivationFilter]::Instance.Start($Path, $Append, $Database)
}

<#
.SYNOPSIS
Stop the log of COM activations in the current process.
.DESCRIPTION
This cmdlet stops a COM activation log for the current process.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Stop-ComActivationLog
Stop COM activation log.
#>
function Stop-ComActivationLog {
    [OleViewDotNet.PowerShell.LoggingActivationFilter]::Instance.Stop()
}

<#
.SYNOPSIS
Get COM AppIDs from a database.
.DESCRIPTION
This cmdlet gets COM AppIDs from the database based on a set of criteria. The default is to return all registered AppIds.
.PARAMETER Database
The database to use.
.PARAMETER AppId
Specify a AppID to lookup.
.PARAMETER Name
Specify a name to match against the AppId name.
.PARAMETER ServiceName
Specify a service name to match against.
.PARAMETER IsService
Specify a returns AppIDs implemented by services.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMAppIDEntry
.EXAMPLE
Get-ComAppId -Database $comdb
Get all COM AppIDs from a database.
#>
function Get-ComAppId {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, ParameterSetName = "FromAppId")]
        [Guid]$AppId,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(ParameterSetName = "FromServiceName")]
        [string]$ServiceName = "",
        [Parameter(ParameterSetName = "FromIsService")]
        [switch]$IsService
    )
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.AppIDs.Values
        }
        "FromAppId" {
            Write-Output $Database.AppIDs[$AppId]
        }
        "FromName" {
            Get-ComAppId $Database | ? Name -Match $Name | Write-Output
        }
        "FromServiceName" {
            Get-ComAppId $Database | ? ServiceName -Match $ServiceName | Write-Output
        }
        "FromIsService" {
            Get-ComAppId $Database | ? IsService | Write-Output
        }
    }
}

<#
.SYNOPSIS
Show a COM database in the main viewer.
.DESCRIPTION
This cmdlet starts the main viewer application and loads a specified database file.
.PARAMETER Database
The database to view.
.PARAMETER Path
The path to the database to view.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Show-ComDatabase -Database $comdb
Show a COM database in the viewer.
.EXAMPLE
Show-ComDatabase -Path com.db
Show a COM database in the viewer from a file.
#>
function Show-ComDatabase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromDb")]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromFile")]
        [string]$Path
    )

    $DeleteFile = $false

    switch($PSCmdlet.ParameterSetName) {
        "FromDb" {
            $Path = (New-TemporaryFile).FullName
            Set-ComDatabase $Database $Path -NoProgress
            $DeleteFile = $true
        }
        "FromFile" {
            # Do nothing.
        }
    }
    $exe = [OleViewDotNet.COMUtilities]::GetExePathForCurrentBitness()
    $args = @("`"-i=$Path`"")
    if ($DeleteFile) {
        $args += @("-d")
    }
    Start-Process $exe $args
}

<#
.SYNOPSIS
Get a COM class or Runtime class instance interfaces.
.DESCRIPTION
This cmdlet enumerates the supported interfaces for a COM class or Runtime class and returns them.
.PARAMETER ClassEntry
The COM or Runtime class to enumerate.
.PARAMETER Refresh
Specify to force the interfaces to be refreshed.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMInterfaceInstance[]
.EXAMPLE
Get-ComClassInterface -ClassEntry $cls
Get instance interfaces for a COM class.
.EXAMPLE
Get-ComClassInterface -ClassEntry $cls -Refresh
Get instance interfaces for a COM class forcing them to be refreshed if necessary.
#>
function Get-ComClassInterface {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [OleViewDotNet.ICOMClassEntry]$ClassEntry,
        [switch]$Refresh
        )
    PROCESS {
        $ClassEntry.LoadSupportedInterfaces($Refresh) | Out-Null
        $ClassEntry.Interfaces | Write-Output
    }
}

<#
.SYNOPSIS
Get COM Runtime classes from a database.
.DESCRIPTION
This cmdlet gets COM Runtime classes from the database based on a set of criteria. The default is to return all registered runtime classes.
.PARAMETER Database
The database to use.
.PARAMETER Name
Specify a name to match against the class name.
.PARAMETER DllPath
Specify the DLL path to match against.
.PARAMETER ActivationType
Specify a type of activation to match against.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMRuntimeClassEntry
.EXAMPLE
Get-ComRuntimeClass -Database $comdb
Get all COM Runtime classes from a database.
.EXAMPLE
Get-ComRuntimeClass -Database $comdb -Name "TestClass"
Get COM Runtime classes which contain TestClass in their name.
.EXAMPLE
Get-ComRuntimeClass -Database $comdb -DllPath "runtime.dll"
Get COM Runtime classes which are implemented in a DLL containing the string "runtime.dll"
.EXAMPLE
Get-ComRuntimeClass -Database $comdb -ActivationType OutOfProcess
Get COM Runtime classes which are implemented out-of-process.
#>
function Get-ComRuntimeClass {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromDllPath")]
        [string]$DllPath,
        [Parameter(Mandatory, ParameterSetName = "FromActivationType")]
        [OleViewDotNet.ActivationType]$ActivationType 
    )
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.RuntimeClasses.Values
        }
        "FromName" {
            Get-ComRuntimeClass $Database | ? Name -Match $Name | Write-Output
        }
        "FromDllPath" {
            Get-ComRuntimeClass $Database | ? DllPath -Match $DllPath | Write-Output
        }
        "FromActivationType" {
            Get-ComRuntimeClass $Database | ? ActivationType -eq $ActivationType | Write-Output
        }
    }
}

<#
.SYNOPSIS
Get COM interfaces from a database.
.DESCRIPTION
This cmdlet gets COM interfaces from the database based on a set of criteria. The default is to return all registered interfaces.
.PARAMETER Database
The database to use.
.PARAMETER Iid
Specify a IID to lookup.
.PARAMETER Name
Specify a name to match against the interface name.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMInterfaceEntry
.EXAMPLE
Get-ComInterface -Database $comdb
Get all COM interfaces from a database.
.EXAMPLE
Get-ComInterface -Database $comdb -Iid "00000001-0000-0000-C000-000000000046"
Get COM interface from an IID from a database.
.EXAMPLE
Get-ComInterface -Database $comdb -Name "IBlah"
Get COM interfaces which contain IBlah in their name.
#>
function Get-ComInterface {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name
    )
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Interfaces.Values
        }
        "FromName" {
            Get-ComInterface $Database | ? Name -Match $Name | Write-Output
        }
        "FromIid" {
            $Database.Interfaces[$Iid] | Write-Output
        }
    }
}

<#
.SYNOPSIS
Filter launch accessible COM database information.
.DESCRIPTION
This cmdlet filters various types of COM database information such as Classes, AppIDs and processes 
to only those launchable accessible by certain processes or tokens.
.PARAMETER InputObject
The COM object entry to select on.
.PARAMETER Token
An access token to perform the access check on.
.PARAMETER Process
A process to get the access token from for the access check.
.PARAMETER ProcessId
A process ID to get the access token from for the access check.
.PARAMETER Access
The access mask to check, for access permissions. Defaults to local execute.
.PARAMETER Access
The access mask to check, for launch permissions. Defaults to local execute and activation.
.PARAMETER Principal
The principal for the access check, defaults to the current user.
.PARAMETER NotAccessible
Filter out accessible objects.
.PARAMETER IgnoreDefault
If the object doesn't have a specific set of launch permissions uses the system default. If this flag is specified objects without a specific launch permission are ignored.
.INPUTS
OleViewDotNet.ICOMAccessSecurity
.OUTPUTS
OleViewDotNet.ICOMAccessSecurity
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess
Get all COM classes which are accessible by the current process.
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess -IgnoreDefault
Get all COM classes which are accessible by the current process ignoring default security permissions.
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess -Token $token
Get all COM classes which are accessible by a specified token.
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess -Process $process
Get all COM classes which are accessible by a specified process.
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess -ProcessId 1234
Get all COM classes which are accessible by a specified process from its ID.
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess -Access 0
Only check for launch permissions and ignore access permissions.
.EXAMPLE
Get-ComClass $comdb | Select-ComAccess -LaunchAccess 0
Only check for access permissions and ignore launch permissions.
#>
function Select-ComAccess {
    [CmdletBinding(DefaultParameterSetName = "FromProcessId")]
    Param(
        [Parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [OleViewDotNet.ICOMAccessSecurity]$InputObject,
        [OleViewDotNet.COMAccessRights]$Access = "ExecuteLocal",
        [OleViewDotNet.COMAccessRights]$LaunchAccess = "ActivateLocal, ExecuteLocal",
        [Parameter(Mandatory, ParameterSetName = "FromToken")]
        [NtApiDotNet.NtToken]$Token,
        [Parameter(Mandatory, ParameterSetName = "FromProcess")]
        [NtApiDotNet.NtProcess]$Process,
        [Parameter(ParameterSetName = "FromProcessId")]
        [int]$ProcessId = $pid,
        [NtApiDotNet.Sid]$Principal = [NtApiDotNet.NtProcess]::Current.User,
        [switch]$NotAccessible,
        [switch]$IgnoreDefault
    )

    BEGIN {
        switch($PSCmdlet.ParameterSetName) {
            "FromProcessId" {
                $access_check = [OleViewDotNet.PowerShell.PowerShellUtils]::GetAccessCheck($ProcessId, `
                    $Principal, $Access, $LaunchAccess, $IgnoreDefault)
            }
            "FromProcess" {
                $access_check = [OleViewDotNet.PowerShell.PowerShellUtils]::GetAccessCheck($Process, `
                    $Principal, $Access, $LaunchAccess, $IgnoreDefault)
            }
            "FromToken" {
                $access_check = [OleViewDotNet.PowerShell.PowerShellUtils]::GetAccessCheck($Token, `
                    $Principal, $Access, $LaunchAccess, $IgnoreDefault)
            }
        }
    }

    PROCESS {
        $result = $access_check.AccessCheck($InputObject)
        if ($NotAccessible) {
            $result = !$result
        }
        if ($result) {
            Write-Output $InputObject
        }
    }

    END {
        if ($null -ne $access_check) {
            $access_check.Dispose()
        }
    }
}

Enum ComObjRefOutput
{
    Object
    Bytes
    Moniker
}

<#
.SYNOPSIS
Get an OBJREF for a COM object.
.DESCRIPTION
This cmdlet marshals a COM object to an OBJREF, returning a byte array, a COMObjRef object or a moniker.
.PARAMETER InputObject
The object to marshal.
.PARAMETER Path
Specify a path for the output OBJREF.
.PARAMETER Output
Specify the output mode for the OBJREF.
.PARAMETER IID
Specify the IID to marshal.
.PARAMETER MarshalContext
Specify the context to marshal for.
.PARAMETER MarshalFlags
Specify flags for the marshal operation.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMObjRef or string.
.EXAMPLE
Get-ComObjRef $obj 
Marshal an object to the file marshal.bin as a COMObjRef object.
.EXAMPLE
Get-ComObjRef $obj -Output Bytes | Set-Content objref.bin -Encoding Bytes
Marshal an object to a byte array and write to a file.
.EXAMPLE
Get-ComObjRef $obj -Output Moniker
Marshal an object to a moniker.
.EXAMPLE
Get-ComObjRef objref.bin
Gets an OBJREF from a file.
#>
function Get-ComObjRef {
    [CmdletBinding(DefaultParameterSetName = "FromPath")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromObject")]
        [object]$InputObject,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromPath")]
        [string]$Path,
        [ComObjRefOutput]$Output = "Object",
        [Parameter(ParameterSetName = "FromObject")]
        [Guid]$Iid = "00000000-0000-0000-C000-000000000046",
        [Parameter(ParameterSetName = "FromObject")]
        [OleViewDotNet.MSHCTX]$MarshalContext = "DIFFERENTMACHINE",
        [Parameter(ParameterSetName = "FromObject")]
        [OleViewdotNet.MSHLFLAGS]$MarshalFlags = "NORMAL"
    )

    switch($PSCmdlet.ParameterSetName) {
        "FromObject" {
            $objref = [OleViewDotNet.COMUtilities]::MarshalObjectToObjRef($InputObject, $Iid, $MarshalContext, $MarshalFlags)
        }
        "FromPath" {
            $ba = Get-Content -Path $Path -Encoding Byte
            $objref = [OleViewDotNet.COMObjRef]::FromArray($ba)
        }
    }

    switch($Output) {
        "Bytes" {
            Write-Output $objref.ToArray()
        }
        "Moniker" {
            $moniker = $objref.ToMoniker()
            Write-Output $moniker
        }
        "Object" {
            Write-Output $objref
        }
    }
}

<#
.SYNOPSIS
Views a COM security descriptor.
.DESCRIPTION
This cmdlet opens a viewer for a COM security descriptor.
.PARAMETER SecurityDescriptor
The security descriptor to view in SDDL format.
.PARAMETER ShowAccess
Show access rights rather than launch rights.
.PARAMETER InputObject
Shows the security descriptor for a database object.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Show-ComSecurityDescriptor $obj
Shows a launch security descriptor from an object.
.EXAMPLE
Show-ComSecurityDescriptor $obj -ShowAccess
Shows an access security descriptor from an object.
.EXAMPLE
Show-ComSecurityDescriptor "D:(A;;GA;;;WD)" 
Shows a SDDL launch security descriptor.
.EXAMPLE
Show-ComSecurityDescriptor "D:(A;;GA;;;WD)" -ShowAccess
Shows a SDDL access security descriptor.
#>
function Show-ComSecurityDescriptor {
    [CmdletBinding(DefaultParameterSetName="FromObject")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromSddl")]
        [string]$SecurityDescriptor,
        [Parameter(Mandatory, Position = 0, ValueFromPipeline, ParameterSetName = "FromObject")]
        [OleViewDotNet.ICOMAccessSecurity]$InputObject,
        [switch]$ShowAccess
    )

    PROCESS {
        $name = ""
        switch($PSCmdlet.ParameterSetName) {
            "FromSddl" {
                # Do nothing.
            }
            "FromObject" {
                if ($ShowAccess) {
                    $SecurityDescriptor = [OleViewDotNet.COMAccessCheck]::GetAccessPermission($InputObject)
                } else {
                    $SecurityDescriptor = [OleViewDotNet.COMAccessCheck]::GetLaunchPermission($InputObject)
                }
                $name = $InputObject.Name.Replace("`"", " ")
            }
        }

        if ("" -ne $SecurityDescriptor) {
            $exe = [OleViewDotNet.COMUtilities]::GetExePathForCurrentBitness()
            if ($ShowAccess) {
                $cmd = "-v"
            } else {
                $cmd = "-l"
            }
            $args = @("`"$cmd=$SecurityDescriptor`"")
            if ("" -ne $name) {
                $args += @("`"$name`"")
            }
            Start-Process $exe $args
        }
    }
}

function Wrap-Object {
    [CmdletBinding(DefaultParameterSetName = "FromType")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [object]$Object,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromType")]
        [Type]$Type
    )

    switch($PSCmdlet.ParameterSetName) {
        "FromIid" {
            [OleViewDotNet.ComWrapperFactory]::Wrap($obj, $Iid)
        }
        "FromType" {
            [OleViewDotNet.ComWrapperFactory]::Wrap($obj, $Type)
        }
    }
}

<#
.SYNOPSIS
Creates a new COM object instance.
.DESCRIPTION
This cmdlet creates a new COM object instance from a class or factory.
.PARAMETER Class
Specify the class to use for the new COM object.
.PARAMETER Factory
Specify an existing class factory for the new COM object.
.PARAMETER Clsid
Specify a CLSID to use for the new COM object.
.PARAMETER ClassContext
Specify the context the new object will be created from.
.PARAMETER RemoteServer
Specify the remote server the COM object will be created on.
#>
function New-ComObject {
    [CmdletBinding(DefaultParameterSetName="FromClass")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClass")]
        [OleViewDotNet.ICOMClassEntry]$Class,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromFactory")]
        [OleViewDotNet.IClassFactory]$Factory,
        [Parameter(Mandatory, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [Parameter(ParameterSetName = "FromClsid")]
        [Parameter(ParameterSetName = "FromClass")]
        [OleViewDotNet.CLSCTX]$ClassContext = "ALL",
        [Parameter(ParameterSetName = "FromClsid")]
        [Parameter(ParameterSetName = "FromClass")]
        [string]$RemoteServer
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                $obj = $Class.CreateInstanceAsObject($ClassContext, $RemoteServer)
            }
            "FromClsid" {
                $obj = [OleViewDotNet.COMUtilities]::CreateInstanceAsObject($Clsid, "00000000-0000-0000-C000-000000000046", $ClassContext, $RemoteServer)
            }
            "FromFactory" {
                $obj = [OleViewDotNet.COMUtilities]::CreateInstanceFromFactory($Factory, "00000000-0000-0000-C000-000000000046")
            }
        }
        $type = [OleViewDotNet.IUnknown]
        Wrap-Object $obj -Type $type | Write-Output
    }
}

<#
.SYNOPSIS
Creates a new COM object factory.
.DESCRIPTION
This cmdlet creates a new COM object factory from a class.
.PARAMETER Class
Specify the class to use for the new COM object factory.
.PARAMETER Clsid
Specify a CLSID to use for the new COM object factory.
.PARAMETER ClassContext
Specify the context the new factory will be created from.
.PARAMETER RemoteServer
Specify the remote server the COM object factory will be created on.
#>
function New-ComObjectFactory {
    [CmdletBinding(DefaultParameterSetName="FromClass")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClass")]
        [OleViewDotNet.ICOMClassEntry]$Class,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [OleViewDotNet.CLSCTX]$ClassContext = "ALL",
        [string]$RemoteServer
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                $obj = $Class.CreateClassFactory($ClassContext, $RemoteServer)
            }
            "FromClsid" {
                $obj = [OleViewDotNet.COMUtilities]::CreateClassFactory($Clsid, "00000000-0000-0000-C000-000000000046", $ClassContext, $RemoteServer)
            }
        }
        $type = [OleViewDotNet.IClassFactory]
        Wrap-Object $obj $type | Write-Output
    }
}