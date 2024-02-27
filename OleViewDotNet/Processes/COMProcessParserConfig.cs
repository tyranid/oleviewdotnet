//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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

namespace OleViewDotNet.Processes;

public struct COMProcessParserConfig
{
    public bool ParseStubMethods;
    public bool ResolveMethodNames;
    public bool ParseRegisteredClasses;
    public bool ParseClients;
    public bool ParseActivationContext;

    public static COMProcessParserConfig Default
    {
        get
        {
            return new()
            {
                ParseStubMethods = ProgramSettings.ParseStubMethods,
                ResolveMethodNames = ProgramSettings.ResolveMethodNames,
                ParseRegisteredClasses = ProgramSettings.ParseRegisteredClasses,
                ParseClients = ProgramSettings.ParseClients,
                ParseActivationContext = ProgramSettings.ParseActivationContext,
            };
        }
    }
}
