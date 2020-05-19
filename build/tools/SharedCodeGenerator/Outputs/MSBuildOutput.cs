// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    [OutputType("msbuild")]
    internal class MSBuildOutput : IOutputFile
    {
        private ConstantCollection _collection;
        private string _directory;
        private string _fileNamePrefix;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            _collection = constantCollection;
            _directory = typeInfo["directory"];
            _fileNamePrefix = typeInfo["file-name-prefix"];
        }

        public string GetPath()
        {
            var name = _collection.Name.Camelize();
            name = char.ToLowerInvariant(name[0]) + name.Substring(1);
            return Path.Combine(_directory, _fileNamePrefix + name + ".props");
        }

        public string GetContent()
        {
            StringBuilder body = new StringBuilder();
            var autoGeneratedMessage = Program.BuildAutogenDisclaimer(_collection.SourcePath);
            body.AppendLine($"<!-- {autoGeneratedMessage} -->"); // Can't use AppendLine becuase it appends \r\n
            body.AppendLine();
            body.AppendLine("<Project>");
            body.AppendLine("\t<PropertyGroup>");
            foreach (var constant in _collection.Constants)
            {
                var name = constant.Key.Replace(ConstantCollection.NameSeparator[0], '_').ToUpper();
                body.AppendLine($"\t\t<{name}>{constant.Value}</{name}>");
            }

            body.AppendLine("\t</PropertyGroup>");
            body.AppendLine("</Project>");
            return body.ToString();
        }
    }
}
