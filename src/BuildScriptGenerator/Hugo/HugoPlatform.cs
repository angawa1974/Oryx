﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    class HugoPlatform : IProgrammingPlatform
    {
        private readonly ILogger<HugoPlatform> _logger;
        private readonly HugoPlatformInstaller _platformInstaller;
        private readonly BuildScriptGeneratorOptions _commonOptions;
        private readonly HugoScriptGeneratorOptions _hugoScriptGeneratorOptions;
        private readonly HugoPlatformDetector _detector;

        public HugoPlatform(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IOptions<HugoScriptGeneratorOptions> hugoScriptGeneratorOptions,
            ILogger<HugoPlatform> logger,
            HugoPlatformInstaller platformInstaller,
            HugoPlatformDetector detector)
        {
            _logger = logger;
            _platformInstaller = platformInstaller;
            _commonOptions = commonOptions.Value;
            _hugoScriptGeneratorOptions = hugoScriptGeneratorOptions.Value;
            _detector = detector;
        }

        public string Name => HugoConstants.PlatformName;

        public IEnumerable<string> SupportedVersions => new[] { HugoConstants.DefaultVersion };

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            PlatformDetectorResult detectionResult;
            if (TryGetExplicitVersion(out var explicitVersion))
            {
                detectionResult = new PlatformDetectorResult
                {
                    Platform = HugoConstants.PlatformName,
                    PlatformVersion = explicitVersion,
                };
            }
            else
            {
                detectionResult = _detector.Detect(context);
            }

            if (detectionResult == null)
            {
                return null;
            }

            var version = GetVersionUsingHierarchicalRules(detectionResult.PlatformVersion);
            version = GetMaxSatisfyingVersionAndVerify(version);
            detectionResult.PlatformVersion = version;
            return detectionResult;
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            var manifestFileProperties = new Dictionary<string, string>();
            manifestFileProperties[ManifestFilePropertyKeys.HugoVersion] = detectorResult.PlatformVersion;

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.HugoSnippet,
                model: null,
                _logger);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,
            };
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            return null;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            string installationScriptSnippet = null;
            if (_commonOptions.EnableDynamicInstall)
            {
                _logger.LogDebug("Dynamic install is enabled.");

                if (_platformInstaller.IsVersionAlreadyInstalled(detectorResult.PlatformVersion))
                {
                    _logger.LogDebug(
                       "Hugo version {version} is already installed. So skipping installing it again.",
                       detectorResult.PlatformVersion);
                }
                else
                {
                    _logger.LogDebug(
                        "Hugo version {version} is not installed. " +
                        "So generating an installation script snippet for it.",
                        detectorResult.PlatformVersion);

                    installationScriptSnippet = _platformInstaller.GetInstallerScriptSnippet(
                        detectorResult.PlatformVersion);
                }
            }
            else
            {
                _logger.LogDebug("Dynamic install not enabled.");
            }

            return installationScriptSnippet;
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return version;
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return true;
        }

        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        public void SetRequiredTools(
            PlatformDetectorResult detectorResult,
            [NotNull] IDictionary<string, string> toolsToVersion)
        {
            if (!string.IsNullOrWhiteSpace(detectorResult.PlatformVersion))
            {
                toolsToVersion[ToolNameConstants.HugoName] = detectorResult.PlatformVersion;
            }
        }

        private string GetVersionUsingHierarchicalRules(string detectedVersion)
        {
            if (!string.IsNullOrEmpty(_hugoScriptGeneratorOptions.HugoVersion))
            {
                return _hugoScriptGeneratorOptions.HugoVersion;
            }

            if (detectedVersion != null)
            {
                return detectedVersion;
            }

            return HugoConstants.DefaultVersion;
        }

        private bool TryGetExplicitVersion(out string explicitVersion)
        {
            explicitVersion = null;

            var platformName = _commonOptions.PlatformName;
            if (platformName.EqualsIgnoreCase(HugoConstants.PlatformName))
            {
                if (string.IsNullOrWhiteSpace(_hugoScriptGeneratorOptions.HugoVersion))
                {
                    return false;
                }

                explicitVersion = _hugoScriptGeneratorOptions.HugoVersion;
                return true;
            }

            return false;
        }
    }
}
