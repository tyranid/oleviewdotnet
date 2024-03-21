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

using NtApiDotNet;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace OleViewDotNetPS.Utils;

public static class PowerShellUtils
{
    private static NtToken GetProcessAccessToken(NtProcess process)
    {
        return process.OpenToken();
    }

    public static NtToken GetAccessToken(NtToken token, NtProcess process, int process_id)
    {
        if (token != null)
        {
            return token.DuplicateToken(SecurityImpersonationLevel.Identification);
        }
        else if (process != null)
        {
            return GetProcessAccessToken(process);
        }
        else
        {
            using var p = NtProcess.Open(process_id, ProcessAccessRights.QueryLimitedInformation);
            return GetProcessAccessToken(p);
        }
    }

    public static COMAccessCheck GetAccessCheck(
        COMAccessToken token,
        COMSid principal,
        COMAccessRights access_rights,
        COMAccessRights launch_rights,
        bool ignore_default)
    {
        return new COMAccessCheck(
            token,
            principal,
            access_rights,
            launch_rights,
            ignore_default);
    }

    public static COMAccessCheck GetAccessCheck(int process_id,
        COMSid principal,
        COMAccessRights access_rights,
        COMAccessRights launch_rights,
        bool ignore_default)
    {
        using var token = COMAccessToken.FromProcess(process_id);
        return new COMAccessCheck(
                token,
                principal,
                access_rights,
                launch_rights,
                ignore_default);
    }

    public static Type GetFactoryType(ICOMClassEntry cls)
    {
        if (cls is COMRuntimeClassEntry)
        {
            return typeof(IActivationFactory);
        }
        return typeof(IClassFactory);
    }

    public static Guid GuidFromInts(int[] ints)
    {
        if (ints.Length != 4)
        {
            throw new ArgumentException("Must provide 4 integers to convert to a GUID");
        }

        byte[] bytes = new byte[16];
        Buffer.BlockCopy(ints, 0, bytes, 0, 16);
        return new Guid(bytes);
    }

    private static void FormatAce(StringBuilder builder, Ace ace, bool sdk_name)
    {
        string mask_str;
        string access_name = "Access";
        if (ace is MandatoryLabelAce)
        {
            mask_str = NtSecurity.AccessMaskToString(ace.Mask.ToMandatoryLabelPolicy(), sdk_name);
            access_name = "Policy";
        }
        else
        {
            mask_str = NtSecurity.AccessMaskToString(ace.Mask.ToSpecificAccess<COMAccessRights>(), sdk_name);
        }

        builder.AppendLine($" - Type  : {(sdk_name ? NtSecurity.AceTypeToSDKName(ace.Type) : ace.Type.ToString())}");
        builder.AppendLine($" - Name  : {ace.Sid.Name}");
        builder.AppendLine($" - Sid   : {ace.Sid}");
        builder.AppendLine($" - Mask  : {ace.Mask:X08}");
        builder.AppendLine($" - {access_name}: {mask_str}");
        builder.AppendLine($" - Flags : {(sdk_name ? NtSecurity.AceFlagsToSDKName(ace.Flags) : ace.Flags.ToString())}");
        if (ace.IsConditionalAce)
        {
            builder.AppendLine($" - Condition: {ace.Condition}");
        }
        builder.AppendLine();
    }

    private static void FormatAcl(StringBuilder builder, Acl acl, string name, bool sdk_name)
    {
        List<string> flags = new();
        if (acl.Defaulted)
            flags.Add("Defaulted");
        if (acl.Protected)
            flags.Add("Protected");
        if (acl.AutoInherited)
            flags.Add("Auto Inherited");
        if (acl.AutoInheritReq)
            flags.Add("Auth Inherit Requested");
        if (acl.Count > 0)
            builder.AppendLine($"{name} {string.Join(", ", flags)}");
        else
            builder.AppendLine(name);

        if (acl.NullAcl)
        {
            builder.AppendLine(" - <NULL ACL>");
            builder.AppendLine();
        }
        else if (acl.Count == 0)
        {
            builder.AppendLine(" - <EMPTY ACL>");
            builder.AppendLine();
        }
        else
        {
            foreach (var ace in acl)
            {
                FormatAce(builder, ace, sdk_name);
            }
        }
    }

    public static string FormatSecurityDescriptor(COMSecurityDescriptor desc, bool sdk_name = false)
    {
        if (desc is null)
            return string.Empty;

        var sd = desc.SecurityDescriptor;

        StringBuilder builder = new();

        builder.AppendLine($"Control: {(sdk_name ? NtSecurity.ControlFlagsToSDKName(sd.Control) : sd.Control.ToString())}");
        if (sd.Owner is null && sd.Group is null && sd.Dacl is null && sd.Sacl is null)
        {
            builder.AppendLine("<NO SECURITY INFORMATION>");
        }

        if (sd.Owner != null)
        {
            builder.AppendLine(sd.Owner.Defaulted ? "<Owner> (Defaulted)" : "<Owner>");
            builder.AppendLine($" - Name   : {sd.Owner.Sid.Name}");
            builder.AppendLine($" - Sid    : {sd.Owner.Sid}");
            builder.AppendLine();
        }

        if (sd.Group != null)
        {
            builder.AppendLine(sd.Group.Defaulted ? "<Group> (Defaulted)" : "<Group>");
            builder.AppendLine($" - Name   : {sd.Group.Sid.Name}");
            builder.AppendLine($" - Sid    : {sd.Group.Sid}");
            builder.AppendLine();
        }

        if (sd.DaclPresent)
        {
            FormatAcl(builder, sd.Dacl, "<DACL>", sdk_name);
        }

        if (sd.SaclPresent && sd.SaclAceCount > 0)
        {
            FormatAcl(builder, sd.Sacl, "<SACL>", sdk_name);
        }

        return builder.ToString();
    }
}
