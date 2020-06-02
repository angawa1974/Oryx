// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class PlatformsInstallationScriptProviderTest
    {
        [Fact]
        public void ContainsInstallationScriptContent_FromSinglePlatform()
        {
            // Arrange
            var detector = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test",
                detectedPlatformVersion: "1.0.0");
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content",
                installationScriptContent: "install-content",
                detector: detector);
            var envScriptProvider = CreateEnvironmentSetupScriptProvider(new[] { platform });
            var context = CreateScriptGeneratorContext();

            // Act
            var setupScript = envScriptProvider.GetBashScriptSnippet(context);

            // Assert
            Assert.Contains("install-content", setupScript);
            Assert.DoesNotContain("script-content", setupScript);
        }

        [Fact]
        public void ContainsInstallationScriptContent_FromMultiplePlatforms()
        {
            // Arrange
            var detector1 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test1",
                detectedPlatformVersion: "1.0.0");
            var platform1 = new TestProgrammingPlatform(
                "test1",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script1-content",
                installationScriptContent: "install1-content",
                detector: detector1);
            var detector2 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "test2",
                detectedPlatformVersion: "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "test2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script2-content",
                installationScriptContent: "install2-content",
                detector: detector2);
            var envScriptProvider = CreateEnvironmentSetupScriptProvider(new[] { platform1, platform2 });
            var context = CreateScriptGeneratorContext();

            // Act
            var setupScript = envScriptProvider.GetBashScriptSnippet(context);

            // Assert
            Assert.Contains("install1-content", setupScript);
            Assert.Contains("install2-content", setupScript);
            Assert.DoesNotContain("script1-content", setupScript);
            Assert.DoesNotContain("script2-content", setupScript);
        }

        private PlatformsInstallationScriptProvider CreateEnvironmentSetupScriptProvider(
            IEnumerable<IProgrammingPlatform> platforms)
        {
            var platformDetector = new DefaultPlatformDetector(
                platforms,
                new DefaultStandardOutputWriter());
            return new PlatformsInstallationScriptProvider(
                platforms,
                platformDetector,
                new DefaultStandardOutputWriter());
        }

        private static BuildScriptGeneratorContext CreateScriptGeneratorContext()
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = new MemorySourceRepo(),
            };
        }
    }
}
