﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using TestUtilities;
using Xunit;

namespace Microsoft.SourceLink.IntegrationTests
{
    public class VstsTests : DotNetSdkTestBase
    {
        public VstsTests() 
            : base("Microsoft.SourceLink.Vsts.Git")
        {
        }

        [ConditionalTheory(typeof(DotNetSdkAvailable))]
        [InlineData("visualstudio.com")]
        [InlineData("vsts.me")]
        public void FullValidation_Https(string host)
        {
            var repo = GitUtilities.CreateGitRepositoryWithSingleCommit(ProjectDir.Path, new[] { ProjectFileName }, $"https://test.{host}/test-org/_git/test-repo");
            var commitSha = repo.Head.Tip.Sha;

            VerifyValues(
                customProps: @"
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
",
                customTargets: "",
                targets: new[]
                {
                    "Build", "Pack"
                },
                expressions: new[]
                {
                    "@(SourceRoot)",
                    "@(SourceRoot->'%(SourceLinkUrl)')",
                    "$(SourceLink)",
                    "$(PrivateRepositoryUrl)",
                    "$(RepositoryUrl)"
                },
                expectedResults: new[]
                {
                    ProjectSourceRoot,
                    $"https://test.{host}/test-org/_apis/git/repositories/test-repo/items?api-version=1.0&versionType=commit&version={commitSha}&path=/*",
                    s_relativeSourceLinkJsonPath,
                    $"https://test.{host}/test-org/_git/test-repo",
                    $"https://test.{host}/test-org/_git/test-repo",
                });

            AssertEx.AreEqual(
                $@"{{""documents"":{{""{ProjectSourceRoot.Replace(@"\", @"\\")}*"":""https://test.{host}/test-org/_apis/git/repositories/test-repo/items?api-version=1.0&versionType=commit&version={commitSha}&path=/*""}}}}",
                File.ReadAllText(Path.Combine(ProjectDir.Path, s_relativeSourceLinkJsonPath)));

            TestUtilities.ValidateAssemblyInformationalVersion(
                Path.Combine(ProjectDir.Path, s_relativeOutputFilePath), 
                "1.0.0+" + commitSha);

            TestUtilities.ValidateNuSpecRepository(
                Path.Combine(ProjectDir.Path, s_relativePackagePath),
                type: "git", 
                commit: commitSha,
                url: $"https://test.{host}/test-org/_git/test-repo");
        }

        [ConditionalTheory(typeof(DotNetSdkAvailable))]
        [InlineData("visualstudio.com")]
        [InlineData("vsts.me")]
        public void FullValidation_Ssh(string host)
        {
            var repo = GitUtilities.CreateGitRepositoryWithSingleCommit(ProjectDir.Path, new[] { ProjectFileName }, $"ssh://test@vs-ssh.{host}:22/test-org/_ssh/test-repo");
            var commitSha = repo.Head.Tip.Sha;

            VerifyValues(
                customProps: @"
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
",
                customTargets: "",
                targets: new[]
                {
                    "Build", "Pack"
                },
                expressions: new[]
                {
                    "@(SourceRoot)",
                    "@(SourceRoot->'%(SourceLinkUrl)')",
                    "$(SourceLink)",
                    "$(PrivateRepositoryUrl)",
                    "$(RepositoryUrl)"
                },
                expectedResults: new[]
                {
                    ProjectSourceRoot,
                    $"https://test.{host}/test-org/_apis/git/repositories/test-repo/items?api-version=1.0&versionType=commit&version={commitSha}&path=/*",
                    s_relativeSourceLinkJsonPath,
                    $"https://test.{host}/test-org/_git/test-repo",
                    $"https://test.{host}/test-org/_git/test-repo",
                });

            AssertEx.AreEqual(
                $@"{{""documents"":{{""{ProjectSourceRoot.Replace(@"\", @"\\")}*"":""https://test.{host}/test-org/_apis/git/repositories/test-repo/items?api-version=1.0&versionType=commit&version={commitSha}&path=/*""}}}}",
                File.ReadAllText(Path.Combine(ProjectDir.Path, s_relativeSourceLinkJsonPath)));

            TestUtilities.ValidateAssemblyInformationalVersion(
                Path.Combine(ProjectDir.Path, s_relativeOutputFilePath),
                "1.0.0+" + commitSha);

            TestUtilities.ValidateNuSpecRepository(
                Path.Combine(ProjectDir.Path, s_relativePackagePath),
                type: "git",
                commit: commitSha,
                url: $"https://test.{host}/test-org/_git/test-repo");
        }
    }
}
