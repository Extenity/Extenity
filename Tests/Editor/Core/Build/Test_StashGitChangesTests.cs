using System;
using System.Diagnostics;
using System.IO;
using Extenity.FileSystemToolbox;
using NUnit.Framework;
using UnityEngine;
namespace ExtenityTests.Build
{
    public class Test_StashGitChangesTests
    {
        private static readonly string _dummyRepoPath = Path.Combine(Application.persistentDataPath,"DummyRepo");
        
        [OneTimeSetUp]
        public void Setup()
        {
            
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            
        }

        private void CreateDummyRepo()
        {
            if (!Directory.Exists(_dummyRepoPath))
            {
                Directory.CreateDirectory(_dummyRepoPath);
            }
        }

        private void CreateDummyFiles()
        {
            
        }

        private void AddSubmoduleToTheDummyRepo()
        {
            
        }

        private void DeleteDummyRepoFolder()
        {
            
        }
    }
}
