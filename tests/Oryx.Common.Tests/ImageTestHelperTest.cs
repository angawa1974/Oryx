﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Common.Tests
{
    public class ImageTestHelperTest
    {
        private const string _imageBaseEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        private const string _tagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        private const string _defaultImageBase = "oryxdevmcr.azurecr.io/public/oryx";

        private const string _buildRepository = "build";
        private const string _packRepository = "pack";
        private const string _latestTag = "latest";
        private const string _ltsVersionsTag = "lts-versions";

        private readonly ITestOutputHelper _output;

        public ImageTestHelperTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_ImageBaseSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_TagSuffixSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}{tagSuffixValue}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_NoImageBaseOrTagSuffixSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{platformName}:{platformVersion}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestRuntimeImage_Validate_BothImageBaseAndTagSuffixSet()
        {
            // Arrange
            var platformName = "test";
            var platformVersion = "1.0";
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var runtimeImage = imageHelper.GetRuntimeImage(platformName, platformVersion);

            // Assert
            var expectedImage = $"{imageBaseValue}/{platformName}:{platformVersion}{tagSuffixValue}";
            Assert.Equal(expectedImage, runtimeImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_ImageBaseSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_latestTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_TagSuffixSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage();

            // Assert
            var expectedTag = tagSuffixValue.TrimStart('-');
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{expectedTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetLtsVersionsBuildImage_Validate_ImageBaseSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetLtsVersionsBuildImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_buildRepository}:{_ltsVersionsTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetLtsVersionsBuildImage_Validate_TagSuffixSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetLtsVersionsBuildImage();

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_ltsVersionsTag}{tagSuffixValue}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestPackImage_Validate_ImageBaseSet()
        {
            // Arrange
            var imageBaseValue = "oryxtest";
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var packImage = imageHelper.GetPackImage();

            // Assert
            var expectedImage = $"{imageBaseValue}/{_packRepository}:{_latestTag}";
            Assert.Equal(expectedImage, packImage);
        }

        [Fact]
        public void GetTestPackImage_Validate_TagSuffixSet()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = "-buildNumber";
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var packImage = imageHelper.GetPackImage();

            // Assert
            var expectedTag = tagSuffixValue.TrimStart('-');
            var expectedImage = $"{_defaultImageBase}/{_packRepository}:{expectedTag}";
            Assert.Equal(expectedImage, packImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_LatestTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage(_latestTag);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_latestTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_LatestVersionsTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Act
            var buildImage = imageHelper.GetBuildImage(_ltsVersionsTag);

            // Assert
            var expectedImage = $"{_defaultImageBase}/{_buildRepository}:{_ltsVersionsTag}";
            Assert.Equal(expectedImage, buildImage);
        }

        [Fact]
        public void GetTestBuildImage_Validate_InvalidTag()
        {
            // Arrange
            var imageBaseValue = string.Empty;
            var tagSuffixValue = string.Empty;
            var imageHelper = new ImageTestHelper(_output, imageBaseValue, tagSuffixValue);

            // Assert
            Assert.Throws<NotSupportedException>(() => { imageHelper.GetBuildImage("invalidTag"); });
        }
    }
}
