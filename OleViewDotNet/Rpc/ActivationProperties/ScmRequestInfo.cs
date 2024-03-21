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

using NtApiDotNet;
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections;
using System.Text;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ScmRequestInfo : IActivationProperty
{
    private ScmRequestInfoData m_inner;

    private PrivateScmRequestInfo PrivateScmInfoInternal
    {
        get
        {
            PrivateInfo ??= new();
            return PrivateInfo;
        }
    }

    internal ScmRequestInfo(byte[] data)
    {
        data.Deserialize(out m_inner);
        if (m_inner.pScmInfo is not null)
        {
            PrivateInfo = new(m_inner.pScmInfo);
        }
        if (m_inner.remoteRequest is not null)
        {
            RemoteInfo = new(m_inner.remoteRequest);
        }
    }

    public ScmRequestInfo()
    {
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_ScmRequestInfo;

    public PrivateScmRequestInfo PrivateInfo { get; set; }
    public RemoteScmRequestInfo RemoteInfo { get; set; }

    public void SetProcessSignatureFromCurrentProcess()
    {
        long signature = LocalResolverClientHandles.Instance.ProcessSignature;
        if (signature == 0)
            throw new InvalidOperationException("Couldn't get local process signature to update.");
        SetProcessSignature(signature);
    }

    public void SetProcessSignature(long signature)
    {
        PrivateScmInfoInternal.ProcessSignature = signature;
    }

    public void SetEnvironmentBlock(string env_block)
    {
        PrivateScmInfoInternal.EnvBlock = env_block;
    }

    public void SetEnvironmentBlockFromCurrentProcess()
    {
        StringBuilder builder = new();
        var env = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry pair in env)
        {
            builder.Append($"{pair.Key}={pair.Value}\0");
        }
        builder.Append("\0");
        SetEnvironmentBlock(builder.ToString());
    }

    public void SetWinstaDesktop(string winsta)
    {
        PrivateScmInfoInternal.WinstaDesktop = winsta;
    }

    public void SetWinstaDesktopFromCurrentProcess()
    {
        using var winsta = NtWindowStation.Current;
        using var desktop = NtDesktop.Current;

        SetWinstaDesktop($"{winsta.Name}\\{desktop.Name}");
    }

    public void SetPrivateScmInfoFromCurrentProcess()
    {
        SetProcessSignatureFromCurrentProcess();
        SetEnvironmentBlockFromCurrentProcess();
        SetWinstaDesktopFromCurrentProcess();
    }

    public byte[] Serialize()
    {
        m_inner.pScmInfo = PrivateInfo?.ToStruct();
        m_inner.remoteRequest = RemoteInfo?.ToStruct();
        return m_inner.Serialize();
    }
}
