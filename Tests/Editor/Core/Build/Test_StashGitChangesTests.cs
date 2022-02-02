using System;
using System.Diagnostics;
using System.IO;
using Extenity.BuildToolbox.Editor;
using Extenity.FileSystemToolbox;
using NUnit.Framework;
using UnityEngine;
namespace ExtenityTests.Build
{
    public class Test_StashGitChangesTests
    {
        private static readonly string _dummyRepoPath = Path.Combine(Application.persistentDataPath,"DummyRepo");
        private static readonly string _dummySubmodulePath = Path.Combine(Application.persistentDataPath,"Submodule");

        [OneTimeSetUp]
        public void Setup()
        {
            CreateDummyRepo(_dummyRepoPath);
            CreateDummyRepo(_dummySubmodulePath);
            
            CreateDummyFiles(_dummyRepoPath);
            CreateDummyFiles(_dummySubmodulePath);
            
            CommitDummyFiles(_dummySubmodulePath);
            CreateDummyFiles(_dummySubmodulePath);
            
            AddSubmoduleToTheDummyRepo(_dummyRepoPath,_dummySubmodulePath);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            DeleteDummyRepoFolder();
        }

        private void CreateDummyRepo(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            BuildTools.RunConsoleCommandAndCaptureOutput("git", $"git init {path} -b 'master'", out string output);
        }

        private void CreateDummyFiles(string repoPath)
        {
            var directoryInfo = new DirectoryInfo(repoPath);

            var files = directoryInfo.GetFiles();
            
            for (int i = files.Length; i < files.Length + 5; i++)
            {
                File.WriteAllText(Path.Combine(repoPath,$"File{i}.txt"), i.ToString());
            }
        }
        private void CommitDummyFiles(string repoPath)
        {
            BuildTools.RunConsoleCommandAndCaptureOutput("git", $"cd {repoPath} && git add . && git commit -m 'test'", out string output);
        }

        private void AddSubmoduleToTheDummyRepo(string repo, string submodule)
        {
            BuildTools.RunConsoleCommandAndCaptureOutput("git", $"cd {repo} && git submodule add {submodule} Sub",out var output);
        }

        private void DeleteDummyRepoFolder()
        {
            Directory.Delete(_dummyRepoPath,true);
            Directory.Delete(_dummySubmodulePath,true);
        }
    }
}
