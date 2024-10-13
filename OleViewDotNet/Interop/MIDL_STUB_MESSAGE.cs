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

using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct MIDL_STUB_MESSAGE
{
    public IntPtr RpcMsg;
    public IntPtr Buffer;
    public IntPtr BufferStart;
    public IntPtr BufferEnd;
    public IntPtr BufferMark;
    public int BufferLength;
    public int MemorySize;
    public IntPtr Memory;
    public byte IsClient;
    public byte Pad;
    public ushort uFlags2;
    public int ReuseBuffer;
    public IntPtr pAllocAllNodesContext;
    public IntPtr pPointerQueueState;
    public int IgnoreEmbeddedPointers;
    public IntPtr PointerBufferMark;
    public byte CorrDespIncrement;
    public byte uFlags;
    public ushort UniquePtrCount;
    public IntPtr MaxCount;
    public int Offset;
    public int ActualCount;
    public IntPtr pfnAllocate;
    public IntPtr pfnFree;
    public IntPtr StackTop;
    public IntPtr pPresentedType;
    public IntPtr pTransmitType;
    public IntPtr SavedHandle;
    public IntPtr StubDesc;
    public IntPtr FullPtrXlatTables;
    public int FullPtrRefId;
    public int PointerLength;
    public int Flags;
    /*
    int fInDontFree       :1;
    int fDontCallFreeInst :1;
    int fUnused1          :1;
    int fHasReturn        :1;
    int fHasExtensions    :1;
    int fHasNewCorrDesc   :1;
    int fIsIn             :1;
    int fIsOut            :1;
    int fIsOicf           :1;
    int fBufferValid      :1;
    int fHasMemoryValidateCallback: 1;
    int fInFree             :1;
    int fNeedMCCP         :1;
    int fUnused2          :3;
    int fUnused3          :16;
    */

    public int dwDestContext;
    public IntPtr pvDestContext;
    public IntPtr SavedContextHandles;
    public int ParamNumber;
    public IntPtr pRpcChannelBuffer;
    public IntPtr pArrayInfo;
    public IntPtr SizePtrCountArray;
    public IntPtr SizePtrOffsetArray;
    public IntPtr SizePtrLengthArray;
    public IntPtr pArgQueue;
    public int dwStubPhase;
    public IntPtr LowStackMark;
    public IntPtr pAsyncMsg;
    public IntPtr pCorrInfo;
    public IntPtr pCorrMemory;
    public IntPtr pMemoryList;
    public IntPtr pCSInfo;
    public IntPtr ConformanceMark;
    public IntPtr VarianceMark;
    public IntPtr Unused;
    public IntPtr pContext;
    public IntPtr ContextHandleHash;
    public IntPtr pUserMarshalList;
    public IntPtr Reserved51_3;
    public IntPtr Reserved51_4;
    public IntPtr Reserved51_5;
}