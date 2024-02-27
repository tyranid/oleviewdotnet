//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

namespace OleViewDotNet.Security;

public enum TokenLogonType
{
    /// <summary>
    /// This is used to specify an undefined logon type
    /// </summary>
    UndefinedLogonType = 0,
    /// <summary>
    /// Interactively logged on (locally or remotely)
    /// </summary>
    Interactive = 2,
    /// <summary>
    /// Accessing system via network
    /// </summary>
    Network,
    /// <summary>
    /// Started via a batch queue
    /// </summary>
    Batch,
    /// <summary>
    /// Service started by service controller
    /// </summary>
    Service,
    /// <summary>
    /// Proxy logon
    /// </summary>
    Proxy,
    /// <summary>
    /// Unlock workstation
    /// </summary>
    Unlock,
    /// <summary>
    /// Network logon with cleartext credentials
    /// </summary>
    NetworkCleartext,
    /// <summary>
    /// Clone caller, new default credentials
    /// </summary>
    NewCredentials,
    /// <summary>
    /// Remove interactive.
    /// </summary>
    RemoteInteractive,
    /// <summary>
    /// Cached Interactive.
    /// </summary>
    CachedInteractive,
    /// <summary>
    /// Cached Remote Interactive.
    /// </summary>
    CachedRemoteInteractive,
    /// <summary>
    /// Cached unlock.
    /// </summary>
    CachedUnlock
}
