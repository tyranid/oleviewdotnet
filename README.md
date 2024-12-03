# OleView.NET

OleView.NET is a .NET 4 application to provide a tool which merges the classic SDK tools
OleView and Test Container into one application. It allows you to find COM objects through
a number of different views (e.g. by CLSID, by ProgID, by server executable), enumerate
interfaces on the object and then create an instance and invoke methods. It also has a basic
container to attack ActiveX objects to so you can see the display output while manipulating
the data.

It comes with a both a UI and a PowerShell module. The PowerShell module must run in the
last version of PowerShell 5, it does not work correctly in PowerShell 7+ as the latest
versions of .NET have removed certain COM interop features that the project relies upon.

## Building

To correctly build from source you'll need to pull the repository and its submodules. You
can do this using the command:

`git clone --recurse-submodules https://github.com/tyranid/oleviewdotnet.git`

If you don't want to build the tools you can also get the latest published release from the
PowerShell Gallery, [here](https://www.powershellgallery.com/packages/OleViewDotNet). Or by
running the PowerShell command:

`Install-Module OleViewDotNet`

The PowerShell module also includes the UI executable, which you can run directly or by using the `Show-ComDatabase` command in PowerShell.

 
## Disclaimer
_All work (barring the shameless borrowing of the icon) copyright James Forshaw (c) 2014-2024_
