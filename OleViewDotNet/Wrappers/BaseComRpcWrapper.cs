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
using OleViewDotNet.Database;
using OleViewDotNet.Rpc.Transport;
using System;

namespace OleViewDotNet.Wrappers;

public abstract class BaseComRpcWrapper : BaseComWrapper
{
    protected BaseComRpcWrapper(Guid iid, string name, COMRegistry registry) 
        : base(iid, name, registry)
    {
    }
}

public abstract class BaseComRpcWrapper<T> : BaseComRpcWrapper, IDisposable where T : RpcClientBase, new()
{
    protected readonly T _object;

    private BaseComRpcWrapper(T client, object obj, COMRegistry registry) : base(client.InterfaceId, typeof(T).Name, registry)
    {
        _object = client;
        client.Connect(new RpcChannelBufferClientTransport(obj, client.InterfaceId, registry));
    }

    protected BaseComRpcWrapper(object obj, COMRegistry registry) : this(new T(), obj, registry)
    {
    }

    public override object Unwrap()
    {
        if (_object.Transport is not RpcChannelBufferClientTransport transport)
        {
            return this;
        }
        return transport.GetObject();
    }

    protected override void OnDispose()
    {
        _object.Dispose();
    }
}