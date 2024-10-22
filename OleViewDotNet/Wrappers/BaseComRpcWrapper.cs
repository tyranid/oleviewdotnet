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

using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Database;
using OleViewDotNet.Rpc.Transport;
using System;

namespace OleViewDotNet.Wrappers;

public abstract class BaseComRpcWrapper<T> : BaseComWrapper, IDisposable where T : RpcClientBase, new()
{
    protected readonly T _object;

    private BaseComRpcWrapper(T client, object obj) : base(client.InterfaceId, typeof(T).Name)
    {
        _object = client;
        RpcCOMClientTransportFactory.SetupFactory();
        RpcChannelBufferClientTransportConfiguration config = new() { Instance = obj };
        client.Connect($"{RpcCOMClientTransportFactory.COMBufferProtocol}:[proxy]", new RpcTransportSecurity() { Configuration = config });
    }

    protected BaseComRpcWrapper(object obj) : this(new T(), obj)
    {
    }

    internal override void SetDatabase(COMRegistry database)
    {
        base.SetDatabase(database);
        if (_object.Transport is RpcChannelBufferClientTransport transport)
        {
            transport.SetDatabase(database);
        }
    }

    public override object Unwrap()
    {
        if (_object.Transport is not RpcChannelBufferClientTransport transport)
        {
            return this;
        }
        return transport.GetObject();
    }

    void IDisposable.Dispose()
    {
        _object.Dispose();
    }
}