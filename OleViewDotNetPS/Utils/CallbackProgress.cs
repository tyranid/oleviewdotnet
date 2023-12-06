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

using System;

namespace OleViewDotNetPS.Utils;

/// <summary>
/// Class to make it easier for PowerShell to implement a IProgress callback.
/// </summary>
public class CallbackProgress : IProgress<Tuple<string, int>>
{
    private readonly string _activity;
    private readonly Action<string, string, int> _callback;

    public CallbackProgress(string activity, Action<string, string, int> callback)
    {
        _callback = callback;
        _activity = activity;
    }

    void IProgress<Tuple<string, int>>.Report(Tuple<string, int> value)
    {
        _callback(_activity, value.Item1, value.Item2);
    }
}
