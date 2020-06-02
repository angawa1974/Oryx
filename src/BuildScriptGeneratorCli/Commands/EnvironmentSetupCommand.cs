// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Sets up environment by detecting and installing platforms.")]
    internal class EnvironmentSetupCommand : CommandBase
    {
        public const string Name = "setup";

        [Argument(0, Description = "The source directory.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<EnvironmentSetupCommand>>();
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            var beginningOutputLog = GetBeginningCommandOutputLog();
            console.WriteLine(beginningOutputLog);

            int exitCode;
            using (var timedEvent = logger.LogTimedEvent("EnvSetupCommand"))
            {
                var context = BuildScriptGenerator.CreateContext(serviceProvider, operationId: null);
                var detector = serviceProvider.GetRequiredService<DefaultPlatformDetector>();
                var detectedPlatforms = detector.DetectPlatforms(context);
                if (!detectedPlatforms.Any())
                {
                    return ProcessConstants.ExitFailure;
                }

                var environmentScriptProvider = serviceProvider.GetRequiredService<PlatformsInstallationScriptProvider>();
                var snippet = environmentScriptProvider.GetBashScriptSnippet(context, detectedPlatforms);

                var scriptBuilder = new StringBuilder()
                    .AppendLine($"#!{FilePaths.Bash}")
                    .AppendLine("set -e")
                    .AppendLine();

                if (!string.IsNullOrEmpty(snippet))
                {
                    scriptBuilder
                        .AppendLine("echo")
                        .AppendLine("echo Setting up environment...")
                        .AppendLine("echo")
                        .AppendLine(snippet)
                        .AppendLine("echo")
                        .AppendLine("echo Done setting up environment.")
                        .AppendLine("echo");
                }

                // Create temporary file to store script
                // Get the path where the generated script should be written into.
                var tempDirectoryProvider = serviceProvider.GetRequiredService<ITempDirectoryProvider>();
                var tempScriptPath = Path.Combine(tempDirectoryProvider.GetTempDirectory(), "setupEnvironment.sh");
                var script = scriptBuilder.ToString();
                File.WriteAllText(tempScriptPath, script);
                timedEvent.AddProperty(nameof(tempScriptPath), tempScriptPath);

                if (DebugMode)
                {
                    console.WriteLine($"Temporary script @ {tempScriptPath}:");
                    console.WriteLine("---");
                    console.WriteLine(scriptBuilder);
                    console.WriteLine("---");
                }

                var environment = serviceProvider.GetRequiredService<IEnvironment>();
                var shellPath = environment.GetEnvironmentVariable("BASH") ?? FilePaths.Bash;

                exitCode = ProcessHelper.RunProcess(
                    shellPath,
                    new[] { tempScriptPath },
                    options.SourceDir,
                    (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            console.WriteLine(args.Data);
                        }
                    },
                    (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            console.Error.WriteLine(args.Data);
                        }
                    },
                    waitTimeForExit: null);
                timedEvent.AddProperty("exitCode", exitCode.ToString());
            }

            return exitCode;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(options, sourceDir: SourceDir);
        }

        internal override IServiceProvider GetServiceProvider(IConsole console)
        {
            // Gather all the values supplied by the user in command line
            SourceDir = string.IsNullOrEmpty(SourceDir) ?
                Directory.GetCurrentDirectory() : Path.GetFullPath(SourceDir);

            // NOTE: Order of the following is important. So a command line provided value has higher precedence
            // than the value provided in a configuration file of the repo.
            var config = new ConfigurationBuilder()
                .AddIniFile(Path.Combine(SourceDir, Constants.BuildEnvironmentFileName), optional: true)
                .AddEnvironmentVariables()
                .Add(GetCommandLineConfigSource())
                .Build();

            // Override the GetServiceProvider() call in CommandBase to pass the IConsole instance to
            // ServiceProviderBuilder and allow for writing to the console if needed during this command.
            var serviceProviderBuilder = new ServiceProviderBuilder(LogFilePath, console)
                .ConfigureServices(services =>
                {
                    // Configure Options related services
                    // We first add IConfiguration to DI so that option services like
                    // `DotNetCoreScriptGeneratorOptionsSetup` services can get it through DI and read from the config
                    // and set the options.
                    services
                        .AddSingleton<IConfiguration>(config)
                        .AddOptionsServices()
                        .Configure<BuildScriptGeneratorOptions>(options =>
                        {
                            // These values are not retrieve through the 'config' api since we do not expect
                            // them to be provided by an end user.
                            options.SourceDir = SourceDir;
                        });
                });

            return serviceProviderBuilder.Build();
        }

        private CustomConfigurationSource GetCommandLineConfigSource()
        {
            var commandLineConfigSource = new CustomConfigurationSource();
            return commandLineConfigSource;
        }
    }
}
