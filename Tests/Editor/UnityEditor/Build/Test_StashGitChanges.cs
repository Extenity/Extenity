using System.IO;
using Extenity.BuildToolbox.Editor;
using Extenity.FileSystemToolbox;
using NUnit.Framework;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityTests.Build
{

	public class Test_StashGitChanges
	{
		private string DummyRepoPath;
		private string DummySubmodulePath;

		[OneTimeSetUp]
		public void Setup()
		{
			DummyRepoPath = Path.Combine(Application.persistentDataPath, "DummyRepo").FixDirectorySeparatorChars();
			DummySubmodulePath = Path.Combine(Application.persistentDataPath, "Submodule").FixDirectorySeparatorChars();
			Log.Info($"DummyRepoPath: {DummyRepoPath}");
			Log.Info($"DummySubmodulePath: {DummySubmodulePath}");

			DeleteDummyRepoFolders();

			CreateDummyRepo(DummyRepoPath);
			CreateDummyRepo(DummySubmodulePath);

			CreateDummyFiles(DummyRepoPath);
			CreateDummyFiles(DummySubmodulePath);

			CommitDummyFiles(DummyRepoPath);
			CommitDummyFiles(DummySubmodulePath);

			CreateDummyFiles(DummySubmodulePath);

			AddSubmoduleToTheDummyRepo(DummyRepoPath, DummySubmodulePath);

			CommitDummyFiles(DummyRepoPath);

			CreateDummyFiles(DummyRepoPath);

			CreateDummyFiles(Path.Combine(DummyRepoPath, "Sub"));
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			DeleteDummyRepoFolders();
		}

		private void CreateDummyRepo(string repoPath)
		{
			DirectoryTools.Create(repoPath);

			RunGit(repoPath, "init");
			RunGit(repoPath, "config core.safecrlf false"); // Disable CRLF line ending conversion warnings.
		}

		private void DeleteDummyRepoFolders()
		{
			DirectoryTools.DeleteWithContent(DummySubmodulePath);
			DirectoryTools.DeleteWithContent(DummyRepoPath);
		}

		private void CreateDummyFiles(string repoPath)
		{
			var files = new DirectoryInfo(repoPath).GetFiles();

			for (int i = files.Length; i < files.Length + 5; i++)
			{
				File.WriteAllText(Path.Combine(repoPath, $"File{i}.txt"), "File content for " + i);
			}
		}

		private void CommitDummyFiles(string repoPath)
		{
			RunGit(repoPath, "add .");
			RunGit(repoPath, "commit -m 'test'");
		}

		private void AddSubmoduleToTheDummyRepo(string repoPath, string submodule)
		{
			RunGit(repoPath, $"submodule add {submodule} Sub");
			RunGit(repoPath, "submodule update --init --force --remote");
		}

		private void RunGit(string repoPath, string command)
		{
			new GitCommandRunner(repoPath).Run(command, out var exitCode);
			Assert.AreEqual(0, exitCode);
		}

		[Test, Order(1)]
		public void Step1_StashAllGitLocalChanges()
		{
			BuildTools.StashAllLocalGitChanges(DummyRepoPath, false);
		}

		[Test, Order(2)]
		public void Step2_StashAllGitLocalChangesIncludingSubmodules()
		{
			BuildTools.StashAllLocalGitChanges(DummyRepoPath, true);
		}

		[Test, Order(3)]
		public void Step3_ApplyLastStash()
		{
			BuildTools.ApplyLastGitStash(DummyRepoPath, false);
		}

		[Test, Order(4)]
		public void Step4_ApplyLastStashIncludingSubmodules()
		{
			BuildTools.StashAllLocalGitChanges(DummyRepoPath, true);
			BuildTools.ApplyLastGitStash(DummyRepoPath, true);
		}

		#region Log

		private static readonly Logger Log = new(nameof(Test_StashGitChanges));

		#endregion
	}

}
