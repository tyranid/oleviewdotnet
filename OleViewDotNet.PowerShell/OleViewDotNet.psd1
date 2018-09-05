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

@{

# Script module or binary module file associated with this manifest.
RootModule = 'OleViewDotNet.psm1'

# Version number of this module.
ModuleVersion = '1.5'

# Supported PSEditions
# CompatiblePSEditions = @()

# ID used to uniquely identify this module
GUID = '0d20a7ea-f6b0-4720-bf06-d422db254e29'

# Author of this module
Author = 'James Forshaw'

# Company or vendor of this module
CompanyName = 'None'

# Copyright statement for this module
Copyright = 'James Forshaw (c) 2018'

# Description of the functionality provided by this module
Description = 'PowerShell module for OleViewDotNet'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '3.0'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
DotNetFrameworkVersion = '4.6'

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
RequiredAssemblies = 'OleViewDotNet.Main.dll', 'OleViewDotNet.PowerShell.dll', 'NtApiDotNet.dll'

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = 'OleViewDotNet_Formatters.ps1xml'

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = 'Get-ComDatabase', 'Set-ComDatabase', 'Compare-ComDatabase', 'Get-ComClass', 'Get-ComProcess', 'Start-ComActivationLog', 'Stop-ComActivationLog', `
                    'Get-ComAppId', 'Show-ComDatabase', 'Get-ComClassInterface', 'Get-ComRuntimeClass', 'Get-ComInterface', 'Select-ComAccess', 'Get-ComObjRef',
                    'Show-ComSecurityDescriptor', 'New-ComObject', 'New-ComObjectFactory', 'Get-ComMoniker', 'Get-ComMonikerDisplayName', 'Get-ComProxy',
                    'Get-ComObjectIpid', 'Set-ComSymbolResolver', 'Get-CurrentComDatabase', 'Set-CurrentComDatabase', 'Get-ComRegisteredClass', 'Format-ComProxy',
                    'Get-ComTypeLib', 'Get-ComTypeLibAssembly', 'Format-ComTypeLib', 'Format-ComGuid', 'Set-ComSymbolCache', 'New-ComStorageObject', 'Get-ComStorageObject',
                    'Get-ComRuntimeInterface', 'Get-ComRuntimeServer', 'Format-ComProcessClient', 'Get-ComCategory', 'Select-ComClassInterface'

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @()

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        # Tags = @()

        # A URL to the license for this module.
        # LicenseUri = ''

        # A URL to the main website for this project.
        # ProjectUri = ''

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        # ReleaseNotes = ''

    } # End of PSData hashtable

} # End of PrivateData hashtable

}

