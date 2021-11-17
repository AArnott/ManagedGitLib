﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using Xunit;
using Xunit.Abstractions;

namespace ManagedGitLib.Tests
{
    public class GitRepositoryTests : IDisposable
    {
        DirectoryInfo notARepo;

        public GitRepositoryTests()
        {
            notARepo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            File.WriteAllText(Path.Combine(notARepo.FullName, "file.txt"), Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            notARepo.Delete(true);
        }

        [Fact]
        public void CreateNotARepoTest()
        {
            Assert.Null(GitRepository.Create(null));
            Assert.Null(GitRepository.Create(""));
            Assert.Null(GitRepository.Create("/A/Path/To/A/Directory/Which/Does/Not/Exist"));
            Assert.True(notARepo.Exists);
            Assert.True(notARepo.GetFiles().Length > 0);
            Assert.Null(GitRepository.Create(notARepo.FullName));
        }

        [Fact]
        public void ParseAlternates_SingleValue_Test()
        {
            var alternates = GitRepository.ParseAlternates(Encoding.UTF8.GetBytes("/home/git/nbgv/.git/objects\n"));
            Assert.Collection(
                alternates,
                a => Assert.Equal("/home/git/nbgv/.git/objects", a));
        }

        [Fact]
        public void ParseAlternates_SingleValue_NoTrailingNewline_Test()
        {
            var alternates = GitRepository.ParseAlternates(Encoding.UTF8.GetBytes("../repo/.git/objects"));
            Assert.Collection(
                alternates,
                a => Assert.Equal("../repo/.git/objects", a));
        }

        [Fact]
        public void ParseAlternates_TwoValues_Test()
        {
            var alternates = GitRepository.ParseAlternates(Encoding.UTF8.GetBytes("/home/git/nbgv/.git/objects:../../clone/.git/objects\n"));
            Assert.Collection(
                alternates,
                a => Assert.Equal("/home/git/nbgv/.git/objects", a),
                a => Assert.Equal("../../clone/.git/objects", a));
        }

        [Fact]
        public void ParseAlternates_PathWithColon_Test()
        {
            var alternates = GitRepository.ParseAlternates(
                Encoding.UTF8.GetBytes("C:/Users/nbgv/objects:C:/Users/nbgv2/objects/:../../clone/.git/objects\n"),
                2);
            Assert.Collection(
                alternates,
                a => Assert.Equal("C:/Users/nbgv/objects", a),
                a => Assert.Equal("C:/Users/nbgv2/objects/", a),
                a => Assert.Equal("../../clone/.git/objects", a));
        }
    }
}
