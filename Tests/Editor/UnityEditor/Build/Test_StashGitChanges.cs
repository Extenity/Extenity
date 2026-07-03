using System.IO;
using Extenity.BuildToolbox.Editor;
using Extenity.FileSystemToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;
using static Extenity.DataToolbox.StringFilterEntry;

namespace ExtenityTests.Build
{

	public class Test_StashGitChanges : ExtenityTestBase
	{
		private string DummyRepoPath;
		private string DummySubmodulePath;

		[OneTimeSetUp]
		public void Setup()
		{
			DummyRepoPath = Path.Combine(Application.persistentDataPath, "DummyRepo").FixDirectorySeparatorChars();
			DummySubmodulePath = Path.Combine(Application.persistentDataPath, "Submodule").FixDirectorySeparatorChars();

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

			RunGit(repoPath, "init --initial-branch=main");
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
			// The submodule path must be quoted, since it may contain spaces (e.g. Unity's
			// persistentDataPath on macOS goes through "Application Support"). Also, git 2.38+
			// refuses the 'file' transport for submodule operations by default, so it is
			// explicitly allowed here via '-c', which keeps the override scoped to these
			// commands on the dummy repo instead of touching any real git configuration.
			RunGit(repoPath, $"-c protocol.file.allow=always submodule add \"{submodule}\" Sub");
			RunGit(repoPath, "-c protocol.file.allow=always submodule update --init --force --remote");
		}

		private void RunGit(string repoPath, string command)
		{
			// The dummy-repo scaffolding commands are silenced to keep the console clean. Errors are
			// still logged, and the exit code assertion below catches any failure.
			new GitCommandRunner(repoPath).Run(command, out var exitCode, logOutput: false);
			Assert.AreEqual(0, exitCode);
		}

		[Test]
		public void StashAndApplyGitLocalChanges()
		{
			// Step 1: Stash local changes in the main repo only.
			BuildTools.StashAllLocalGitChanges(DummyRepoPath, false);

			AssertExpectLog((LogType.Log, CreateExact("[Git] Running: git add .")),
			                (LogType.Log, CreateExact("[Git] Running: git stash")),
			                (LogType.Log, CreateStartsWith("[Git] Saved working directory and index state WIP on main:")));

			// Step 2: Stash including submodules. The main repo was already stashed in Step 1, so only the
			// submodule has changes left to save.
			BuildTools.StashAllLocalGitChanges(DummyRepoPath, true);

			AssertExpectLog((LogType.Log, CreateExact("[Git] Running: git submodule foreach git add .")),
			                (LogType.Log, CreateStartsWith("[Git] Entering 'Sub'")),
			                (LogType.Log, CreateExact("[Git] Running: git submodule foreach git stash")),
			                (LogType.Log, CreateContains("Saved working directory and index state WIP on (no branch):")),
			                (LogType.Log, CreateExact("[Git] Running: git add .")),
			                (LogType.Log, CreateExact("[Git] Running: git stash")),
			                (LogType.Log, CreateStartsWith("[Git] No local changes to save")));

			// Step 3: Apply the last stash on the main repo only.
			BuildTools.ApplyLastGitStash(DummyRepoPath, false);

			AssertExpectLog((LogType.Log, CreateExact("[Git] Running: git stash pop 0")),
			                (LogType.Log, CreateStartsWith("[Git] On branch main")));

			// Step 4: Stash and apply including submodules. The submodule's changes are still sitting in its
			// stash from Step 2, so it has nothing to save, while the main repo saves the changes that Step 3
			// just popped.
			BuildTools.StashAllLocalGitChanges(DummyRepoPath, true);

			AssertExpectLog((LogType.Log, CreateExact("[Git] Running: git submodule foreach git add .")),
			                (LogType.Log, CreateStartsWith("[Git] Entering 'Sub'")),
			                (LogType.Log, CreateExact("[Git] Running: git submodule foreach git stash")),
			                (LogType.Log, CreateContains("No local changes to save")),
			                (LogType.Log, CreateExact("[Git] Running: git add .")),
			                (LogType.Log, CreateExact("[Git] Running: git stash")),
			                (LogType.Log, CreateStartsWith("[Git] Saved working directory and index state WIP on main:")));

			BuildTools.ApplyLastGitStash(DummyRepoPath, true);

			AssertExpectLog((LogType.Log, CreateExact("[Git] Running: git submodule foreach git stash pop 0")),
			                (LogType.Log, CreateStartsWith("[Git] Entering 'Sub'")),
			                (LogType.Log, CreateExact("[Git] Running: git stash pop 0")),
			                (LogType.Log, CreateStartsWith("[Git] On branch main")));
		}
	}

}
