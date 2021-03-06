﻿using System.IO;
using System.Linq;

using Xunit;

namespace Coverlet.Integration.Tests
{
    public class Collectors : BaseTest
    {
        private ClonedTemplateProject PrepareTemplateProject()
        {
            ClonedTemplateProject clonedTemplateProject = CloneTemplateProject();
            UpdateNugeConfigtWithLocalPackageFolder(clonedTemplateProject.ProjectRootPath!);
            AddCoverletCollectosRef(clonedTemplateProject.ProjectRootPath!);
            return clonedTemplateProject;
        }

        private void AssertCollectorsInjection(ClonedTemplateProject clonedTemplateProject)
        {
            // Check out/in process collectors injection
            Assert.Contains("[coverlet]", File.ReadAllText(clonedTemplateProject.GetFiles("log.datacollector.*.txt").Single()));
            Assert.Contains("[coverlet]", File.ReadAllText(clonedTemplateProject.GetFiles("log.host.*.txt").Single()));
        }

        [Fact]
        public void TestVsTest_Test()
        {
            using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
            string runSettingsPath = AddCollectorRunsettingsFile(clonedTemplateProject.ProjectRootPath!);
            Assert.True(DotnetCli($"test \"{clonedTemplateProject.ProjectRootPath}\" --collect:\"XPlat Code Coverage\" --diag:{Path.Combine(clonedTemplateProject.ProjectRootPath, "log.txt")}", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!), standardOutput);
            // We don't have any result to check because tests and code to instrument are in same assembly so we need to pass
            // IncludeTestAssembly=true we do it in other test
            Assert.Contains("Test Run Successful.", standardOutput);
            AssertCollectorsInjection(clonedTemplateProject);
        }

        [Fact]
        public void TestVsTest_Test_Settings()
        {
            using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
            string runSettingsPath = AddCollectorRunsettingsFile(clonedTemplateProject.ProjectRootPath!);
            Assert.True(DotnetCli($"test \"{clonedTemplateProject.ProjectRootPath}\" --collect:\"XPlat Code Coverage\" --settings \"{runSettingsPath}\" --diag:{Path.Combine(clonedTemplateProject.ProjectRootPath, "log.txt")}", out string standardOutput, out string standardError), standardOutput);
            Assert.Contains("Test Run Successful.", standardOutput);
            AssertCoverage(clonedTemplateProject);
            AssertCollectorsInjection(clonedTemplateProject);
        }

        [Fact]
        public void TestVsTest_VsTest()
        {
            using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
            string runSettingsPath = AddCollectorRunsettingsFile(clonedTemplateProject.ProjectRootPath!);
            Assert.True(DotnetCli($"publish {clonedTemplateProject.ProjectRootPath}", out string standardOutput, out string standardError), standardOutput);
            string publishedTestFile = clonedTemplateProject.GetFiles("*" + ClonedTemplateProject.AssemblyName + ".dll").Single(f => f.Contains("publish"));
            Assert.NotNull(publishedTestFile);
            Assert.True(DotnetCli($"vstest \"{publishedTestFile}\" --collect:\"XPlat Code Coverage\" --diag:{Path.Combine(clonedTemplateProject.ProjectRootPath, "log.txt")}", out standardOutput, out standardError), standardOutput);
            // We don't have any result to check because tests and code to instrument are in same assembly so we need to pass
            // IncludeTestAssembly=true we do it in other test
            Assert.Contains("Test Run Successful.", standardOutput);
            AssertCollectorsInjection(clonedTemplateProject);
        }

        [Fact]
        public void TestVsTest_VsTest_Settings()
        {
            using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
            string runSettingsPath = AddCollectorRunsettingsFile(clonedTemplateProject.ProjectRootPath!);
            Assert.True(DotnetCli($"publish \"{clonedTemplateProject.ProjectRootPath}\"", out string standardOutput, out string standardError), standardOutput);
            string publishedTestFile = clonedTemplateProject.GetFiles("*" + ClonedTemplateProject.AssemblyName + ".dll").Single(f => f.Contains("publish"));
            Assert.NotNull(publishedTestFile);
            Assert.True(DotnetCli($"vstest \"{publishedTestFile}\" --collect:\"XPlat Code Coverage\" --ResultsDirectory:\"{clonedTemplateProject.ProjectRootPath}\" /settings:\"{runSettingsPath}\" --diag:{Path.Combine(clonedTemplateProject.ProjectRootPath, "log.txt")}", out standardOutput, out standardError), standardOutput);
            Assert.Contains("Test Run Successful.", standardOutput);
            AssertCoverage(clonedTemplateProject);
            AssertCollectorsInjection(clonedTemplateProject);
        }
    }
}
