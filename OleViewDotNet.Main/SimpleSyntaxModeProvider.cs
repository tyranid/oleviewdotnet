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

using ICSharpCode.TextEditor.Document;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace OleViewDotNet
{
    internal class SimpleSyntaxModeProvider : ISyntaxModeFileProvider
    {
        private List<SyntaxMode> syntaxModes = null;
        private string _syntaxMode;

        public ICollection<SyntaxMode> SyntaxModes
        {
            get
            {
                return syntaxModes;
            }
        }

        public SimpleSyntaxModeProvider(string filename, string name, string extensions, string syntaxMode)
        {
            syntaxModes = new List<SyntaxMode>();
            _syntaxMode = syntaxMode;
            syntaxModes.Add(new SyntaxMode(filename, name, extensions));
        }

        public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
        {
            return new XmlTextReader(new StringReader(_syntaxMode));
        }

        public void UpdateSyntaxModeList()
        {
            // resources don't change during runtime
        }
    }
}
