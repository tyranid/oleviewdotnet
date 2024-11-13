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
using OleViewDotNet.Utilities;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Rpc.Transport;

internal sealed class RpcChannelBufferStub : RpcChannelBuffer, IRpcChannelBuffer
{
    private IRpcStubBuffer m_stub;

    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    private class AggregableObject : ICustomQueryInterface
    {
        private readonly Guid m_iid;
        public AggregableObject(Guid iid)
        {
            m_iid = iid;
        }

        CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(ref Guid iid, out IntPtr ppv)
        {
            if (iid == m_iid)
            {
                ppv = Marshal.GetIUnknownForObject(new());
                return CustomQueryInterfaceResult.Handled;
            }
            ppv = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
        }
    }

    void IRpcChannelBuffer.GetBuffer(ref RPCOLEMESSAGE pMessage, in Guid riid)
    {
        pMessage.Buffer = Marshal.AllocHGlobal(pMessage.cbBuffer);
    }

    void IRpcChannelBuffer.SendReceive(ref RPCOLEMESSAGE pMessage, out int pStatus)
    {
        throw new NotImplementedException();
    }

    void IRpcChannelBuffer.FreeBuffer(ref RPCOLEMESSAGE pMessage)
    {
        if (pMessage.Buffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(pMessage.Buffer);
            pMessage.Buffer = IntPtr.Zero;
            pMessage.cbBuffer = 0;
        }
    }

    void IRpcChannelBuffer.GetDestCtx(out int pdwDestContext, out IntPtr ppvDestContext)
    {
        pdwDestContext = (int)MSHCTX.LOCAL;
        ppvDestContext = IntPtr.Zero;
    }

    int IRpcChannelBuffer.IsConnected()
    {
        return IsConnected ? 1 : 0;
    }

    public override bool IsConnected => m_stub is not null;

    public override MSHCTX DestContext => MSHCTX.LOCAL;

    public override byte[] SendReceive(byte[] ndr_data, int proc_num)
    {
        RPCOLEMESSAGE msg = new();
        msg.iMethod = proc_num;
        msg.cbBuffer = ndr_data.Length;
        msg.dataRepresentation = 0x10;
        IRpcChannelBuffer buffer = this;
        buffer.GetBuffer(ref msg, COMKnownGuids.IID_IUnknown);
        Marshal.Copy(ndr_data, 0, msg.Buffer, ndr_data.Length);
        m_stub.Invoke(ref msg, buffer).CheckHr();
        byte[] ret = new byte[msg.cbBuffer];
        Marshal.Copy(msg.Buffer, ret, 0, ret.Length);
        buffer.FreeBuffer(ref msg);
        return ret;
    }

    protected override void OnDispose()
    {
        m_stub?.Disconnect();
        m_stub = null;
    }

    public RpcChannelBufferStub(object obj, Guid iid)
    {
        NativeMethods.CoGetPSClsid(iid, out Guid clsid).CheckHr();
        IPSFactoryBuffer ps = (IPSFactoryBuffer)COMUtilities.CreateClassFactory(clsid,
            typeof(IPSFactoryBuffer).GUID, CLSCTX.INPROC_SERVER, null);
        ps.CreateStub(iid, new AggregableObject(iid), out IRpcStubBuffer stub).CheckHr();
        m_stub = stub;
        m_stub.Connect(obj).CheckHr();
    }
}
