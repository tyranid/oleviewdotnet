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

using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class PrivateScmRequestInfo
{
    private CustomPrivScmInfo m_inner;

    public int Apartment
    {
        get => m_inner.Apartment;
        set => m_inner.Apartment = value;
    }

    public string WinstaDesktop
    {
        get => m_inner.pwszWinstaDesktop;
        set => m_inner.pwszWinstaDesktop = value;
    }

    public long ProcessSignature
    {
        get => m_inner.ProcessSignature;
        set => m_inner.ProcessSignature = value;
    }

    public string EnvBlock
    {
        get
        {
            if (m_inner.pEnvBlock is null)
                return null;
            return new string(m_inner.pEnvBlock);
        }

        set
        {
            if (value is null)
            {
                m_inner.pEnvBlock = null;
                m_inner.EnvBlockLength = 0;
            }
            else
            {
                char[] buffer = value.ToCharArray();
                m_inner.EnvBlockLength = buffer.Length;
                m_inner.pEnvBlock = buffer;
            }
        }
    }

    internal PrivateScmRequestInfo(CustomPrivScmInfo inner)
    {
        m_inner = inner;
    }

    public PrivateScmRequestInfo() : this(default)
    {
    }

    internal CustomPrivScmInfo ToStruct()
    {
        return m_inner;
    }
}
