//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OleViewDotNet
{
    public enum FilterOperation
    {
        Include,
        Exclude
    }

    public enum FilterType
    {
        Clsid,
        AppId,
        Category,
        Interface,
        ProgID,
        TypeLib,
        LowRights,
        MimeType,
        Server,
    }

    public enum FilterComparison
    {
        Contains,
        Excludes,
        Equals,
        NotEquals,
        StartsWith,
        EndsWith,
    }

    public class RegistryViewerFilter
    {
        public FilterOperation Operation { get; set; }
        public FilterType Type { get; set; }
        public FilterComparison Comparison { get; set; }
    }
}
