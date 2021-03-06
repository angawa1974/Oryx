// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpVersionProvider : IPhpVersionProvider
    {
        private PlatformVersionInfo _versionInfo;

        public PlatformVersionInfo GetVersionInfo()
        {
            if (_versionInfo == null)
            {
                var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PhpConstants.InstalledPhpVersionsDir);
                _versionInfo = PlatformVersionInfo.CreateOnDiskVersionInfo(
                    installedVersions,
                    PhpConstants.DefaultPhpRuntimeVersion);
            }

            return _versionInfo;
        }
    }
}