// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="TestProjectPaths.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest.Integration
{
    using System;
    using System.IO;

    internal static class TestProjectPaths
    {
        static TestProjectPaths()
        {
            var tfmDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            TargetFramework = tfmDirectory.Name;

            var configurationDirectory = tfmDirectory.Parent ?? throw new InvalidOperationException("Unable to determine configuration directory for the test assembly output.");
            Configuration = configurationDirectory.Name;

            var binDirectory = configurationDirectory.Parent ?? throw new InvalidOperationException("Unable to determine bin directory for the test assembly output.");
            var projectDirectory = binDirectory.Parent ?? throw new InvalidOperationException("Unable to determine test project directory.");
            TestProjectDirectory = projectDirectory.FullName;

            var solutionDirectory = projectDirectory.Parent ?? throw new InvalidOperationException("Unable to determine solution root.");
            SolutionRoot = solutionDirectory.FullName;

            TestDataDirectory = Path.Combine(TestProjectDirectory, "TestData");

            DnsCliOutputDirectory = Path.Combine(SolutionRoot, "dns-cli", "bin", Configuration, TargetFramework);
            DnsCliDllPath = Path.Combine(DnsCliOutputDirectory, "dns-cli.dll");
        }

        public static string Configuration { get; }

        public static string TargetFramework { get; }

        public static string TestProjectDirectory { get; }

        public static string TestDataDirectory { get; }

        public static string SolutionRoot { get; }

        public static string DnsCliOutputDirectory { get; }

        public static string DnsCliDllPath { get; }
    }
}
