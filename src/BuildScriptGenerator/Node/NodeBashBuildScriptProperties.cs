﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    /// <summary>
    /// Build script template for NodeJs in Bash.
    /// </summary>
    public partial class NodeBashBuildScript
    {
        public NodeBashBuildScript(
            string npmInstallCommand,
            string benvArgs)
        {
            this.NpmInstallCommand = npmInstallCommand;
            this.BenvArgs = benvArgs;
        }

        public string NpmInstallCommand { get; set; }

        public string BenvArgs { get; set; }
    }
}