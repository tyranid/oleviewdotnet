//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2024
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
using System;

namespace OleViewDotNet.Rpc.Transport;

public abstract class RpcChannelBuffer : IDisposable
{
    protected abstract void OnDispose();

    public void Dispose()
    {
        OnDispose();
    }

    public abstract byte[] SendReceive(byte[] ndr_data, int proc_num);

    public abstract bool IsConnected { get; }

    public abstract MSHCTX DestContext { get; }

    public static RpcChannelBuffer FromObject(object obj, Guid iid)
    {
        using SafeComObjectHandle proxy = SafeComObjectHandle.FromObject(obj, iid);
        if (!proxy.IsProxy())
        {
            return new RpcChannelBufferStub(obj, iid);
        }

        MIDL_STUB_MESSAGE stub_message = new();
        NativeMethods.NdrProxyInitialize(proxy, new(), ref stub_message, new(), 0);
        try
        {
            return new RpcChannelBufferProxy(stub_message.pRpcChannelBuffer, iid, proxy);
        }
        finally
        {
            NativeMethods.NdrProxyFreeBuffer(proxy, ref stub_message);
        }
    }
}
