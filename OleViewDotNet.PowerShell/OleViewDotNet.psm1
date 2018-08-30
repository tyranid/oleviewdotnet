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

Set-StrictMode -Version Latest

$Script:GlobalDbgHelpPath = "dbghelp.dll"
$Script:GlobalSymbolPath = "srv*https://msdl.microsoft.com/download/symbols"
$Script:CurrentComDatabase = $null

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

function Wrap-ComObject {
    [CmdletBinding(DefaultParameterSetName = "FromType")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [object]$Object,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromType")]
        [Type]$Type,
        [switch]$NoWrapper
    )

    if ($NoWrapper) {
        return $Object
    }

    switch($PSCmdlet.ParameterSetName) {
        "FromIid" {
            [OleViewDotNet.ComWrapperFactory]::Wrap($Object, $Iid)
        }
        "FromType" {
            [OleViewDotNet.ComWrapperFactory]::Wrap($Object, $Type)
        }
    }
}

function Unwrap-ComObject {
    [CmdletBinding(DefaultParameterSetName = "FromType")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [object]$Object
    )

    [OleViewDotNet.ComWrapperFactory]::Unwrap($Object)
}

function Get-ComSymbolResolver {
    Param (
        [parameter( Position=0)]
        [string]$DbgHelpPath = "",
        [parameter(Position=1)]
        [string]$SymbolPath = "srv*https://msdl.microsoft.com/download/symbols"
    )
    if ($DbgHelpPath -eq "") {
        $DbgHelpPath = $Script:GlobalDbgHelpPath
    }
    if ($SymbolPath -eq "") {
        $SymbolPath = $env:_NT_SYMBOL_PATH
        if ($SymbolPath -eq "") {
            $SymbolPath = $Script:GlobalSymbolPath
        }
    }
    @{DbgHelpPath=$DbgHelpPath; SymbolPath=$SymbolPath}
}

<#
.SYNOPSIS
Gets the current COM database.
.DESCRIPTION
This cmdlet gets the current COM database.
.PARAMETER Database
A database parameter to test. This function returns $Database if it's not $null, otherwise returns the current database.
#>
function Get-CurrentComDatabase {
    Param(
        [parameter(Position=0)]
        [OleViewDotNet.COMRegistry]$Database
    )

    if ($null -ne $Database) {
        $Database
    } else {
        $Script:CurrentComDatabase
    }
}

<#
.SYNOPSIS
Sets the current COM database.
.DESCRIPTION
This cmdlet sets the current COM database. It allows you to load a COM database and not need to pass it as a parameter.
.PARAMETER Database
The database to set as the current database. You can specify $null to remove the current.
#>
function Set-CurrentComDatabase {
    Param(
        [parameter(Mandatory, Position=0)]
        [AllowNull()]
        [OleViewDotNet.COMRegistry]$Database
    )
    $Script:CurrentComDatabase = $Database
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
.PARAMETER SetCurrent
Specify after loading that the database is set as the current database. When setting the current database the object isn't returned. To access it directly
call Get-CurrentComDatabase.
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
.EXAMPLE
Get-ComDatabase -SetCurrent
Load a default, merged COM database then sets it as the current database.
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
        [switch]$NoProgress,
        [switch]$SetCurrent
    )
    $callback = New-CallbackProgress -Activity "Loading COM Registry" -NoProgress:$NoProgress
    $comdb = switch($PSCmdlet.ParameterSetName) {
        "FromRegistry" {
            [OleViewDotNet.COMRegistry]::Load($LoadMode, $User, $callback)
        }
        "FromFile" {
            $Path = Resolve-Path $Path
            [OleViewDotNet.COMRegistry]::Load($Path, $callback)
        }
    }
    if ($SetCurrent) {
        Set-CurrentComDatabase $comdb
    } else {
        Write-Output $comdb
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
Set-ComDatabase -Path output.db
Save the current database to the file output.db
.EXAMPLE
Set-ComDatabase -Path output.db -Database $comdb 
Save a specific database to the file output.db
#>
function Set-ComDatabase {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory,Position=0)]
        [string]$Path,
        [OleViewDotNet.COMRegistry]$Database,
        [switch]$NoProgress
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }

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
Specify a name which equals the class name.
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
.PARAMETER Object
Specify looking up the COM class based on an object instance. Needs to support an IPersist inteface to extract the CLSID.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMCLSIDEntry
.EXAMPLE
Get-ComClass 
Get all COM classes from the current databae.
.EXAMPLE
Get-ComClass -Database $comdb
Get all COM classes from a database.
.EXAMPLE
Get-ComClass -Clsid "ffe1df5f-9f06-46d3-af27-f1fc10d63892"
Get a COM class with a specified CLSID.
.EXAMPLE
Get-ComClass -Name "TestClass"
Get COM classes which where the name is TestClass.
.EXAMPLE
Get-ComClass -ServerName "obj.ocx"
Get COM classes which are implemented in a server containing the string "obj.ocx"
.EXAMPLE
Get-ComClass -ServerType InProcServer32
Get COM classes which are registered with an in-process server.
.EXAMPLE
Get-ComClass -Iid "00000001-0000-0000-C000-000000000046"
Get COM class registered as an interface proxy.
.EXAMPLE
Get-ComClass -ProgId htafile
Get COM class from a Prog ID.
.EXAMPLE
Get-ComClass -InteractiveUser
Get COM classes registered to run as the interactive user.
.EXAMPLE
Get-ComClass -Service
Get COM classes registered to run inside a service.
.EXAMPLE
Get-ComClass -ServiceName "ExampleService"
Get COM classes registered to run inside a service with a specific name.
.EXAMPLE
Get-ComClass -Object $obj
Get COM class based on an object instance. Needs 
#>
function Get-ComClass {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
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
        [switch]$InteractiveUser,
        [Parameter(Mandatory, ParameterSetName = "FromService")]
        [switch]$Service,
        [Parameter(Mandatory, ParameterSetName = "FromServiceName")]
        [string]$ServiceName,
        [Parameter(Mandatory, ParameterSetName = "FromObject")]
        [object]$Object
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }

    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Clsids.Values
        }
        "FromClsid" {
            Write-Output $Database.Clsids[$Clsid]
        }
        "FromName" {
            Get-ComClass -Database $Database | ? Name -eq $Name | Write-Output
        }
        "FromServer" {
            Get-ComClass -Database $Database | Where-HasComServer -ServerName $ServerName -ServerType $ServerType | Write-Output
        }
        "FromIid" {
            Write-Output $Database.MapIidToInterface($Iid).ProxyClassEntry
        }
        "FromProgId" {
            Write-Output $Database.MapProgIdToClsid($ProgId)
        }
        "FromIU" {
            Get-ComClass -Database $Database | ? { $_.HasAppID -and $_.AppIDEntry.RunAs -eq  "Interactive User" } | Write-Output
        }
        "FromService" {
            Get-ComClass -Database $Database | ? { $_.HasAppID -and $_.AppIDEntry.IsService } | Write-Output
        }
        "FromServiceName" {
            Get-ComClass -Database $Database -Service | ? { $_.AppIDEntry.ServiceName -eq $ServiceName } | Write-Output
        }
        "FromObject" {
            $Object = Unwrap-ComObject $Object
            $Clsid = [OleViewDotNet.COMUtilities]::GetObjectClass($Object)
            if ($Clsid -ne [Guid]::Empty) {
                Get-ComClass -Database $Database -Clsid $Clsid | Write-Output
            }
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
.PARAMETER ParseClients
Specify to parse client proxy information from the process.
.PARAMETER NoProgress
Don't show progress for process parsing.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMProcessEntry
.EXAMPLE
Get-ComProcess
Get all COM processes using the current database.
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
        [OleViewDotNet.COMRegistry]$Database,
        [alias("dbghelp")]
        [string]$DbgHelpPath = "",
        [string]$SymbolPath = "",
        [switch]$ParseStubMethods,
        [switch]$ResolveMethodNames,
        [switch]$ParseRegisteredClasses,
        [switch]$ParseClients,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromProcessId")]
        [alias("pid")]
        [int[]]$ProcessId,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromProcess")]
        [System.Diagnostics.Process[]]$Process,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromObjRef")]
        [OleViewDotNet.COMObjRef[]]$ObjRef,
        [parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [switch]$NoProgress
    )

    BEGIN {
        $resolver = Get-ComSymbolResolver $DbgHelpPath $SymbolPath
        $procs = @()
        $objrefs = @()
    }

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "All" {
                $procs = Get-Process
            }
            "FromProcessId" {
                $procs = Get-Process -Id $ProcessId
            }
            "FromProcess" {
                $procs += $Process
            }
            "FromObjRef" {
                $objrefs += $ObjRef
            }
            "FromName" {
                $procs = Get-Process -Name $Name
            }
        }
    }

    END {
        $Database = Get-CurrentComDatabase $Database
        if ($null -eq $Database) {
            Write-Error "No database specified and current database isn't set"
            return
        }
        $callback = New-CallbackProgress -Activity "Parsing COM Processes" -NoProgress:$NoProgress
        $config = [OleViewDotNet.COMProcessParserConfig]::new($resolver.DbgHelpPath, $resolver.SymbolPath, `
                    $ParseStubMethods, $ResolveMethodNames, $ParseRegisteredClasses, $ParseClients)

        if ($PSCmdlet.ParameterSetName -eq "FromObjRef") {
            [OleViewDotNet.COMProcessParser]::GetProcesses([OleViewDotNet.COMObjRef[]]$objrefs, $config, $callback, $Database) | Write-Output
        } else {
            [OleViewDotNet.COMProcessParser]::GetProcesses([System.Diagnostics.Process[]]$procs, $config, $callback, $Database) | Write-Output
        }
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

    $Database = Get-CurrentComDatabase $Database
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
    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.AppIDs.Values
        }
        "FromAppId" {
            Write-Output $Database.AppIDs[$AppId]
        }
        "FromName" {
            Get-ComAppId -Database $Database | ? Name -eq $Name | Write-Output
        }
        "FromServiceName" {
            Get-ComAppId -Database $Database | ? ServiceName -eq $ServiceName | Write-Output
        }
        "FromIsService" {
            Get-ComAppId -Database $Database | ? IsService | Write-Output
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
Show-ComDatabase
Show the current COM database in the viewer.
.EXAMPLE
Show-ComDatabase -Database $comdb
Show a COM database in the viewer.
.EXAMPLE
Show-ComDatabase -Path com.db
Show a COM database in the viewer from a file.
#>
function Show-ComDatabase {
    [CmdletBinding(DefaultParameterSetName="FromDb")]
    Param(
        [Parameter(Position = 0, ParameterSetName = "FromDb")]
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromFile")]
        [string]$Path
    )

    $DeleteFile = $false

    switch($PSCmdlet.ParameterSetName) {
        "FromDb" {
            $Database = Get-CurrentComDatabase $Database
            if ($null -eq $Database) {
                Write-Error "No database specified and current database isn't set"
                return
            }
            $Path = (New-TemporaryFile).FullName
            Set-ComDatabase $Path -NoProgress -Database $Database
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
.PARAMETER Factory
Specify to return the implemented factory interfaces.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMInterfaceInstance[]
.EXAMPLE
Get-ComClassInterface -ClassEntry $cls
Get instance interfaces for a COM class.
.EXAMPLE
Get-ComClassInterface -ClassEntry $cls -Factory
Get factory interfaces for a COM class.
.EXAMPLE
Get-ComClassInterface -ClassEntry $cls -Refresh
Get instance interfaces for a COM class forcing them to be refreshed if necessary.
#>
function Get-ComClassInterface {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [OleViewDotNet.ICOMClassEntry]$ClassEntry,
        [switch]$Refresh,
        [switch]$Factory
        )
    PROCESS {
        $ClassEntry.LoadSupportedInterfaces($Refresh) | Out-Null
        if ($Factory) {
            $ClassEntry.FactoryInterfaces | Write-Output
        } else {
            $ClassEntry.Interfaces | Write-Output
        }
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
Specify a name to equal the class name.
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
Get-ComRuntimeClass -Database $comdb -Name "Windows.ABC.XYZ"
Get COM Runtime classes with the name Windows.ABC.XYZ.
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
        [OleViewDotNet.COMRegistry]$Database,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromDllPath")]
        [string]$DllPath,
        [Parameter(Mandatory, ParameterSetName = "FromActivationType")]
        [OleViewDotNet.ActivationType]$ActivationType 
    )
    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.RuntimeClasses.Values
        }
        "FromName" {
            Get-ComRuntimeClass -Database $Database | ? Name -eq $Name | Write-Output
        }
        "FromDllPath" {
            Get-ComRuntimeClass -Database $Database | ? DllPath -eq $DllPath | Write-Output
        }
        "FromActivationType" {
            Get-ComRuntimeClass -Database $Database | ? ActivationType -eq $ActivationType | Write-Output
        }
    }
}

<#
.SYNOPSIS
Get COM interfaces from a database.
.DESCRIPTION
This cmdlet gets COM interfaces from the database based on a set of criteria. The default is to return all registered interfaces.
.PARAMETER Database
The database to use. If not specified then the current database is used.
.PARAMETER Iid
Specify a IID to lookup.
.PARAMETER Name
Specify a name to match against the interface name.
.PARAMETER Object
A running COM object to query for interfaces (can take a long time/hang).
.PARAMETER Proxy
Return interfaces which have a registered proxy class.
.PARAMETER TypeLib
Return interfaces which have a registered type library.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMInterfaceEntry
.EXAMPLE
Get-ComInterface
Get all COM interfaces from the current database.
.EXAMPLE
Get-ComInterface -Database $comdb
Get all COM interfaces from a specific database.
.EXAMPLE
Get-ComInterface -Iid "00000001-0000-0000-C000-000000000046"
Get COM interface from an IID from a database.
.EXAMPLE
Get-ComInterface -Name "IBlah"
Get COM interface which is called IBlah.
.EXAMPLE
Get-ComInterface -Object $obj
Get COM interfaces supported by an object.
#>
function Get-ComInterface {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromObject")]
        [object]$Object,
        [Parameter(Mandatory, ParameterSetName = "FromProxy")]
        [switch]$Proxy,
        [Parameter(Mandatory, ParameterSetName = "FromTypeLib")]
        [switch]$TypeLib,
        [OleViewDotNet.COMRegistry]$Database
    )
    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Interfaces.Values
        }
        "FromName" {
            Get-ComInterface -Database $Database | ? Name -eq $Name | Write-Output
        }
        "FromIid" {
            $Database.Interfaces[$Iid] | Write-Output
        }
        "FromObject" {
            $Database.GetInterfacesForObject($Object) | Write-Output
        }
        "FromProxy" {
            Get-ComInterface -Database $Database | ? ProxyClassEntry -ne $null | Write-Output
        }
        "FromTypeLib" {
            Get-ComInterface -Database $Database | ? TypeLibEntry -ne $null | Write-Output
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
Get-ComClass | Select-ComAccess
Get all COM classes which are accessible by the current process.
.EXAMPLE
Get-ComClass | Select-ComAccess -IgnoreDefault
Get all COM classes which are accessible by the current process ignoring default security permissions.
.EXAMPLE
Get-ComClass | Select-ComAccess -Token $token
Get all COM classes which are accessible by a specified token.
.EXAMPLE
Get-ComClass | Select-ComAccess -Process $process
Get all COM classes which are accessible by a specified process.
.EXAMPLE
Get-ComClass | Select-ComAccess -ProcessId 1234
Get all COM classes which are accessible by a specified process from its ID.
.EXAMPLE
Get-ComClass | Select-ComAccess -Access 0
Only check for launch permissions and ignore access permissions.
.EXAMPLE
Get-ComClass | Select-ComAccess -LaunchAccess 0
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

function Out-ObjRef {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, ValueFromPipeline)]
        [OleViewDotNet.ComObjRef]$ObjRef,
        [ComObjRefOutput]$Output = "Object"
    )

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
Get an OBJREF for a COM object.
.DESCRIPTION
This cmdlet marshals a COM object to an OBJREF, returning a byte array, a COMObjRef object or a moniker.
.PARAMETER Object
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
        [object]$Object,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromPath")]
        [string]$Path,
        [ComObjRefOutput]$Output = "Object",
        [Parameter(ParameterSetName = "FromObject", ValueFromPipelineByPropertyName)]
        [Guid]$Iid = "00000000-0000-0000-C000-000000000046",
        [Parameter(ParameterSetName = "FromObject")]
        [OleViewDotNet.MSHCTX]$MarshalContext = "DIFFERENTMACHINE",
        [Parameter(ParameterSetName = "FromObject")]
        [OleViewdotNet.MSHLFLAGS]$MarshalFlags = "NORMAL"
    )

    BEGIN {
        switch($PSCmdlet.ParameterSetName) {
            "FromObject" {
                $Object = Unwrap-ComObject $Object
            }
        }
    }

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromObject" {
                [OleViewDotNet.COMUtilities]::MarshalObjectToObjRef($Object, `
                        $Iid, $MarshalContext, $MarshalFlags) | Out-ObjRef -Output $Output
            }
            "FromPath" {
                $ba = Get-Content -Path $Path -Encoding Byte
                [OleViewDotNet.COMObjRef]::FromArray($ba) | Out-ObjRef -Output $Output
            }
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
.PARAMETER NoWrapper
Don't wrap object in a callable wrapper.
.PARAMETER Moniker
Specify a moniker to bind to.
#>
function New-ComObject {
    [CmdletBinding(DefaultParameterSetName="FromClass")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClass")]
        [OleViewDotNet.ICOMClassEntry]$Class,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromFactory")]
        [OleViewDotNet.IClassFactory]$Factory,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromActivationFactory")]
        [System.Runtime.InteropServices.WindowsRuntime.IActivationFactory]$ActivationFactory,
        [Parameter(Mandatory, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [Parameter(ParameterSetName = "FromClsid")]
        [Parameter(ParameterSetName = "FromClass")]
        [OleViewDotNet.CLSCTX]$ClassContext = "ALL",
        [Parameter(ParameterSetName = "FromClsid")]
        [Parameter(ParameterSetName = "FromClass")]
        [string]$RemoteServer,
        [Parameter(ParameterSetName = "FromObjRef")]
        [OleViewDotNet.COMObjRef]$ObjRef,
        [Parameter(ParameterSetName = "FromIpid")]
        [OleViewDotNet.COMIPIDEntry]$Ipid,
        [switch]$NoWrapper
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                $obj = $Class.CreateInstanceAsObject($ClassContext, $RemoteServer)
            }
            "FromClsid" {
                $obj = [OleViewDotNet.COMUtilities]::CreateInstanceAsObject($Clsid, `
                    "00000000-0000-0000-C000-000000000046", $ClassContext, $RemoteServer)
            }
            "FromFactory" {
                $obj = [OleViewDotNet.COMUtilities]::CreateInstanceFromFactory($Factory, `
                    "00000000-0000-0000-C000-000000000046")
            }
            "FromActivationFactory" {
                $obj = $ActivationFactory.ActivateInstance()
            }
            "FromObjRef" {
                $obj = [OleViewDotNet.COMUtilities]::UnmarshalObject($ObjRef)
            }
            "FromIpid" {
                $obj = [OleViewDotNet.COMUtilities]::UnmarshalObject($Ipid.ToObjRef())
            }
        }

        if ($null -ne $obj) {
            $type = [OleViewDotNet.IUnknown]
            Wrap-ComObject $obj -Type $type -NoWrapper:$NoWrapper | Write-Output
        }
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
.PARAMETER NoWrapper
Don't wrap factory object in a callable wrapper.
#>
function New-ComObjectFactory {
    [CmdletBinding(DefaultParameterSetName="FromClass")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClass")]
        [OleViewDotNet.ICOMClassEntry]$Class,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [OleViewDotNet.CLSCTX]$ClassContext = "ALL",
        [string]$RemoteServer,
        [switch]$NoWrapper
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                $obj = $Class.CreateClassFactory($ClassContext, $RemoteServer)
            }
            "FromClsid" {
                $obj = [OleViewDotNet.COMUtilities]::CreateClassFactory($Clsid, `
                    "00000000-0000-0000-C000-000000000046", $ClassContext, $RemoteServer)
            }
        }

        if ($null -ne $obj) {
            $type = [OleViewDotNet.PowerShell.PowerShellUtils]::GetFactoryType($Class)
            Wrap-ComObject $obj $type -NoWrapper:$NoWrapper | Write-Output
        }
    }
}

<#
.SYNOPSIS
Creates a new COM moniker instance and optionally binds to it.
.DESCRIPTION
This cmdlet creates a new COM moniker instance and optionally binds to the object.
.PARAMETER NoWrapper
Don't wrap object in a callable wrapper.
.PARAMETER Moniker
Specify a moniker to parse.
.PARAMETER Bind
Bind to parsed moniker.
.PARAMETER Composite
Parse the moniker as a composite, each component separated by a '!'
#>
function Get-ComMoniker {
    Param(
        [Parameter(Mandatory, Position = 0)]
        [string]$Moniker,
        [switch]$Bind,
        [switch]$Composite,
        [switch]$NoWrapper
    )

    if ($Bind) {
        $type = [OleViewDotNet.IUnknown]
        $obj = [OleViewDotNet.COMUtilities]::ParseAndBindMoniker($Moniker, $Composite)
    } else {
        $type = [System.Runtime.InteropServices.ComTypes.IMoniker]
        $obj = [OleViewDotNet.COMUtilities]::ParseMoniker($Moniker, $Composite)
    }

    if ($null -ne $obj) {
        Wrap-ComObject $obj $type -NoWrapper:$NoWrapper | Write-Output
    }
}

<#
.SYNOPSIS
Gets the display name from a COM moniker.
.DESCRIPTION
This cmdlet gets the display name from a COM moniker
.PARAMETER Moniker
Specify a moniker to get the display name from.
#>
function Get-ComMonikerDisplayName {
    Param(
        [Parameter(Mandatory, Position = 0)]
        [System.Runtime.InteropServices.ComTypes.IMoniker]$Moniker
    )

    [OleViewDotNet.COMUtilities]::GetMonikerDisplayName($Moniker) | Write-Output
}

<#
.SYNOPSIS
Parses COM proxy information for an interface or a proxy class.
.DESCRIPTION
This cmdlet parses the COM proxy information for an interface or specified COM proxy class. If a class is specified all interfaces
from that class are returned.
.PARAMETER Class
A COM proxy class.
.PARAMETER Interface
A COM interface with a registered proxy.
.PARAMETER InterfaceInstance
A COM interface instance.
.PARAMETER Iid
A COM IID which is being proxied.
.PARAMETER AsText
Return the results as text rather than objects.
.OUTPUTS
The parsed proxy information and complex types.
.EXAMPLE
Get-ComProxy $intf
Parse the proxy information for an interface.
.EXAMPLE
Get-ComProxy $class
Parse the proxy information for a class.
.EXAMPLE
Get-ComProxy "00000001-0000-0000-C000-000000000046"
Parse the proxy based on a IID.
#>
function Get-ComProxy {
    [CmdletBinding(DefaultParameterSetName = "FromIid")]
    Param(
        [parameter(Mandatory, Position=0, ParameterSetName = "FromInterface", ValueFromPipeline)]
        [OleViewDotNet.COMInterfaceEntry]$Interface,
        [parameter(Mandatory, Position=0, ParameterSetName = "FromInterfaceInstance", ValueFromPipeline)]
        [OleViewDotNet.COMInterfaceInstance]$InterfaceInstance,
        [parameter(Mandatory, Position=0, ParameterSetName = "FromClass")]
        [OleViewDotNet.COMClsidEntry]$Class,
        [parameter(Mandatory, Position=0, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [parameter(ParameterSetName = "FromIid")]
        [OleViewDotNet.COMRegistry]$Database,
        [switch]$AsText
    )

    PROCESS {
        $proxy = switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                [OleViewDotNet.COMProxyInstance]::GetFromCLSID($Class, $null)
            }
            "FromInterface" {
                [OleViewDotNet.COMProxyInterfaceInstance]::GetFromIID($Interface, $null)
            }
            "FromInterfaceInstance" {
                [OleViewDotNet.COMProxyInterfaceInstance]::GetFromIID($InterfaceInstance, $null)
            }
            "FromIid" {
                $Database = Get-CurrentComDatabase $Database
                if ($null -eq $Database) {
                    Write-Error "No database specified and current database isn't set"
                    return
                }
                $intf = Get-ComInterface -Database $Database -Iid $Iid
                if ($null -ne $intf) {
                    [OleViewDotNet.COMProxyInterfaceInstance]::GetFromIID($intf, $null)
                }
            }
        }
        if ($null -ne $proxy) {
            if ($AsText) {
                Write-Output $proxy.FormatText()
            } else {
                Write-Output $proxy
            }
        }
    }
}

<#
.SYNOPSIS
Sets the COM symbol resolver paths.
.DESCRIPTION
This cmdlet sets the COM symbol resolver paths. This allows you to specify symbol resolver paths for cmdlets which support it.
.PARAMETER DbgHelpPath
Specify path to a dbghelp DLL to use for symbol resolving. This should be ideally the dbghelp from debugging tool for Windows
which will allow symbol servers however you can use the system version if you just want to pull symbols locally.
.PARAMETER SymbolPath
Specify path for the symbols.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Set-ComSymbolResolver -DbgHelpPath c:\windbg\x64\dbghelp.dll
Specify the global dbghelp path.
.EXAMPLE
Set-ComSymbolResolver -DbgHelpPath dbghelp.dll -SymbolPath "c:\symbols"
Specify the global dbghelp path using c:\symbols to source the symbol files.
#>
function Set-ComSymbolResolver {
    Param(
        [alias("dbghelp")]
        [parameter(Mandatory, Position=0)]
        [string]$DbgHelpPath,
        [parameter(Position=1)]
        [string]$SymbolPath
    )

    $Script:GlobalDbgHelpPath = $DbgHelpPath
    if ("" -ne $SymbolPath) {
        $Script:GlobalSymbolPath = $SymbolPath
    }
}

<#
.SYNOPSIS
Gets IPID entries for a COM object.
.DESCRIPTION
This cmdlet gets the IPID entries for a COM object. It queries for all known remote interfaces on the object, marshal the interfaces
then parse the containing process. If the containing process cannot be opend then this will fail.
.PARAMETER Database
The COM database to extract information from.
.PARAMETER object
The object to query.
.PARAMETER DbgHelpPath
Specify path to a dbghelp DLL to use for symbol resolving. This should be ideally the dbghelp from debugging tool for Windows
which will allow symbol servers however you can use the system version if you just want to pull symbols locally.
.PARAMETER SymbolPath
Specify path for the symbols.
.PARAMETER ResolveMethodNames
Specify to try and resolve method names for interfaces.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMIPIDEntry[]
.EXAMPLE
Get-ComObjectIpid $comdb $obj
Get all
.EXAMPLE
Set-ComSymbolResolver -DbgHelpPath dbghelp.dll -SymbolPath "c:\symbols"
Specify the global dbghelp path using c:\symbols to source the symbol files.
#>
function Get-ComObjectIpid {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, Position=0)]
        [object]$Object,
        [OleViewDotNet.COMRegistry]$Database,
        [alias("dbghelp")]
        [string]$DbgHelpPath = "",
        [string]$SymbolPath = "",
        [switch]$ResolveMethodNames
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }

    $resolver = Get-ComSymbolResolver $DbgHelpPath $SymbolPath
    $ps = Get-ComInterface -Database $Database -Object $Object | Get-ComObjRef $Object | Get-ComProcess -Database $Database `
        -DbgHelpPath $resolver.DbgHelpPath -ParseStubMethods -SymbolPath $resolver.SymbolPath -ResolveMethodNames:$ResolveMethodNames
    $ps.Ipids | Write-Output
}

<#
.SYNOPSIS
Get register class information from COM processes.
.DESCRIPTION
This cmdlet parses all accessible processes for registered COM clsses.
.PARAMETER Database
The database to use to lookup information.
.PARAMETER DbgHelpPath
Specify location of DBGHELP.DLL file. For remote symbol support use one from Debugging Tools for Windows.
.PARAMETER SymbolPath
Specify the location of symbols for the resolver.
.PARAMETER NoProgress
Don't show progress for process parsing.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMProcessClassRegistration
.EXAMPLE
Get-ComRegisteredClass
Get all COM registered classes accessible.
#>
function Get-ComRegisteredClass {
    [CmdletBinding()]
    Param(
        [OleViewDotNet.COMRegistry]$Database,
        [alias("dbghelp")]
        [string]$DbgHelpPath = "",
        [string]$SymbolPath = "",
        [switch]$NoProgress
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    Get-ComProcess -DbgHelpPath $DbgHelpPath -SymbolPath $SymbolPath -Database $Database -ParseRegisteredClasses -NoProgress:$NoProgress `
        | select -ExpandProperty Classes | ? {$_.Context -eq 0 -or $_.Context -match "LOCAL_SERVER"} | Write-Output
}

<#
.SYNOPSIS
Formats a COM proxy or IPID as text.
.DESCRIPTION
This cmdlet formats a COM proxy object or IPID entry as text.
.PARAMETER Proxy
The proxy or IPID entry to format.
.PARAMETER Flags
Specify flags for the formatter.
.INPUTS
OleViewDotNet.IProxyFormatter
.OUTPUTS
string
.EXAMPLE
Format-ComProxy $proxy
Format a COM proxy as text.
.EXAMPLE
Format-ComProxy $proxy -Flags RemoveComments
Format a COM proxy as text removing comments.
.EXAMPLE
Format-ComProxy $proxy -Flags RemoveComplexTypes
Format a COM proxy as text removing complex types.
.EXAMPLE
$ipids | Format-ComProxy
Format a list of IPIDs as text.
#>
function Format-ComProxy {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, Position=0, ValueFromPipeline)]
        [OleViewDotNet.IProxyFormatter]$Proxy,
        [OleViewDotNet.ProxyFormatterFlags]$Flags = 0
    )

    PROCESS {
        $Proxy.FormatText($Flags) | Write-Output
    }
}

<#
.SYNOPSIS
Gets registered COM type libraries.
.DESCRIPTION
This cmdlet gets all COM type libraries from a database.`
.PARAMETER Database
The database to use to lookup information.
.PARAMETER Iid
Get type library from an IID if that interface has a type library.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMTypeLibVersionEntry[]
.EXAMPLE
Get-ComTypeLib
Get all COM registered type libraries.
#>
function Get-ComTypeLib {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [OleViewDotNet.COMRegistry]$Database,
        [parameter(Mandatory, ParameterSetName = "FromIid")]
        [Guid]$Iid
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            $Database.Typelibs.Values.Versions | Write-Output
        }
        "FromIid" {
            $intf = Get-ComInterface -Database $Database -Iid $Iid
            if ($null -ne $intf) {
                $intf.TypeLibVersionEntry | Write-Output
            }
        }
    }
}

<#
.SYNOPSIS
Gets a .NET assembly which represents the type library.
.DESCRIPTION
This cmdlet converts a type library to a .NET assembly. This process can take a while depending on the size of the library.
.PARAMETER TypeLib
The type library version entry to convert.
.PARAMETER Path
The path to a type library to convert.
.PARAMETER NoProgress
Don't show progress during conversion.
.INPUTS
OleViewDotNet.COMTypeLibVersionEntry or string.
.OUTPUTS
System.Reflection.Assembly
.EXAMPLE
Get-ComTypeLibAssembly $typelib
Get a .NET assembly from a type library entry.
#>
function Get-ComTypeLibAssembly {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromTypeLib", Position=0)]
        [OleViewDotNet.COMTypeLibVersionEntry]$TypeLib,
        [parameter(Mandatory, ParameterSetName = "FromPath")]
        [string]$Path,
        [switch]$NoProgress
    )

    PROCESS {
        $callback = New-CallbackProgress -Activity "Converting TypeLib" -NoProgress:$NoProgress
        if ($PSCmdlet.ParameterSetName -eq "FromTypeLib") {
            $Path = ""
            if ([Environment]::Is64BitProcess) {
                $Path = $TypeLib.Win64Path
            }
            if ($Path -eq "") {
                $Path = $TypeLib.Win32Path
            }
        }
        if ($Path -ne "") {
            [OleViewDotNet.COMUtilities]::LoadTypeLib($Path, $callback) | Write-Output
        }
    }
}

<#
.SYNOPSIS
Formats a .NET assembly or Type which represents COM type.
.DESCRIPTION
This cmdlet formats a .NET assembly or a .NET type which represents a COM type.
.PARAMETER Assembly
Specify a converted COM type library assembly.
.PARAMETER InterfacesOnly
Only convert interfaces to text.
.PARAMETER Type
Specify COM type object.
.PARAMETER TypeLib
Specify a registered COM typelib entry to format.
.PARAMETER NoProgress
Don't show progress for conversion.
.PARAMETER Path
The path to a type library to format.
.INPUTS
System.Reflection.Assembly
.OUTPUTS
string
.EXAMPLE
Format-ComTypeLib $typelib
Format a .NET assembly.
.EXAMPLE
Format-ComTypeLib $type
Format a .NET assembly.
#>
function Format-ComTypeLib {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, ValueFromPipeline, Position=0, ParameterSetName = "FromAssembly")]
        [System.Reflection.Assembly]$Assembly,
        [parameter(Mandatory, ValueFromPipeline, Position=0, ParameterSetName = "FromType")]
        [System.Type]$Type,
        [parameter(Mandatory, ValueFromPipeline, Position=0, ParameterSetName = "FromTypeLib")]
        [OleViewDotNet.COMTypeLibVersionEntry]$TypeLib,
        [parameter(Mandatory, ParameterSetName = "FromPath")]
        [string]$Path,
        [parameter(ParameterSetName = "FromAssembly")]
        [parameter(ParameterSetName = "FromTypeLib")]
        [parameter(ParameterSetName = "FromPath")]
        [switch]$InterfacesOnly,
        [parameter(ParameterSetName = "FromTypeLib")]
        [parameter(ParameterSetName = "FromPath")]
        [switch]$NoProgress
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromAssembly" {
                [OleViewDotNet.COMUtilities]::FormatComAssembly($Assembly, $InterfacesOnly) | Write-Output
            }
            "FromType" {
                [OleViewDotNet.COMUtilities]::FormatComType($Type) | Write-Output
            }
            "FromTypeLib" {
                Get-ComTypeLibAssembly -TypeLib $TypeLib -NoProgress:$NoProgress | Format-ComTypeLib -InterfacesOnly:$InterfacesOnly
            }
            "FromPath" {
                Get-ComTypeLibAssembly -Path $Path -NoProgress:$NoProgress | Format-ComTypeLib -InterfacesOnly:$InterfacesOnly
            }
        }
    }
}

<#
.SYNOPSIS
Formats a GUID in various formats.
.DESCRIPTION
This cmdlet formats a GUID in various formats.
.INPUTS
System.Guid
.OUTPUTS
string
.EXAMPLE
Format-ComGuid $Guid
Format a GUID to a string.
.EXAMPLE
Format-ComGuid $Guid Object
Format a GUID to a HTML object.
#>
function Format-ComGuid {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, ValueFromPipeline, Position=0)]
        [Guid]$Guid,
        [OleViewDotNet.GuidFormat]$Format = "String"
    )

    PROCESS {
        [OleViewDotNet.COMUtilities]::GuidToString($Guid, $Format)
    }
}

<#
.SYNOPSIS
Generate a symbol cache file for the current base DLL.
.DESCRIPTION
This cmdlet generates a symbol cache file in a specified directory. This should be copied to cached_symbols directory 
in the installation to allow you to parse processes without symbols. You need to do this in both a 32 and 64 bit 
process to get support for both symbols.
.PARAMETER Path
The directory where the cached symbol file will be created.
.PARAMETER DbgHelpPath
Specify location of DBGHELP.DLL file. For remote symbol support use one from Debugging Tools for Windows.
.PARAMETER SymbolPath
Specify the location of symbols for the resolver.
.INPUTS
None
.OUTPUTS
None
.EXAMPLE
Set-ComSymbolCache .\symbol_cache
Generates a symbol cache in the "symbol_cache" directory.
#>
function Set-ComSymbolCache {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, Position = 0)]
        [string]$Path,
        [alias("dbghelp")]
        [string]$DbgHelpPath = "",
        [string]$SymbolPath = ""
    )

    $resolver = Get-ComSymbolResolver $DbgHelpPath $SymbolPath
    $Path = Resolve-Path $Path
    if ($null -ne $Path) {
        [OleViewDotNet.COMUtilities]::ClearCachedSymbols()
        [OleViewDotNet.COMUtilities]::GenerateSymbolFile($Path, $resolver.DbgHelpPath, $resolver.SymbolPath)
        [OleViewDotNet.COMUtilities]::SetupCachedSymbols()
    }
}

<#
.SYNOPSIS
Creates a new OLE storage object.
.DESCRIPTION
This cmdlet creates a new OLE storage object based on a file.
.PARAMETER Path
The path to the storage object to create.
.PARAMETER Mode
The mode to use when creating the storage object.
.PARAMETER Format
The format of the storage object to create.
.INPUTS
None
.OUTPUTS
OleViewDotNet.StorageWrapper
.EXAMPLE
New-ComStorageObject storage.stg
Creates a new storage object.
#>
function New-ComStorageObject {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, Position = 0)]
        [string]$Path,
        [OleViewDotNet.STGM]$Mode = "SHARE_EXCLUSIVE, READWRITE",
        [OleViewDotNet.STGFMT]$Format = "Storage"
    )

    $type = [OleViewDotNet.IStorage]
    $iid = $type.GUID
    $Path = Resolve-LocalPath $Path
    [OleViewDotNet.COMUtilities]::CreateStorage($Path, $Mode, $Format)
}

<#
.SYNOPSIS
Opens an existing OLE storage object.
.DESCRIPTION
This cmdlet opens a existing OLE storage object based on a file.
.PARAMETER Path
The path to the storage object to open.
.PARAMETER Mode
The mode to use when opening the storage object.
.PARAMETER Format
The format of the storage object to open.
.PARAMETER ReadOnly
Opens storage read only. Overrides $Mode parameter.
.INPUTS
None
.OUTPUTS
OleViewDotNet.StorageWrapper
.EXAMPLE
Get-ComStorageObject storage.stg
Creates a new storage object.
#>
function Get-ComStorageObject {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, Position = 0)]
        [string]$Path,
        [switch]$ReadOnly,
        [OleViewDotNet.STGM]$Mode = "SHARE_EXCLUSIVE, READWRITE",
        [OleViewDotNet.STGFMT]$Format = "Storage"
    )

    $type = [OleViewDotNet.IStorage]
    $iid = $type.GUID
    $Path = Resolve-LocalPath $Path
    if ($ReadOnly) {
        $Mode = "SHARE_EXCLUSIVE, READ"
    }

    [OleViewDotNet.COMUtilities]::OpenStorage($Path, $Mode, $Format)
}

<#
.SYNOPSIS
Get COM runtime interfaces from local metadata.
.DESCRIPTION
This cmdlet gets types representing COM runtime interfaces based on a set of criteria. The default is to return all interfaces.
.PARAMETER Iid
Specify a IID to lookup.
.PARAMETER Name
Specify a name to match against the interface name.
.PARAMETER FullName
Specify a full name to match against the interface name.
.INPUTS
None
.OUTPUTS
System.Type
.EXAMPLE
Get-ComRuntimeInterface
Get all COM runtime interfaces.
.EXAMPLE
Get-ComRuntimeInterface -Iid "00000001-0000-0000-C000-000000000046"
Get COM runtime interface from an IID.
.EXAMPLE
Get-ComRuntimeInterface -Name "IBlah"
Get COM runtime interface called IBlah.
.EXAMPLE
Get-ComRuntimeInterface -FullName "App.ABC.IBlah"
Get COM runtime interface whose full name is App.ABC.IBlah.
#>
function Get-ComRuntimeInterface {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromIid", ValueFromPipelineByPropertyName)]
        [Guid]$Iid,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromFullName")]
        [string]$FullName
    )
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            [OleViewDotNet.COMUtilities]::RuntimeInterfaceMetadata.Values | Write-Output 
        }
        "FromName" {
            Get-ComRuntimeInterface | ? Name -eq $Name | Write-Output
        }
        "FromFullName" {
            Get-ComRuntimeInterface | ? FullName -eq $FullName | Write-Output
        }
        "FromIid" {
            [OleViewDotNet.COMUtilities]::RuntimeInterfaceMetadata[$Iid] | Write-Output
        }
    }
}