using System.IO;
using Extenity.BuildToolbox.Editor;
using Extenity.FileSystemToolbox;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace ExtenityTests.Build
{
    public class Test_StashGitChangesTests
    {
        private static readonly string _dummyRepoPath = Path.Combine(Application.persistentDataPath, "DummyRepo").FixDirectorySeparatorChars();
        private static readonly string _dummySubmodulePath = Path.Combine(Application.persistentDataPath, "Submodule").FixDirectorySeparatorChars();

        private GitCommandRunner _commandRunner;
        [OneTimeSetUp]
        public void Setup()
        {
            CreateDummyRepo(_dummyRepoPath);
            CreateDummyRepo(_dummySubmodulePath);

            CreateDummyFiles(_dummyRepoPath);
            CreateDummyFiles(_dummySubmodulePath);

            CommitDummyFiles(_dummyRepoPath);
            CommitDummyFiles(_dummySubmodulePath);
            
            CreateDummyFiles(_dummySubmodulePath);

            AddSubmoduleToTheDummyRepo(_dummyRepoPath, _dummySubmodulePath);

            CommitDummyFiles(_dummyRepoPath);

            CreateDummyFiles(_dummyRepoPath);
            
            CreateDummyFiles(Path.Combine(_dummyRepoPath, "Sub"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            DeleteDummyRepoFolders();
        }

        private void CreateDummyRepo(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _commandRunner = new GitCommandRunner(path);

            var output = _commandRunner.Run($"init . -b master", out int exitCode);

            Debug.Log(output);
        }

        private void CreateDummyFiles(string repoPath)
        {
            var directoryInfo = new DirectoryInfo(repoPath);

            var files = directoryInfo.GetFiles();

            for (int i = files.Length; i < files.Length + 5; i++)
            {
                File.WriteAllText(Path.Combine(repoPath, $"File{i}.txt"), i.ToString());
            }
        }
        private void CommitDummyFiles(string repoPath)
        {
            _commandRunner = new GitCommandRunner(repoPath);
            _commandRunner.Run("add .", out int exitCode);
            _commandRunner.Run("commit -m 'test'", out exitCode);

            Assert.AreEqual(0, exitCode);
        }

        private void AddSubmoduleToTheDummyRepo(string repo, string submodule)
        {
            _commandRunner = new GitCommandRunner(repo);
            _commandRunner.Run($"submodule add {submodule}  Sub", out var exitCode);
            _commandRunner.Run("submodule update --init --force --remote", out exitCode);
            Assert.AreEqual(0, exitCode);

        }

        [Test, Order(0)]
        public void StashAllGitLocalChanges()
        {
            BuildTools.StashAllLocalGitChanges(_dummyRepoPath, null, false);
        }
        [Test, Order(1)]
        public void StashAllGitLocalChangesIncludingSubmodules()
        {
            BuildTools.StashAllLocalGitChanges(_dummyRepoPath);
        }
        [Test, Order(2)]
        public void ApplyLastStash()
        {
            BuildTools.ApplyLastGitStash(_dummyRepoPath,false);
        }
        [Test, Order(3)]
        public void ApplyLastStashIncludingSubmodules()
        {
            StashAllGitLocalChangesIncludingSubmodules();
            BuildTools.ApplyLastGitStash(_dummyRepoPath);
        }


        private void DeleteDummyRepoFolders()
        {
            if (Directory.Exists(_dummySubmodulePath))
            {
                Directory.Delete(_dummySubmodulePath,true);
            }
            if (Directory.Exists(_dummyRepoPath))
            {
                Directory.Delete(_dummyRepoPath,true);
            }
        }
    }
}
