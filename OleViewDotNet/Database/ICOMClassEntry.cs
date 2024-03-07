//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OleViewDotNet.Database;

public interface ICOMClassEntry
{
    /// <summary>
    /// The name of the class entry.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// The clsid of the class entry.
    /// </summary>
    Guid Clsid { get; }
    /// <summary>
    /// The default server name.
    /// </summary>
    string DefaultServer { get; }
    /// <summary>
    /// Default type of the server.
    /// </summary>
    COMServerType DefaultServerType { get; }

    /// <summary>
    /// Get list of supported Interface IIDs (that we know about)
    /// NOTE: This will load the object itself to check what is supported, it _might_ crash the app
    /// The returned array is cached so subsequent calls to this function return without calling into COM
    /// </summary>
    /// <param name="refresh">Force the supported interface list to refresh</param>
    /// <param name="token">Token to use when querying for the interfaces.</param>
    /// <returns>Returns true if supported interfaces were refreshed.</returns>
    /// <exception cref="Win32Exception">Thrown on error.</exception>
    Task<bool> LoadSupportedInterfacesAsync(bool refresh, COMAccessToken token);

    /// <summary>
    /// Get list of supported Interface IIDs Synchronously
    /// </summary>
    /// <param name="refresh">Force the supported interface list to refresh</param>
    /// <param name="token">Token to use when querying for the interfaces.</param>
    /// <returns>Returns true if supported interfaces were refreshed.</returns>
    /// <exception cref="Win32Exception">Thrown on error.</exception>
    bool LoadSupportedInterfaces(bool refresh, COMAccessToken token);

    /// <summary>
    /// Indicates that the class' interface list has been loaded.
    /// </summary>
    bool InterfacesLoaded {  get; }

    /// <summary>
    /// Get list of interfaces.
    /// </summary>
    /// <remarks>You must have called LoadSupportedInterfaces before this call to get any useful output.</remarks>
    IEnumerable<COMInterfaceInstance> Interfaces { get; }

    /// <summary>
    /// Get list of factory interfaces.
    /// </summary>
    /// <remarks>You must have called LoadSupportedFactoryInterfaces before this call to get any useful output.</remarks>
    IEnumerable<COMInterfaceInstance> FactoryInterfaces { get; }

    /// <summary>
    /// Create an instance of this object.
    /// </summary>
    /// <param name="dwContext">The class context.</param>
    /// <param name="server">The remote server address.</param>
    /// <param name="auth_info">Authentication information for the remote connection.</param>
    /// <returns>The instance of the object.</returns>
    object CreateInstanceAsObject(CLSCTX dwContext, string server, COMAuthInfo auth_info = null);

    /// <summary>
    /// Create an instance of this object's class factory.
    /// </summary>
    /// <param name="dwContext">The class context.</param>
    /// <param name="server">The remote server address.</param>
    /// <param name="auth_info">Authentication information for the remote connection.</param>
    /// <returns>The instance of the object.</returns>
    object CreateClassFactory(CLSCTX dwContext, string server, COMAuthInfo auth_info = null);

    /// <summary>
    /// True if this class supports remote activation.
    /// </summary>
    bool SupportsRemoteActivation { get; }
}
