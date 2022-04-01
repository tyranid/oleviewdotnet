﻿# This file is part of OleViewDotNet.
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

$Script:GlobalDbgHelpPath = [OleViewDotNet.COMUtilities]::GetDefaultDbgHelp()
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
        [Parameter(ParameterSetName = "FromIid")]
        [switch]$LoadType,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromType")]
        [Type]$Type,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromInterface")]
        [OleViewDotNet.Database.COMInterfaceEntry]$Interface,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromInterfaceInstance")]
        [OleViewDotNet.Database.COMInterfaceInstance]$InterfaceInstance,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromIpid")]
        [OleViewDotNet.COMIPIDEntry]$Ipid,
        [switch]$NoWrapper,
        [OleViewDotNet.Database.COMRegistry]$Database
    )

    if ($NoWrapper) {
        return $Object
    }

    $db = $null

    if ($LoadType) {
        $db = Get-CurrentComDatabase $Database
        if ($null -eq $db) {
            Write-Error "No database specified and current database isn't set"
            return
        }
     }

    switch($PSCmdlet.ParameterSetName) {
        "FromIid" {
            [OleViewDotNet.Wrappers.COMWrapperFactory]::Wrap($Object, $Iid, $db)
        }
        "FromType" {
            [OleViewDotNet.Wrappers.COMWrapperFactory]::Wrap($Object, $Type)
        }
        "FromInterface" {
            [OleViewDotNet.Wrappers.COMWrapperFactory]::Wrap($Object, $Interface)
        }
        "FromInterfaceInstance" {
            [OleViewDotNet.Wrappers.COMWrapperFactory]::Wrap($Object, $InterfaceInstance)
        }
        "FromIpid" {
            [OleViewDotNet.Wrappers.COMWrapperFactory]::Wrap($Object, $Ipid)
        }
    }
}

function Unwrap-ComObject {
    [CmdletBinding(DefaultParameterSetName = "FromType")]
    Param(
        [Parameter(Mandatory, Position = 0)]
        [object]$Object
    )

    [OleViewDotNet.Wrappers.COMWrapperFactory]::Unwrap($Object)
}

<#
.SYNOPSIS
Wrap a COM object inside a callable wrapper.
.DESCRIPTION
This cmdlet generates a callable wrapper for a COM interface and wraps the object.
.PARAMETER Object
The object to wrap.
.PARAMETER Iid
The interface ID to base the wrapper on.
.PARAMETER LoadType
Specify to load interface type from proxy/typelib if available and not already loaded.
.PARAMETER Database
Specify with LoadType to indicate the database to get interface information from.
.PARAMETER Type
The existing interface type to wrap with.
.PARAMETER Interface
A COM interface from a database. If no existing interface exists for this class it'll try and build one from its proxy.
#>
function Get-ComObjectInterface {
    [CmdletBinding(DefaultParameterSetName = "FromType")]
    Param(
        [Parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [object[]]$Object,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [Parameter(ParameterSetName = "FromIid")]
        [switch]$LoadType,
        [Parameter(ParameterSetName = "FromIid")]
        [OleViewDotNet.Database.COMRegistry]$Database,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromType")]
        [Type]$Type,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromInterface")]
        [OleViewDotNet.Database.COMInterfaceEntry]$Interface,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "FromInterfaceInstance")]
        [OleViewDotNet.Database.COMInterfaceInstance]$InterfaceInstance
    )

    PROCESS {
        foreach($o in $Object) {
            $o = Unwrap-ComObject $o
            switch($PSCmdlet.ParameterSetName) {
                "FromIid" {
                    Wrap-ComObject -Object $o -Iid $Iid -LoadType:$LoadType | Write-Output
                }
                "FromType" {
                    Wrap-ComObject -Object $o -Type $Type | Write-Output
                }
                "FromInterface" {
                    Wrap-ComObject -Object $o -Interface $Interface | Write-Output
                }
                "FromInterfaceInstance" {
                    Wrap-ComObject -Object $o -InterfaceInstance $InterfaceInstance | Write-Output
                }
            }
        }
    }
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
        [OleViewDotNet.Database.COMRegistry]$Database
    )

    if ($null -ne $Database) {
        $Database
    } else {
        if ($null -eq $Script:CurrentComDatabase) {
            $result = $Host.UI.PromptForChoice("Load COM Database?", `
                    "No database specified, do you want to load the default?", @("&Yes", "&No"), 0)
            if ($result -eq 0) {
                Get-ComDatabase
            }
        }
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
        [OleViewDotNet.Database.COMRegistry]$Database
    )
    $Script:CurrentComDatabase = $Database
}

<#
.SYNOPSIS
Get a COM database from the registry or a file.
.DESCRIPTION
This cmdlet loads a COM registration information database from the current registry or a file and returns an object which can be inspected or passed to other methods. The
database will be set as the current global data unless -PassThru is specified.
.PARAMETER LoadMode
Specify what to load from the registry.
.PARAMETER User
Specify a user to load when loading user-specific COM registration information.
.PARAMETER Path
Specify a path to load a saved COM database.
.PARAMETER NoProgress
Don't show progress for load.
.PARAMETER SetCurrent
No longer used.
.PARAMETER PassThru
Specify to return the loaded database.
.PARAMETER
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
$db = Get-ComDatabase -PassThru
Load a default, merged COM database and return it as an object.
#>
function Get-ComDatabase {
    [CmdletBinding(DefaultParameterSetName = "FromRegistry")]
    Param(
        [Parameter(ParameterSetName = "FromRegistry")]
        [OleViewDotNet.Database.COMRegistryMode]$LoadMode = "Merged",
        [Parameter(ParameterSetName = "FromRegistry")]
        [NtApiDotNet.Sid]$User,
        [Parameter(Mandatory, ParameterSetName = "FromFile", Position = 0)]
        [string]$Path,
        [switch]$NoProgress,
        [switch]$SetCurrent,
        [switch]$PassThru
    )
    $callback = New-CallbackProgress -Activity "Loading COM Registry" -NoProgress:$NoProgress
    $comdb = switch($PSCmdlet.ParameterSetName) {
        "FromRegistry" {
            [OleViewDotNet.Database.COMRegistry]::Load($LoadMode, $User, $callback)
        }
        "FromFile" {
            $Path = Resolve-Path $Path
            [OleViewDotNet.Database.COMRegistry]::Load($Path, $callback)
        }
    }

    if ($SetCurrent) {
        Write-Warning "SetCurrent is now the default. Use PassThru to return a database object."
    }

    if ($PassThru) {
        Write-Output $comdb
    } else {
        Set-CurrentComDatabase $comdb
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
        [OleViewDotNet.Database.COMRegistry]$Database,
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
        [OleViewDotNet.Database.COMRegistry]$Left,
        [Parameter(Mandatory, Position = 1)]
        [OleViewDotNet.Database.COMRegistry]$Right,
        [OleViewDotNet.Database.COMRegistryDiffMode]$DiffMode = "LeftOnly",
        [switch]$NoProgress
    )
    $callback = New-CallbackProgress -Activity "Comparing COM Registries" -NoProgress:$NoProgress
    [OleViewDotNet.Database.COMRegistry]::Diff($Left, $Right, $DiffMode, $callback)
}

function Where-HasComServer {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [OleViewDotNet.Database.COMCLSIDEntry]$ClassEntry,
        [string]$ServerName,
        [OleViewDotNet.Database.COMServerType]$ServerType
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
.PARAMETER AllowNoReg
Allows the class entry to be returned even if not registered.
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
.PARAMETER CatId
Specify looking up the COM classes based on a category ID.
.PARAMETER CatName
Specify looking up the COM classes based on a category name.
.PARAMETER Source
Specify looking up the COM classes based on a source type.
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
Get-ComClass -PartialClsid "ffe1df5f"
Get COM classes with a partial CLSID.
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
Get COM class registered as an interface proxy for a specific IID.
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
Get COM class based on an object instance.
.EXAMPLE
Get-ComClass -CatId "62c8fe65-4ebb-45e7-b440-6e39b2cdbf29"
Get COM classes in a category with ID 62c8fe65-4ebb-45e7-b440-6e39b2cdbf29.
.EXAMPLE
Get-ComClass -CatName ".NET Category"
Get COM classes in the .NET category.
.EXAMPLE
Get-ComClass -Source Packaged
Get COM classes which came from packaged COM source.
#>
function Get-ComClass {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [OleViewDotNet.Database.COMRegistry]$Database,
        [Parameter(Position = 0, Mandatory, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [Parameter(Mandatory, ParameterSetName = "FromPartialClsid")]
        [string]$PartialClsid,
        [Parameter(ParameterSetName = "FromClsid")]
        [switch]$AllowNoReg,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(ParameterSetName = "FromServer")]
        [string]$ServerName = "",
        [Parameter(ParameterSetName = "FromServer")]
        [OleViewDotNet.Database.COMServerType]$ServerType = "UnknownServer",
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
        [object]$Object,
        [Parameter(Mandatory, ParameterSetName = "FromCatId")]
        [Guid]$CatId,
        [Parameter(Mandatory, ParameterSetName = "FromCatName")]
        [string]$CatName,
        [Parameter(Mandatory, ParameterSetName = "FromSource")]
        [OleViewDotNet.Database.COMRegistryEntrySource]$Source,
        [Parameter(Mandatory, ParameterSetName = "FromTrustedMarshaller")]
        [switch]$TrustedMarshaller
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set."
        return
    }

    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Clsids.Values
        }
        "FromClsid" {
            if ($AllowNoReg) {
                $Database.MapClsidToEntry($Clsid) | Write-Output
            } else {
                Write-Output $Database.Clsids[$Clsid]
            }
        }
        "FromPartialClsid" {
            Get-ComClass -Database $Database | Where-Object Clsid -Match $PartialClsid | Write-Output
        }
        "FromName" {
            Get-ComClass -Database $Database | Where-Object Name -eq $Name | Write-Output
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
            Get-ComClass -Database $Database | Where-Object { $_.HasAppID -and $_.AppIDEntry.RunAs -eq  "Interactive User" } | Write-Output
        }
        "FromService" {
            Get-ComClass -Database $Database | Where-Object { $_.HasAppID -and $_.AppIDEntry.IsService } | Write-Output
        }
        "FromServiceName" {
            Get-ComClass -Database $Database -Service | Where-Object { $_.AppIDEntry.ServiceName -eq $ServiceName } | Write-Output
        }
        "FromObject" {
            $Object = Unwrap-ComObject $Object
            $Clsid = [OleViewDotNet.COMUtilities]::GetObjectClass($Object)
            if ($Clsid -ne [Guid]::Empty) {
                Get-ComClass -Database $Database -Clsid $Clsid | Write-Output
            }
        }
        "FromCatId" {
            Get-ComCategory -CatId $CatId | Select-Object -ExpandProperty ClassEntries | Write-Output
        }
        "FromCatName" {
            Get-ComCategory -Name $CatName | Select-Object -ExpandProperty ClassEntries | Write-Output
        }
        "FromSource" {
            Get-ComClass -Database $Database | Where-Object Source -eq $Source | Write-Output
        }
        "FromTrustedMarshaller" {
            Get-ComClass -Database $Database | Where-Object TrustedMarshaller
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
.PARAMETER ParseActivationContext
Specify to parse process activation context.
.PARAMETER NoProgress
Don't show progress for process parsing.
.PARAMETER ServiceName
Specify names of services to parse.
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
Get-Process notepad | Get-ComProcess
Get COM process from a list of processes.
#>
function Get-ComProcess {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [OleViewDotNet.Database.COMRegistry]$Database,
        [alias("dbghelp")]
        [string]$DbgHelpPath = "",
        [string]$SymbolPath = "",
        [switch]$ParseStubMethods,
        [switch]$ResolveMethodNames,
        [switch]$ParseRegisteredClasses,
        [switch]$ParseClients,
        [switch]$ParseActivationContext,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromProcessId")]
        [alias("pid")]
        [int[]]$ProcessId,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromProcess")]
        [System.Diagnostics.Process[]]$Process,
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName = "FromObjRef")]
        [OleViewDotNet.COMObjRef[]]$ObjRef,
        [parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [parameter(Mandatory, ParameterSetName = "FromServiceName")]
        [string[]]$ServiceName,
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
                    $ParseStubMethods, $ResolveMethodNames, $ParseRegisteredClasses, $ParseClients, $ParseActivationContext)

        switch($PSCmdlet.ParameterSetName) {
            "FromObjRef" {
                [OleViewDotNet.COMProcessParser]::GetProcesses([OleViewDotNet.COMObjRef[]]$objrefs, $config, $callback, $Database) | Write-Output
            }
            "FromServiceName" {
                [OleViewDotNet.COMProcessParser]::GetProcesses($ServiceName, $config, $callback, $Database) | Write-Output
            }
            default {
                [OleViewDotNet.COMProcessParser]::GetProcesses([System.Diagnostics.Process[]]$procs, $config, $callback, $Database) | Write-Output
            }
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
        [OleViewDotNet.Database.COMRegistry]$Database
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
.PARAMETER Source
Specify looking up the COM AppIDs based on a source type.
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
        [OleViewDotNet.Database.COMRegistry]$Database,
        [Parameter(Position = 0, Mandatory, ParameterSetName = "FromAppId")]
        [Guid]$AppId,
        [Parameter(Mandatory, ParameterSetName = "FromPartialAppId")]
        [string]$PartialAppId,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(ParameterSetName = "FromServiceName")]
        [string]$ServiceName = "",
        [Parameter(ParameterSetName = "FromIsService")]
        [switch]$IsService,
        [Parameter(Mandatory, ParameterSetName = "FromSource")]
        [OleViewDotNet.Database.COMRegistryEntrySource]$Source
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
        "FromPartialAppId" {
            Get-ComAppId -Database $Database | Where-Object AppId -Match $PartialAppId | Write-Output
        }
        "FromName" {
            Get-ComAppId -Database $Database | Where-Object Name -eq $Name | Write-Output
        }
        "FromServiceName" {
            Get-ComAppId -Database $Database | Where-Object ServiceName -eq $ServiceName | Write-Output
        }
        "FromIsService" {
            Get-ComAppId -Database $Database | Where-Object IsService | Write-Output
        }
        "FromSource" {
            Get-ComAppId -Database $Database | Where-Object Source -eq $Source | Write-Output
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
        [OleViewDotNet.Database.COMRegistry]$Database,
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
The COM or Runtime classes to enumerate.
.PARAMETER ServerType
Specify a type of server to match against. If specified as UnknownServer will search DefaultServerType.
.PARAMETER Refresh
Specify to force the interfaces to be refreshed.
.PARAMETER Factory
Specify to return the implemented factory interfaces.
.PARAMETER NoQuery
Specify to not query for the interfaces at all and only return what's already available.
.PARAMETER NoProgress
Don't show progress for query.
.PARAMETER Token
The token to use when querying for the interfaces.
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
        [OleViewDotNet.Database.ICOMClassEntry[]]$ClassEntry,
        [OleViewDotNet.Database.COMServerType]$ServerType = "UnknownServer",
        [switch]$Refresh,
        [switch]$Factory,
        [switch]$NoQuery,
        [switch]$NoProgress,
        [NtApiDotNet.NtToken]$Token
        )
    PROCESS {
        $i = 0
        foreach($class in $ClassEntry) {
            if (!$NoQuery) {
                if (!$NoProgress) {
                    if ($PSCmdlet.MyInvocation.ExpectingInput) {
                        Write-Progress "Querying for Class Interfaces" -Status $class.Name
                    } else {
                        Write-Progress "Querying for Class Interfaces" -Status $class.Name -PercentComplete (($i / $ClassEntry.Count) * 100)
                        $i++
                    }
                }
                $class.LoadSupportedInterfaces($Refresh, $Token, $ServerType) | Out-Null
            }
            if ($Factory) {
                $class.FactoryInterfaces | Write-Output
            } else {
                $class.Interfaces | Write-Output
            }
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
Get-ComRuntimeClass $comdb
Get all COM Runtime classes from the current database.
.EXAMPLE
Get-ComRuntimeClass -Name "Windows.ABC.XYZ"
Get COM Runtime classes with the name Windows.ABC.XYZ.
.EXAMPLE
Get-ComRuntimeClass -DllPath "c:\path\to\runtime.dll"
Get COM Runtime classes which are implemented in a DLL with the path "c:\path\to\runtime.dll"
.EXAMPLE
Get-ComRuntimeClass -ActivationType OutOfProcess
Get COM Runtime classes which are implemented out-of-process.
#>
function Get-ComRuntimeClass {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [OleViewDotNet.Database.COMRegistry]$Database,
        [Parameter(Position = 0, Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromDllPath")]
        [string]$DllPath,
        [Parameter(Mandatory, ParameterSetName = "FromActivationType")]
        [OleViewDotNet.Database.ActivationType]$ActivationType,
        [Parameter(Mandatory, ParameterSetName = "FromTrustLevel")]
        [OleViewDotNet.Database.TrustLevel]$TrustLevel
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
            $Database.RuntimeClasses[$Name] | Write-Output
        }
        "FromDllPath" {
            Get-ComRuntimeClass -Database $Database | Where-Object DllPath -eq $DllPath | Write-Output
        }
        "FromActivationType" {
            Get-ComRuntimeClass -Database $Database | Where-Object ActivationType -eq $ActivationType | Write-Output
        }
        "FromTrustLevel" {
            Get-ComRuntimeClass -Database $Database | Where-Object TrustLevel -eq $TrustLevel | Write-Output
        }
    }
}

<#
.SYNOPSIS
Get COM Runtime servers from a database.
.DESCRIPTION
This cmdlet gets COM Runtime server from the database based on a set of criteria. The default is to return all registered runtime servers.
.PARAMETER Database
The database to use. Optional if the current database has been set.
.PARAMETER Name
Specify a name to equal the server name.
.PARAMETER ExePath
Specify the executable path to match against.
.PARAMETER ServerType
Specify a type of server to match against.
.PARAMETER IdentityType
Specify the identity type of the server to match against.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMRuntimeClassEntry
.EXAMPLE
Get-ComRuntimeServer
Get all COM Runtime classes from the current data database.
.EXAMPLE
Get-ComRuntimeServer -Name "ABC"
Get COM Runtime server with the name ABC.
#>
function Get-ComRuntimeServer {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [OleViewDotNet.Database.COMRegistry]$Database,
        [Parameter(Position = 0, Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromExePath")]
        [string]$ExePath,
        [Parameter(Mandatory, ParameterSetName = "FromServerType")]
        [OleViewDotNet.Database.ServerType]$ServerType,
        [Parameter(Mandatory, ParameterSetName = "FromIdentityType")]
        [OleViewDotNet.Database.IdentityType]$IdentityType
    )
    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.RuntimeServers.Values
        }
        "FromName" {
            $Database.RuntimeServers[$Name] | Write-Output
        }
        "FromExePath" {
            Get-ComRuntimeServer -Database $Database | Where-Object ExePath -eq $ExePath | Write-Output
        }
        "FromServerType" {
            Get-ComRuntimeServer -Database $Database | Where-Object ServerType -eq $ServerType | Write-Output
        }
        "FromIdentityType" {
            Get-ComRuntimeServer -Database $Database | Where-Object IdentityType -eq $IdentityType | Write-Output
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
.PARAMETER AllowNoReg
Creates an interface entry even if not registered.
.PARAMETER Name
Specify a name to match against the interface name.
.PARAMETER Object
A running COM object to query for interfaces (can take a long time/hang).
.PARAMETER Proxy
Return interfaces which have a registered proxy class.
.PARAMETER TypeLib
Return interfaces which have a registered type library.
.PARAMETER Source
Return interfaces which came from a specific source.
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
        [Parameter(Position = 0, Mandatory, ParameterSetName = "FromIid", ValueFromPipelineByPropertyName)]
        [Guid]$Iid,
        [Parameter(ParameterSetName = "FromIid")]
        [switch]$AllowNoReg,
        [Parameter(Mandatory, ParameterSetName = "FromPartialIid")]
        [string]$PartialIid,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromObject")]
        [object]$Object,
        [Parameter(Mandatory, ParameterSetName = "FromProxy")]
        [switch]$Proxy,
        [Parameter(Mandatory, ParameterSetName = "FromTypeLib")]
        [switch]$TypeLib,
        [Parameter(Mandatory, ParameterSetName = "FromSource")]
        [OleViewDotNet.Database.COMRegistryEntrySource]$Source,
        [OleViewDotNet.Database.COMRegistry]$Database
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
            Get-ComInterface -Database $Database | Where-Object Name -eq $Name | Write-Output
        }
        "FromPartialIid" {
            Get-ComInterface -Database $Database | Where-Object Iid -Match $PartialIid | Write-Output
        }
        "FromIid" {
            if ($AllowNoReg) {
                $Database.MapIidToInterface($Iid) | Write-Output
            } else {
                $Database.Interfaces[$Iid] | Write-Output
            }
        }
        "FromObject" {
            $Database.GetInterfacesForObject($Object) | Write-Output
        }
        "FromProxy" {
            Get-ComInterface -Database $Database | Where-Object ProxyClassEntry -ne $null | Write-Output
        }
        "FromTypeLib" {
            Get-ComInterface -Database $Database | Where-Object TypeLibEntry -ne $null | Write-Output
        }
        "FromSource" {
            Get-ComInterface -Database $Database | Where-Object Source -eq $Source | Write-Output
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
        [alias("pid")]
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
        [parameter(Mandatory, Position = 0, ValueFromPipeline)]
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
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromBytes")]
        [byte[]]$Bytes,
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
            "FromBytes" {
                [OleViewDotNet.COMObjRef]::FromArray($Bytes) | Out-ObjRef -Output $Output
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
.PARAMETER Default
Shows the default security descriptor for the specified database.
.PARAMETER RuntimeDefault
Shows the default security descriptor for the Windows Runtime Broker.
.PARAMETER Restriction
Shows the security descriptor restriction for the specified database.
.PARAMETER Database
Specify the database for showing the default or restriction security descriptors.
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
.EXAMPLE
Show-ComSecurityDescriptor -Default
Show the default launch security descriptor for the current database.
.EXAMPLE
Show-ComSecurityDescriptor -Default -ShowAccess
Show the default access security descriptor for the current database.
.EXAMPLE
Show-ComSecurityDescriptor -Restriction
Show the launch security descriptor restriction for the current database.
.EXAMPLE
Show-ComSecurityDescriptor -Restriction -ShowAccess
Show the access security descriptor restriction for the current database.
#>
function Show-ComSecurityDescriptor {
    [CmdletBinding(DefaultParameterSetName="FromObject")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromSddl")]
        [string]$SecurityDescriptor,
        [Parameter(Mandatory, Position = 0, ValueFromPipeline, ParameterSetName = "FromObject")]
        [OleViewDotNet.ICOMAccessSecurity]$InputObject,
        [Parameter(Mandatory, ParameterSetName = "FromRestriction")]
        [switch]$Restriction,
        [Parameter(Mandatory, ParameterSetName = "FromDefault")]
        [switch]$Default,
        [Parameter(Mandatory, ParameterSetName = "FromRuntimeDefault")]
        [switch]$RuntimeDefault,
        [Parameter(ParameterSetName = "FromRestriction")]
        [Parameter(ParameterSetName = "FromDefault")]
        [OleViewDotNet.Database.COMRegistry]$Database,
        [switch]$ShowAccess
    )

    PROCESS {
        if ($PSCmdlet.ParameterSetName -eq "FromRestriction" -or $PSCmdlet.ParameterSetName -eq "FromDefault") {
            $Database = Get-CurrentComDatabase $Database
            if ($null -eq $Database) {
                Write-Error "No database specified and current database isn't set"
                return
            }
        }
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
            "FromDefault" {
                if ($ShowAccess) {
                    $SecurityDescriptor = $Database.DefaultAccessPermission
                } else {
                    $SecurityDescriptor = $Database.DefaultLaunchPermission
                }
                $name = "Default"
            }
            "FromRestriction" {
                if ($ShowAccess) {
                    $SecurityDescriptor = $Database.DefaultAccessRestriction
                } else {
                    $SecurityDescriptor = $Database.DefaultLaunchRestriction
                }
                $name = "Restriction"
            }
            "FromRuntimeDefault" {
                $SecurityDescriptor = [OleViewDotNet.Database.COMRuntimeClassEntry]::DefaultActivationPermission
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
                $args += @("`"-n=$name`"")
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
.PARAMETER SessionId
Specify the console session to create the object in.
.PARAMETER NoWrapper
Don't wrap object in a callable wrapper.
.PARAMETER Iid
Specify the interface to query for initially.
.PARAMETER LoadType
Specify to load interface type from proxy/typelib if available and not already loaded.
.PARAMETER Database
Specify with LoadType to indicate the database to get interface information from.
.PARAMETER Ipid
Create the object from an existing IPID.
#>
function New-ComObject {
    [CmdletBinding(DefaultParameterSetName="FromClass")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClass", ValueFromPipeline)]
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromSessionIdClass")]
        [OleViewDotNet.Database.ICOMClassEntry]$Class,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromFactory", ValueFromPipeline)]
        [OleViewDotNet.Wrappers.IClassFactoryWrapper]$Factory,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromActivationFactory")]
        [OleViewDotNet.Wrappers.IActivationFactoryWrapper]$ActivationFactory,
        [Parameter(Mandatory, ParameterSetName = "FromClsid")]
        [Parameter(Mandatory, ParameterSetName = "FromSessionIdClsid")]
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
        [Parameter(Mandatory, ParameterSetName = "FromSessionIdClass")]
        [Parameter(Mandatory, ParameterSetName = "FromSessionIdClsid")]
        [int]$SessionId,
        [switch]$NoWrapper,
        [Guid]$Iid = [guid]::Empty,
        [switch]$LoadType,
        [OleViewDotNet.Database.COMRegistry]$Database
    )

    PROCESS {
        $obj = $null
        $iid_set = $true
        if ($Iid -eq [guid]::Empty) {
            $Iid = "00000000-0000-0000-C000-000000000046"
            $iid_set = $false
        }

        switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                $obj = $Class.CreateInstanceAsObject($ClassContext, $RemoteServer)
            }
            "FromClsid" {
                $obj = [OleViewDotNet.COMUtilities]::CreateInstanceAsObject($Clsid, `
                    $Iid, $ClassContext, $RemoteServer)
            }
            "FromFactory" {
                $obj = [OleViewDotNet.COMUtilities]::CreateInstanceFromFactory($Factory, $Iid)
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
            "FromSessionIdClass" {
                $obj = Get-ComMoniker "session:$SessionId!new:$($Class.Clsid)" -Bind -NoWrapper
            }
            "FromSessionIdClsid" {
                $obj = Get-ComMoniker "session:$SessionId!new:$Clsid" -Bind -NoWrapper
            }
        }

        if ($null -ne $obj) {
            if ($null -ne $Ipid -and !$iid_set) {
                Wrap-ComObject $obj -Ipid $Ipid -NoWrapper:$NoWrapper | Write-Output
            } else {
                Wrap-ComObject $obj -Iid $Iid -NoWrapper:$NoWrapper -LoadType:$LoadType -DataBase $DataBase | Write-Output
            }
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
.PARAMETER SessionId
Specify the console session to create the factory in.
.PARAMETER NoWrapper
Don't wrap factory object in a callable wrapper.
#>
function New-ComObjectFactory {
    [CmdletBinding(DefaultParameterSetName="FromClass")]
    Param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClass", ValueFromPipeline)]
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromSessionIdClass")]
        [OleViewDotNet.Database.ICOMClassEntry]$Class,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromClsid")]
        [Parameter(Mandatory, Position = 0, ParameterSetName = "FromSessionIdClsid")]
        [Guid]$Clsid,
        [Parameter(ParameterSetName = "FromClsid")]
        [Parameter(ParameterSetName = "FromClass")]
        [OleViewDotNet.CLSCTX]$ClassContext = "ALL",
        [Parameter(ParameterSetName = "FromClsid")]
        [Parameter(ParameterSetName = "FromClass")]
        [string]$RemoteServer,
        [Parameter(Mandatory, ParameterSetName = "FromSessionIdClass")]
        [Parameter(Mandatory, ParameterSetName = "FromSessionIdClsid")]
        [int]$SessionId,
        [switch]$NoWrapper
    )

    PROCESS {
        $obj = $null
        switch($PSCmdlet.ParameterSetName) {
            "FromClass" {
                $obj = $Class.CreateClassFactory($ClassContext, $RemoteServer)
            }
            "FromClsid" {
                $obj = [OleViewDotNet.COMUtilities]::CreateClassFactory($Clsid, `
                    "00000000-0000-0000-C000-000000000046", $ClassContext, $RemoteServer)
            }
            "FromSessionIdClass" {
                $obj = Get-ComMoniker "session:$SessionId!clsid:$($Class.Clsid)" -Bind -NoWrapper
            }
            "FromSessionIdClsid" {
                $obj = Get-ComMoniker "session:$SessionId!clsid:$Clsid" -Bind -NoWrapper
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

    $obj = $null
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
        [OleViewDotNet.Database.COMInterfaceEntry]$Interface,
        [parameter(Mandatory, Position=0, ParameterSetName = "FromInterfaceInstance", ValueFromPipeline)]
        [OleViewDotNet.Database.COMInterfaceInstance]$InterfaceInstance,
        [parameter(Mandatory, Position=0, ParameterSetName = "FromClass")]
        [OleViewDotNet.Database.COMClsidEntry]$Class,
        [parameter(Mandatory, ParameterSetName = "FromClsid")]
        [Guid]$Clsid,
        [parameter(Mandatory, Position=0, ParameterSetName = "FromIid")]
        [Guid]$Iid,
        [parameter(ParameterSetName = "FromIid")]
        [parameter(ParameterSetName = "FromClsid")]
        [OleViewDotNet.Database.COMRegistry]$Database,
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
                $intf = Get-ComInterface -Database $Database -Iid $Iid
                if ($null -ne $intf) {
                    [OleViewDotNet.COMProxyInterfaceInstance]::GetFromIID($intf, $null)
                }
            }
            "FromClsid" {
                $class = Get-ComClass -Database $Database -Clsid $Clsid
                if ($null -ne $class) {
                    [OleViewDotNet.COMProxyInstance]::GetFromCLSID($class, $null)
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
        [OleViewDotNet.Database.COMRegistry]$Database,
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
Get registered class information from COM processes.
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
        [OleViewDotNet.Database.COMRegistry]$Database,
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
        | select -ExpandProperty Classes | Where-Object {$_.Context -eq 0 -or $_.Context -match "LOCAL_SERVER"} | Write-Output
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
.PARAMETER Process
Format the IPIDs of a COM process.
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
    [CmdletBinding(DefaultParameterSetName="FromProxy")]
    Param(
        [parameter(Mandatory, Position=0, ValueFromPipeline, ParameterSetName="FromProxy")]
        [OleViewDotNet.IProxyFormatter]$Proxy,
        [parameter(Mandatory, Position=0, ValueFromPipeline, ParameterSetName="FromProcess")]
        [OleViewDotNet.COMProcessEntry]$Process,
        [parameter(Mandatory, Position=0, ValueFromPipeline, ParameterSetName="FromInterface")]
        [OleViewDotNet.Database.COMInterfaceEntry]$Interface,
        [parameter(Mandatory, Position=0, ValueFromPipeline, ParameterSetName = "FromInterfaceInstance")]
        [OleViewDotNet.Database.COMInterfaceInstance]$InterfaceInstance,
        [OleViewDotNet.ProxyFormatterFlags]$Flags = 0
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromProxy" {
                $Proxy.FormatText($Flags) | Write-Output
            }
            "FromProcess" {
                $Process.Ipids | Format-ComProxy -Flags $Flags
            }
            "FromInterface" {
                $Interface | Get-ComProxy | Format-ComProxy -Flags $Flags
            }
            "FromInterfaceInstance" {
                $InterfaceInstance | Get-ComProxy | Format-ComProxy -Flags $Flags
            }
        }
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
.PARAMETER Source
Specify a source where the typelib came from.
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
        [parameter(Mandatory, ParameterSetName = "FromIid", Position=0)]
        [Guid]$Iid,
        [parameter(Mandatory, ParameterSetName = "FromInterface")]
        [OleViewDotNet.Database.COMInterfaceEntry]$Interface,
        [parameter(Mandatory, ParameterSetName = "FromInterfaceInstance")]
        [OleViewDotNet.Database.COMInterfaceInstance]$InterfaceInstance,
        [Parameter(Mandatory, ParameterSetName = "FromSource")]
        [OleViewDotNet.Database.COMRegistryEntrySource]$Source,
        [OleViewDotNet.Database.COMRegistry]$Database
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
        "FromInterface" {
            Get-ComTypeLib $Interface.Iid -Database $Database | Write-Output
        }
        "FromInterfaceInstance" {
            Get-ComTypeLib $InterfaceInstance.Iid -Database $Database | Write-Output
        }
        "FromSource" {
            Get-ComTypeLib -Database $Database | Where-Object Source -eq $Source | Write-Output
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
        [OleViewDotNet.Database.COMTypeLibVersionEntry]$TypeLib,
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
        [OleViewDotNet.Database.COMTypeLibVersionEntry]$TypeLib,
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

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "All" {
                [OleViewDotNet.COMUtilities]::RuntimeInterfaceMetadata.Values | Write-Output 
            }
            "FromName" {
                Get-ComRuntimeInterface | Where-Object Name -eq $Name | Write-Output
            }
            "FromFullName" {
                Get-ComRuntimeInterface | Where-Object FullName -eq $FullName | Write-Output
            }
            "FromIid" {
                [OleViewDotNet.COMUtilities]::RuntimeInterfaceMetadata[$Iid] | Write-Output
            }
        }
    }
}

<#
.SYNOPSIS
Format COM clients as a DOT graph.
.DESCRIPTION
This cmdlet converts a set of processes with parsed clients into a DOT graph format.
.PARAMETER Process
One or more processes to format.
.PARAMETER RemoveUnknownProcess
Remove unknown processes from the graph.
.PARAMETER IncludePid
Include one or more PIDs as roots of the graph.
.PARAMETER IncludeName
Include one or more process names as roots of the graph.
.PARAMETER Path
Output the graph to a path.
.INPUTS
None
.OUTPUTS
string
.EXAMPLE
Format-ComProcessClient $ps
Format a list of processes.
.EXAMPLE
Get-ComProcess -ParseClients | Format-ComProcessClient
Format a list of processes.
#>
function Format-ComProcessClient {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [OleViewDotNet.COMProcessEntry[]]$Process,
        [int[]]$IncludePid,
        [string[]]$IncludeName,
        [switch]$RemoveUnknownProcess,
        [string]$Path
    )

    BEGIN {
        $builder = [OleViewDotNet.PowerShell.ComClientGraphBuilder]::new($IncludePid, $IncludeName, $RemoveUnknownProcess)
    }

    PROCESS {
        $builder.AddRange($Process)
    }

    END {
        if ("" -ne $Path) {
            $builder.ToString() | Set-Content -Path $Path
        } else {
            $builder.ToString() | Write-Output
        }
    }
}

<#
.SYNOPSIS
Get COM categories from a database.
.DESCRIPTION
This cmdlet gets the COM categories from a database.
.PARAMETER CatId
Specify a CATID to lookup.
.PARAMETER Name
Specify a name to match against the category name.
.PARAMETER Database
The COM database to use.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMCategory
.EXAMPLE
Get-ComCategory
Get all COM categories.
.EXAMPLE
Get-ComCategory -CatId "00000001-0000-0000-C000-000000000046"
Get COM category from a CATID.
.EXAMPLE
Get-ComCategory -Name "Blah"
Get the COM category with the name blah.
#>
function Get-ComCategory {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromCatId")]
        [Guid]$CatId,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [OleViewDotNet.Database.COMRegistry]$Database
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }

    switch($PSCmdlet.ParameterSetName) {
        "All" {
            $Database.ImplementedCategories.Values | Write-Output 
        }
        "FromName" {
            Get-ComCategory | Where-Object Name -eq $Name | Write-Output
        }
        "FromCatId" {
            $Database.ImplementedCategories[$CatId] | Write-Output
        }
    }
}

<#
.SYNOPSIS
Filter COM classes based on whether they support one or more interfaces.
.DESCRIPTION
This cmdlet filters COM classes for one or more supported interfaces. This can take a very long time if the interface list needs to be queried first.
.PARAMETER InputObject
The COM class entry to filter on.
.PARAMETER Iid
Filter on an IID or list of IIDs.
.PARAMETER Name
Filter on a name or list of names.
.PARAMETER NameMatch
Filter on a partial matching name.
.PARAMETER Factory
Filter based on the factory interfaces rather than the main class interfaces.
.PARAMETER Refresh
Specify to force the interfaces to be refreshed.
.PARAMETER NoQuery
Specify to not query for the interfaces at all and only return what's already available.
.PARAMETER Exclude
Specify to return classes which do not implement one of the interfaces.
.PARAMETER NoProgress
Don't show progress for query.
.INPUTS
OleViewDotNet.ICOMClassEntry
.OUTPUTS
OleViewDotNet.ICOMClassEntry
.EXAMPLE
Get-ComClass | Select-ComClassInterface -Name "IDispatch"
Get all COM classes which implement IDispatch.
.EXAMPLE
Get-ComClass | Select-ComClassInterface -NameMatch "Blah"
Get all COM classes which implement an interface containing the name Blah.
.EXAMPLE
Get-ComClass | Select-ComClassInterface -Iid "00020400-0000-0000-c000-000000000046"
Get all COM classes which implement an interface with a specific IID.
.EXAMPLE
Get-ComClass | Select-ComClassInterface -Factory -Iid "00000001-0000-0000-C000-000000000046"
Get all COM classes which implement a factory interface with a specific IID.
#>
function Select-ComClassInterface {
    [CmdletBinding(DefaultParameterSetName = "FromName")]
    Param(
        [Parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [OleViewDotNet.Database.ICOMClassEntry]$InputObject,
        [Parameter(Mandatory, ParameterSetName = "FromIid")]
        [Guid[]]$Iid,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string[]]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromNameMatch")]
        [string]$NameMatch,
        [switch]$Factory,
        [switch]$Refresh,
        [switch]$NoQuery,
        [switch]$Exclude,
        [switch]$NoProgress
    )

    PROCESS {
        $result = $false
        $intfs = Get-ComClassInterface $InputObject -Factory:$Factory -Refresh:$Refresh -NoQuery:$NoQuery -NoProgress:$NoProgress
        $result = $false
        foreach($intf in $intfs) {
            switch($PSCmdlet.ParameterSetName) {
                "FromIid" {
                    $result = $intf.Iid -in $Iid
                }
                "FromName" {
                    $result = $intf.Name -in $Name
                }
                "FromNameMatch" {
                    $result = $intf.Name -match $NameMatch
                }
            }
            if ($result) {
                break
            }
        }

        if ($Exclude) {
            $result = !$result
        }

        if ($result) {
            Write-Output $InputObject
        }
    }
}

<#
.SYNOPSIS
Get Windows Runtime extensions.
.DESCRIPTION
This cmdlet gets the Windows Runtime extensions from a database.
.PARAMETER ContractId
Specify a contract ID to lookup.
.PARAMETER Launch
Only show Windows.Launch extensions.
.PARAMETER Protocol
Only show Windows.Protocol extensions. Select out the Protocol property to see the name.
.PARAMETER BackgroundTasks
Only show Windows.BackgroundTasks extensions.
.PARAMETER Database
The COM database to use.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMRuntimeExtensionEntry
.EXAMPLE
Get-ComRuntimeExtension
Get all Windows Runtime extensions.
.EXAMPLE
Get-ComRuntimeExtension -ContractId "Windows.Protocol"
Get Windows Runtime extensions with contract ID of Windows.Protocol.
.EXAMPLE
Get-ComRuntimeExtension -Launch
Get Windows Runtime extensions with contract ID of Windows.Launch.
.EXAMPLE
Get-ComRuntimeExtension -Protocol
Get Windows Runtime extensions with contract ID of Windows.Protocol.
.EXAMPLE
Get-ComRuntimeExtension -BackgroundTasks
Get Windows Runtime extensions with contract ID of Windows.BackgroundTasks.
#>
function Get-ComRuntimeExtension {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromCategoryId")]
        [string]$ContractId,
        [Parameter(Mandatory, ParameterSetName = "FromLaunch")]
        [switch]$Launch,
        [Parameter(Mandatory, ParameterSetName = "FromProtocol")]
        [switch]$Protocol,
        [Parameter(Mandatory, ParameterSetName = "FromBackgroundTasks")]
        [switch]$BackgroundTasks,
        [OleViewDotNet.Database.COMRegistry]$Database
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }

    switch($PSCmdlet.ParameterSetName) {
        "All" {
            $Database.RuntimeExtensions | Write-Output 
        }
        "FromCategoryId" {
            $Database.RuntimeExtensionsByContractId[$ContractId] | Write-Output
        }
        "FromLaunch" {
            $Database.RuntimeExtensionsByContractId["Windows.Launch"] | Write-Output
        }
        "FromProtocol" {
            $Database.RuntimeExtensionsByContractId["Windows.Protocol"] | Write-Output
        }
        "FromBackgroundTasks" {
            $Database.RuntimeExtensionsByContractId["Windows.BackgroundTasks"] | Write-Output
        }
    }
}

<#
.SYNOPSIS
Start a Windows Runtime extension.
.DESCRIPTION
This cmdlet starts a Windows Runtime extension and returns a reference to the COM interface.
.PARAMETER ContractId
Specify a contract ID to start.
.PARAMETER PackageId
Specify a package ID to start.
.PARAMETER AppId
Specify an AppId to start.
.PARAMETER Extension
Specify an extension to start.
.PARAMETER UseExistingHostId
Use an existing host ID for the activation. A hosted application in the same package must be running.
.PARAMETER HostId
Specify the host ID to use for the activation.
.PARAMETER NoWrapper
Don't wrap object in a callable wrapper.
.INPUTS
OleViewDotNet.COMRuntimeExtensionEntry
.OUTPUTS
COM object instance.
.EXAMPLE
Start-ComRuntimeExtension $Extension
Start a runtime extension from an extension object.
.EXAMPLE
Get-ComRuntimeExtension -ContractId "Windows.Protocol" -PackageId "Pkg_1.0.0.0_xxxx" -AppId App
Start a runtime extension from parts.
.EXAMPLE
Start-ComRuntimeExtension $Extension -UseExistingHostId
Start a runtime extension from an extension object using an existing host ID.
.EXAMPLE
Start-ComRuntimeExtension $Extension -HostId 1234
Start a runtime extension from an extension object using an specific host ID.
#>
function Start-ComRuntimeExtension {
    [CmdletBinding()]
    Param(
        [parameter(Mandatory, Position = 0, ParameterSetName = "FromParts")]
        [string]$ContractId,
        [parameter(Mandatory, Position = 1, ParameterSetName = "FromParts")]
        [string]$PackageId,
        [parameter(Mandatory, Position = 2, ParameterSetName = "FromParts")]
        [string]$AppId,
        [parameter(Mandatory, Position = 0, ParameterSetName = "FromExtension", ValueFromPipeline)]
        [OleViewDotNet.Database.COMRuntimeExtensionEntry]$Extension,
        [switch]$UseExistingHostId,
        [int64]$HostId,
        [switch]$NoWrapper
    )

    PROCESS {
        try
        {
            switch($PSCmdlet.ParameterSetName) {
                "FromParts" {
                    $act = [OleViewDotNet.RuntimeExtensionActivator]::new($ContractId, 
                            $PackageId, $AppId)
                }
                "FromExtension" {
                    $act = [OleViewDotNet.RuntimeExtensionActivator]::new($Extension)
                }
            }
            
            if ($UseExistingHostId) {
                $act.UseExistingHostId()
            } else {
                $act.HostId = $HostId
            }
            $obj = $act.Activate()
            if ($null -ne $obj) {
                $type = [OleViewDotNet.IUnknown]
                Wrap-ComObject $obj -Type $type -NoWrapper:$NoWrapper | Write-Output
            }
        }
        catch
        {
            Write-Error $_
        }
    }
}

<#
.SYNOPSIS
Get COM MIME types.
.DESCRIPTION
This cmdlet gets the COM mime types from a database.
.PARAMETER MimeType
Specify the name of a MIME type to lookup.
.PARAMETER Extension
Specify the file extension of a MIME type to lookup, can have a period or not.
.PARAMETER Database
The COM database to use.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMMimeType
.EXAMPLE
Get-ComMimeType
Get all COM MIME types from the current database.
.EXAMPLE
Get-ComMimeType -MimeType "application/xml"
Get the COM MIME type with the name application/xml.
.EXAMPLE
Get-ComMimeType -Extension xml
Get the COM MIME type with the extension xml.
#>
function Get-ComMimeType {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [Parameter(Mandatory, ParameterSetName = "FromMimeType")]
        [string]$MimeType,
        [Parameter(Mandatory, ParameterSetName = "FromExtension")]
        [string]$Extension,
        [OleViewDotNet.Database.COMRegistry]$Database
    )

    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }

    switch($PSCmdlet.ParameterSetName) {
        "All" {
            $Database.MimeTypes | Write-Output 
        }
        "FromMimeType" {
            $Database.MimeTypes | Where-Object MimeType -eq $MimeType | Write-Output
        }
        "FromExtension" {
            if (!$Extension.StartsWith(".")) {
                $Extension = "." + $Extension
            }
            $Database.MimeTypes | Where-Object Extension -eq $Extension | Write-Output
        }
    }
}

<#
.SYNOPSIS
Get COM ProgIDs from a database.
.DESCRIPTION
This cmdlet gets COM ProgIDs from the database based on a set of criteria. The default is to return all registered ProgIds.
.PARAMETER Database
The database to use.
.PARAMETER ProgId
Specify a ProgID to lookup.
.PARAMETER Name
Specify a name to match against the ProgId name.
.PARAMETER Source
Specify a source where the ProgID came from.
.INPUTS
None
.OUTPUTS
OleViewDotNet.COMProgIDEntry
.EXAMPLE
Get-ComProgId
Get all COM ProgIds from the current database.
#>
function Get-ComProgId {
    [CmdletBinding(DefaultParameterSetName = "All")]
    Param(
        [OleViewDotNet.Database.COMRegistry]$Database,
        [Parameter(Position = 0, Mandatory, ParameterSetName = "FromProgId")]
        [string]$ProgId,
        [Parameter(Mandatory, ParameterSetName = "FromName")]
        [string]$Name,
        [Parameter(Mandatory, ParameterSetName = "FromSource")]
        [OleViewDotNet.Database.COMRegistryEntrySource]$Source
    )
    $Database = Get-CurrentComDatabase $Database
    if ($null -eq $Database) {
        Write-Error "No database specified and current database isn't set"
        return
    }
    switch($PSCmdlet.ParameterSetName) {
        "All" {
            Write-Output $Database.Progids.Values
        }
        "FromProgId" {
            Write-Output $Database.Progids[$ProgId]
        }
        "FromName" {
            Get-ComProgId -Database $Database | Where-Object Name -Match $Name | Write-Output
        }
        "FromSource" {
            Get-ComProgId -Database $Database | Where-Object Source -eq $Source | Write-Output
        }
    }
}

<#
.SYNOPSIS
Converts a type library or proxy instance to a .NET assembly.
.DESCRIPTION
This cmdlet converts a type library or proxy instance object to a .NET assembly. This assembly can then be used to 
access COM interfaces. The assembly will be automatically registered with the applications so that you can use it with
Get-ComObjectInterface.
.PARAMETER TypeLib
The type library version entry to convert.
.PARAMETER Path
The path to a type library to convert.
.PARAMETER NoProgress
Don't show progress during conversion.
.PARAMETER Proxy
The proxy instance to convert.
.PARAMETER Ipid
The IPID entry to convert. Must have been created with ParseStubMethods parameter.
.INPUTS
None
.OUTPUTS
System.Reflection.Assembly
#>
function ConvertTo-ComAssembly {
    [CmdletBinding(DefaultParameterSetName="FromTypeLib")]
    Param(
        [parameter(Mandatory, ParameterSetName = "FromTypeLib", Position = 0)]
        [OleViewDotNet.Database.COMTypeLibVersionEntry]$TypeLib,
        [parameter(Mandatory, ParameterSetName = "FromPath", Position = 0)]
        [string]$Path,
        [parameter(Mandatory, ParameterSetName = "FromProxy", Position = 0)]
        [OleViewDotNet.COMProxyInstance]$Proxy,
        [parameter(Mandatory, ParameterSetName = "FromProxyInterface", Position = 0)]
        [OleViewDotNet.COMProxyInterfaceInstance]$ProxyInterface,
        [parameter(Mandatory, ParameterSetName = "FromIpid", Position = 0)]
        [OleViewDotNet.COMIPIDEntry]$Ipid,
        [switch]$NoProgress
    )

    PROCESS {
        $callback = New-CallbackProgress -Activity "Converting to Assembly" -NoProgress:$NoProgress
        switch($PSCmdlet.ParameterSetName) {
            "FromTypeLib" {
                Get-ComTypeLibAssembly -TypeLib $TypeLib -NoProgress:$NoProgress | Write-Output
            }
            "FromPath" {
                Get-ComTypeLibAssembly -Path $TypeLib -NoProgress:$NoProgress | Write-Output
            }
            "FromProxy" {
                [OleViewDotNet.COMUtilities]::ConvertProxyToAssembly($Proxy, $callback) | Write-Output
            }
            "FromProxyInterface" {
                [OleViewDotNet.COMUtilities]::ConvertProxyToAssembly($ProxyInterface, $callback) | Write-Output
            }
            "FromIpid" {
                [OleViewDotNet.COMUtilities]::ConvertProxyToAssembly($Ipid, $callback) | Write-Output
            }
        }
    }
}

<#
.SYNOPSIS
Converts various input formats into a GUID.
.DESCRIPTION
This cmdlet converts various input formats into a GUID structure.
.PARAMETER ComGuid
Get GUIDs from database objects.
.PARAMETER Bytes
Convert from a 16 byte array.
.PARAMETER Ints
Convert from a 4 integer array.
.INPUTS
None
.OUTPUTS
System.Guid
#>
function Get-ComGuid {
    [CmdletBinding(DefaultParameterSetName="FromComGuid")]
    Param(
        [parameter(Mandatory, ParameterSetName = "FromComGuid", Position = 0, ValueFromPipeline)]
        [OleViewDotNet.IComGuid[]]$ComGuid,
        [parameter(Mandatory, ParameterSetName = "FromBytes")]
        [byte[]]$Bytes,
        [parameter(Mandatory, ParameterSetName = "FromInts")]
        [int[]]$Ints
    )

    PROCESS {
        switch($PSCmdlet.ParameterSetName) {
            "FromBytes" {
                [guid]::new($Bytes) | Write-Output
            }
            "FromInts" {
                [OleViewDotNet.PowerShell.PowerShellUtils]::GuidFromInts($Ints) | Write-Output
            }
            "FromComGuid" {
                foreach($obj in $Object) {
                    $Object.ComGuid | Write-Output
                }
            }
        }
    }
}

<#
.SYNOPSIS
Tests if an object supports an interface.
.DESCRIPTION
This cmdlet queries a COM object for a specified interface.
.PARAMETER Iid
The IID of the interface.
.PARAMETER Interface
A COM interface entry.
.PARAMETER Object
The object to query.
.INPUTS
None
.OUTPUTS
Boolean
#>
function Test-ComInterface {
    [CmdletBinding(DefaultParameterSetName="FromIid")]
    Param(
        [parameter(Mandatory, ParameterSetName = "FromIid", Position = 0)]
        [Guid]$Iid,
        [parameter(Mandatory, Position=1)]
        [object]$Object,
        [parameter(Mandatory, ParameterSetName = "FromIntf", Position = 0)]
        [OleViewDotNet.Database.COMInterfaceEntry]$Interface
    )

    if($PSCmdlet.ParameterSetName -eq "FromIid") {
        $Interface = Get-ComInterface -Iid $Iid -AllowNoReg
    }

    $Object = Unwrap-ComObject -Object $Object

    return $Interface.TestInterface($Object)
}
