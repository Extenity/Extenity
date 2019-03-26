//#define PathToolsSupportDoubleBackslashPaths
using System;
using Extenity.DataToolbox;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_PathTools : AssertionHelper
	{
		#region Alter File Name

		[Test]
		public static void AddPrefixToFileName()
		{
			// With extension
			CheckPathWithPrefix(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				@"Prefix-",
				@"C:\Directory Name\Subdir Name\{0}File Name.fileextension");
			CheckPathWithPrefix(
				@"\Directory Name\Subdir Name\File Name.fileextension",
				@"Prefix-",
				@"\Directory Name\Subdir Name\{0}File Name.fileextension");
			CheckPathWithPrefix(
				@"\File Name.fileextension",
				@"Prefix-",
				@"\{0}File Name.fileextension");
			CheckPathWithPrefix(
				@"File Name.fileextension",
				@"Prefix-",
				@"{0}File Name.fileextension");

			// Without extension
			CheckPathWithPrefix(
				@"C:\Directory Name\Subdir Name\File Name",
				@"Prefix-",
				@"C:\Directory Name\Subdir Name\{0}File Name");
			CheckPathWithPrefix(
				@"\Directory Name\Subdir Name\File Name",
				@"Prefix-",
				@"\Directory Name\Subdir Name\{0}File Name");
			CheckPathWithPrefix(
				@"\File Name",
				@"Prefix-",
				@"\{0}File Name");
			CheckPathWithPrefix(
				@"File Name",
				@"Prefix-",
				@"{0}File Name");
		}

		[Test]
		public static void AddSuffixToFileName()
		{
			// With extension
			CheckPathWithSuffix(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				@"-Suffix",
				@"C:\Directory Name\Subdir Name\File Name{0}.fileextension");
			CheckPathWithSuffix(
				@"\Directory Name\Subdir Name\File Name.fileextension",
				@"-Suffix",
				@"\Directory Name\Subdir Name\File Name{0}.fileextension");
			CheckPathWithSuffix(
				@"Directory Name\Subdir Name\File Name.fileextension",
				@"-Suffix",
				@"Directory Name\Subdir Name\File Name{0}.fileextension");
			CheckPathWithSuffix(
				@"C:\File Name.fileextension",
				@"-Suffix",
				@"C:\File Name{0}.fileextension");
			CheckPathWithSuffix(
				@"\File Name.fileextension",
				@"-Suffix",
				@"\File Name{0}.fileextension");
			CheckPathWithSuffix(
				@"File Name.fileextension",
				@"-Suffix",
				@"File Name{0}.fileextension");

			// Without extension
			CheckPathWithSuffix(
				@"C:\Directory Name\Subdir Name\File Name",
				@"-Suffix",
				@"C:\Directory Name\Subdir Name\File Name{0}");
			CheckPathWithSuffix(
				@"\Directory Name\Subdir Name\File Name",
				@"-Suffix",
				@"\Directory Name\Subdir Name\File Name{0}");
			CheckPathWithSuffix(
				@"Directory Name\Subdir Name\File Name",
				@"-Suffix",
				@"Directory Name\Subdir Name\File Name{0}");
			CheckPathWithSuffix(
				@"C:\File Name",
				@"-Suffix",
				@"C:\File Name{0}");
			CheckPathWithSuffix(
				@"\File Name",
				@"-Suffix",
				@"\File Name{0}");
			CheckPathWithSuffix(
				@"File Name",
				@"-Suffix",
				@"File Name{0}");
		}

		private static void CheckPathWithPrefix(string path, string addition, string expectedPath)
		{
			DoCheckPathWithPrefix(path, addition, expectedPath);
			DoCheckPathWithPrefix(path.TrimAll(), addition.TrimAll(), expectedPath.TrimAll());
			DoCheckPathWithPrefix(path.FixDirectorySeparatorChars('\\'), addition, expectedPath.FixDirectorySeparatorChars('\\'));
			DoCheckPathWithPrefix(path.FixDirectorySeparatorChars('/'), addition, expectedPath.FixDirectorySeparatorChars('/'));
		}

		private static void CheckPathWithSuffix(string path, string addition, string expectedPath)
		{
			DoCheckPathWithSuffix(path, addition, expectedPath);
			DoCheckPathWithSuffix(path.TrimAll(), addition.TrimAll(), expectedPath.TrimAll());
			DoCheckPathWithSuffix(path.FixDirectorySeparatorChars('\\'), addition, expectedPath.FixDirectorySeparatorChars('\\'));
			DoCheckPathWithSuffix(path.FixDirectorySeparatorChars('/'), addition, expectedPath.FixDirectorySeparatorChars('/'));
		}

		private static void DoCheckPathWithPrefix(string path, string addition, string expectedPath)
		{
			expectedPath = string.Format(expectedPath, addition);
			var result = path.AddPrefixToFileName(addition);
			Assert.AreEqual(expectedPath, result);
		}

		private static void DoCheckPathWithSuffix(string path, string addition, string expectedPath)
		{
			expectedPath = string.Format(expectedPath, addition);
			var result = path.AddSuffixToFileName(addition);
			Assert.AreEqual(expectedPath, result);
		}

		#endregion

		#region String Operations - Remove First/Last Directory

		[Test]
		public static void RemoveFirstDirectoryFromPath()
		{
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"C:\",
				null);
			CheckRemoveFirstDirectoryThrows<ArgumentNullException>(
				"",
				null);
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"\\",
				null);
#endif

			// Without filename
			CheckRemoveFirstDirectory(
				@"C:\Directory Name\Subdir Name\",
				@"C:\Subdir Name\");
			CheckRemoveFirstDirectory(
				@"C:\Directory Name\",
				@"C:\");
			CheckRemoveFirstDirectory(
				@"Directory Name\Subdir Name\",
				@"Subdir Name\");
			CheckRemoveFirstDirectory(
				@"Directory Name\",
				@"");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectory(
				@"\\Directory Name\Subdir Name\",
				@"\\Subdir Name\");
			CheckRemoveFirstDirectory(
				@"\\Directory Name\",
				@"\\");
#endif

			// With extension
			CheckRemoveFirstDirectory(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				@"C:\Subdir Name\File Name.fileextension");
			CheckRemoveFirstDirectory(
				@"C:\Directory Name\File Name.fileextension",
				@"C:\File Name.fileextension");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"C:\File Name.fileextension",
				null);
			CheckRemoveFirstDirectory(
				@"Directory Name\Subdir Name\File Name.fileextension",
				@"Subdir Name\File Name.fileextension");
			CheckRemoveFirstDirectory(
				@"Directory Name\File Name.fileextension",
				@"File Name.fileextension");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"File Name.fileextension",
				null);
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectory(
				@"\\Directory Name\Subdir Name\File Name.fileextension",
				@"\\Subdir Name\File Name.fileextension");
			CheckRemoveFirstDirectory(
				@"\\Directory Name\File Name.fileextension",
				@"\\File Name.fileextension");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"\\File Name.fileextension",
				null);
#endif

			// Without extension
			CheckRemoveFirstDirectory(
				@"C:\Directory Name\Subdir Name\File Name",
				@"C:\Subdir Name\File Name");
			CheckRemoveFirstDirectory(
				@"C:\Directory Name\File Name",
				@"C:\File Name");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"C:\File Name",
				null);
			CheckRemoveFirstDirectory(
				@"Directory Name\Subdir Name\File Name",
				@"Subdir Name\File Name");
			CheckRemoveFirstDirectory(
				@"Directory Name\File Name",
				@"File Name");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"File Name",
				null);
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectory(
				@"\\Directory Name\Subdir Name\File Name",
				@"\\Subdir Name\File Name");
			CheckRemoveFirstDirectory(
				@"\\Directory Name\File Name",
				@"\\File Name");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(
				@"\\File Name",
				null);
#endif
		}

		[Test]
		public static void RemoveLastDirectoryFromPath()
		{
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"C:\",
				null);
			CheckRemoveLastDirectoryThrows<ArgumentNullException>(
				"",
				null);
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"\\",
				null);
#endif

			// Without filename
			CheckRemoveLastDirectory(
				@"C:\Directory Name\Subdir Name\",
				@"C:\Directory Name\");
			CheckRemoveLastDirectory(
				@"C:\Directory Name\",
				@"C:\");
			CheckRemoveLastDirectory(
				@"Directory Name\Subdir Name\",
				@"Directory Name\");
			CheckRemoveLastDirectory(
				@"Directory Name\",
				@"");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectory(
				@"\\Directory Name\Subdir Name\",
				@"\\Directory Name\");
			CheckRemoveLastDirectory(
				@"\\Directory Name\",
				@"\\");
#endif

			// With extension
			CheckRemoveLastDirectory(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				@"C:\Directory Name\File Name.fileextension");
			CheckRemoveLastDirectory(
				@"C:\Directory Name\File Name.fileextension",
				@"C:\File Name.fileextension");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"C:\File Name.fileextension",
				null);
			CheckRemoveLastDirectory(
				@"Directory Name\Subdir Name\File Name.fileextension",
				@"Directory Name\File Name.fileextension");
			CheckRemoveLastDirectory(
				@"Directory Name\File Name.fileextension",
				@"File Name.fileextension");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"File Name.fileextension",
				null);
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectory(
				@"\\Directory Name\Subdir Name\File Name.fileextension",
				@"\\Directory Name\File Name.fileextension");
			CheckRemoveLastDirectory(
				@"\\Directory Name\File Name.fileextension",
				@"\\File Name.fileextension");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"\\File Name.fileextension",
				null);
#endif

			// Without extension
			CheckRemoveLastDirectory(
				@"C:\Directory Name\Subdir Name\File Name",
				@"C:\Directory Name\File Name");
			CheckRemoveLastDirectory(
				@"C:\Directory Name\File Name",
				@"C:\File Name");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"C:\File Name",
				null);
			CheckRemoveLastDirectory(
				@"Directory Name\Subdir Name\File Name",
				@"Directory Name\File Name");
			CheckRemoveLastDirectory(
				@"Directory Name\File Name",
				@"File Name");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"File Name",
				null);
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectory(
				@"\\Directory Name\Subdir Name\File Name",
				@"\\Directory Name\File Name");
			CheckRemoveLastDirectory(
				@"\\Directory Name\File Name",
				@"\\File Name");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(
				@"\\File Name",
				null);
#endif
		}

		private static void CheckRemoveFirstDirectory(string path, string expectedPath)
		{
			DoCheckRemoveFirstDirectory(path, expectedPath);
			DoCheckRemoveFirstDirectory(path.TrimAll(), expectedPath.TrimAll());
			DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('\\'), expectedPath.FixDirectorySeparatorChars('\\'));
			DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('/'), expectedPath.FixDirectorySeparatorChars('/'));
		}

		private static void CheckRemoveFirstDirectoryThrows<T>(string path, string expectedPath) where T : Exception
		{
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path, expectedPath));
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path.TrimAll(), expectedPath.TrimAll()));
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('\\'), expectedPath.FixDirectorySeparatorChars('\\')));
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('/'), expectedPath.FixDirectorySeparatorChars('/')));
		}

		private static void CheckRemoveLastDirectory(string path, string expectedPath)
		{
			DoCheckRemoveLastDirectory(path, expectedPath);
			DoCheckRemoveLastDirectory(path.TrimAll(), expectedPath.TrimAll());
			DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('\\'), expectedPath.FixDirectorySeparatorChars('\\'));
			DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('/'), expectedPath.FixDirectorySeparatorChars('/'));
		}

		private static void CheckRemoveLastDirectoryThrows<T>(string path, string expectedPath) where T : Exception
		{
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path, expectedPath));
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path.TrimAll(), expectedPath.TrimAll()));
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('\\'), expectedPath.FixDirectorySeparatorChars('\\')));
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('/'), expectedPath.FixDirectorySeparatorChars('/')));
		}

		private static void DoCheckRemoveFirstDirectory(string path, string expectedPath)
		{
			Assert.AreEqual(expectedPath, path.RemoveFirstDirectoryFromPath());
		}

		private static void DoCheckRemoveLastDirectory(string path, string expectedPath)
		{
			Assert.AreEqual(expectedPath, path.RemoveLastDirectoryFromPath());
		}

		#endregion

		#region String Operations - Get Parent Directory

		[Test]
		public static void GetParentDirectoryName()
		{
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"C:\");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"\");
			CheckGetParentDirectoryNameThrows<ArgumentNullException>(@"");
			CheckGetParentDirectoryNameThrows<ArgumentNullException>(null);
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"\\");
#endif

			// Without filename
			CheckGetParentDirectoryName(
				@"C:\Directory Name\Subdir Name\",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"C:\Directory Name\",
				@"Directory Name");
			CheckGetParentDirectoryName(
				@"Directory Name\Subdir Name\",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"Directory Name\",
				@"Directory Name");
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryName(
				@"\\Directory Name\Subdir Name\",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"\\Directory Name\",
				@"Directory Name");
#endif

			// With extension
			CheckGetParentDirectoryName(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"C:\Directory Name\File Name.fileextension",
				@"Directory Name");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(
				@"C:\File Name.fileextension");
			CheckGetParentDirectoryName(
				@"Directory Name\Subdir Name\File Name.fileextension",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"Directory Name\File Name.fileextension",
				@"Directory Name");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(
				@"File Name.fileextension");
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryName(
				@"\\Directory Name\Subdir Name\File Name.fileextension",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"\\Directory Name\File Name.fileextension",
				@"Directory Name");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(
				@"\\File Name.fileextension");
#endif

			// Without extension
			CheckGetParentDirectoryName(
				@"C:\Directory Name\Subdir Name\File Name",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"C:\Directory Name\File Name",
				@"Directory Name");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(
				@"C:\File Name");
			CheckGetParentDirectoryName(
				@"Directory Name\Subdir Name\File Name",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"Directory Name\File Name",
				@"Directory Name");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(
				@"File Name");
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryName(
				@"\\Directory Name\Subdir Name\File Name",
				@"Subdir Name");
			CheckGetParentDirectoryName(
				@"\\Directory Name\File Name",
				@"Directory Name");
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(
				@"\\File Name");
#endif
		}

		private static void CheckGetParentDirectoryName(string path, string expected)
		{
			DoCheckGetParentDirectoryName(path, expected);
			//DoCheckGetParentDirectoryName(path.TrimAll(), expectedPath.TrimAll()); // TODO: Not sure about this. Decide if it should be included.
			DoCheckGetParentDirectoryName(path.FixDirectorySeparatorChars('\\'), expected.FixDirectorySeparatorChars('\\'));
			DoCheckGetParentDirectoryName(path.FixDirectorySeparatorChars('/'), expected.FixDirectorySeparatorChars('/'));
		}

		private static void CheckGetParentDirectoryNameThrows<T>(string path) where T : Exception
		{
			Assert.Throws<T>(() => DoCheckGetParentDirectoryName(path, null));
			//Assert.Throws<T>(() => DoCheckGetParentDirectoryName(path.TrimAll(), null)); // TODO: Not sure about this. Decide if it should be included.
			Assert.Throws<T>(() => DoCheckGetParentDirectoryName(path.FixDirectorySeparatorChars('\\'), null));
			Assert.Throws<T>(() => DoCheckGetParentDirectoryName(path.FixDirectorySeparatorChars('/'), null));
		}

		private static void DoCheckGetParentDirectoryName(string path, string expected)
		{
			Assert.AreEqual(expected, path.GetParentDirectoryName());
		}

		#endregion

		#region Path Info

		[Test]
		public static void SplitPath()
		{
			CheckSplitPathThrows<ArgumentNullException>(
				"",
				null,
				null,
				null);

			// With extension
			CheckSplitPath(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				@"C:\",
				@"Directory Name\Subdir Name\",
				@"File Name.fileextension");
			CheckSplitPath(
				@"C:\Directory Name\File Name.fileextension",
				@"C:\",
				@"Directory Name\",
				@"File Name.fileextension");
			CheckSplitPath(
				@"C:\File Name.fileextension",
				@"C:\",
				@"",
				@"File Name.fileextension");

			// Without extension
			CheckSplitPath(
				@"C:\Directory Name\Subdir Name\File Name",
				@"C:\",
				@"Directory Name\Subdir Name\",
				@"File Name");
			CheckSplitPath(
				@"C:\Directory Name\File Name",
				@"C:\",
				@"Directory Name\",
				@"File Name");
			CheckSplitPath(
				@"C:\File Name",
				@"C:\",
				@"",
				@"File Name");

			// Without filename
			CheckSplitPath(
				@"C:\Directory Name\Subdir Name\",
				@"C:\",
				@"Directory Name\Subdir Name\",
				@"");
			CheckSplitPath(
				@"C:\Directory Name\",
				@"C:\",
				@"Directory Name\",
				@"");
			CheckSplitPath(
				@"C:\",
				@"C:\",
				@"",
				@"");
		}

		private static void CheckSplitPath(string path, string expectedRoot, string expectedDirectoryWithoutRoot, string expectedFileName)
		{
			DoCheckSplitPath(path, expectedRoot, expectedDirectoryWithoutRoot, expectedFileName);
			DoCheckSplitPath(path.TrimAll(), expectedRoot.TrimAll(), expectedDirectoryWithoutRoot.TrimAll(), expectedFileName.TrimAll());
			DoCheckSplitPath(path.FixDirectorySeparatorChars('\\'), expectedRoot.FixDirectorySeparatorChars('\\'), expectedDirectoryWithoutRoot.FixDirectorySeparatorChars('\\'), expectedFileName.FixDirectorySeparatorChars('\\'));
			DoCheckSplitPath(path.FixDirectorySeparatorChars('/'), expectedRoot.FixDirectorySeparatorChars('/'), expectedDirectoryWithoutRoot.FixDirectorySeparatorChars('/'), expectedFileName.FixDirectorySeparatorChars('/'));
		}

		private static void CheckSplitPathThrows<T>(string path, string expectedRoot, string expectedDirectoryWithoutRoot, string expectedFileName) where T : Exception
		{
			Assert.Throws<T>(() => DoCheckSplitPath(path, expectedRoot, expectedDirectoryWithoutRoot, expectedFileName));
			Assert.Throws<T>(() => DoCheckSplitPath(path.TrimAll(), expectedRoot.TrimAll(), expectedDirectoryWithoutRoot.TrimAll(), expectedFileName.TrimAll()));
			Assert.Throws<T>(() => DoCheckSplitPath(path.FixDirectorySeparatorChars('\\'), expectedRoot.FixDirectorySeparatorChars('\\'), expectedDirectoryWithoutRoot.FixDirectorySeparatorChars('\\'), expectedFileName.FixDirectorySeparatorChars('\\')));
			Assert.Throws<T>(() => DoCheckSplitPath(path.FixDirectorySeparatorChars('/'), expectedRoot.FixDirectorySeparatorChars('/'), expectedDirectoryWithoutRoot.FixDirectorySeparatorChars('/'), expectedFileName.FixDirectorySeparatorChars('/')));
		}

		private static void DoCheckSplitPath(string path, string expectedRoot, string expectedDirectoryWithoutRoot, string expectedFileName)
		{
			path.SplitPath(out var root, out var directoryWithoutRoot, out var fileName);
			Assert.AreEqual(expectedRoot, root);
			Assert.AreEqual(expectedDirectoryWithoutRoot, directoryWithoutRoot);
			Assert.AreEqual(expectedFileName, fileName);
		}

		#endregion

		#region Path Info

		[Test]
		public static void AddFileExtension()
		{
			CheckAddFileExtension(
				@"C:\Directory Name\Subdir Name\File Name",
				".fileextension",
				true,
				@"C:\Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(
				@"C:\Directory Name\Subdir Name\File Name",
				".fileextension",
				false,
				@"C:\Directory Name\Subdir Name\File Name.fileextension");

			CheckAddFileExtension(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				".fileextension",
				true,
				@"C:\Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(
				@"C:\Directory Name\Subdir Name\File Name.fileextension",
				".fileextension",
				false,
				@"C:\Directory Name\Subdir Name\File Name.fileextension.fileextension");

			// Rootless
			CheckAddFileExtension(
				@"Directory Name\Subdir Name\File Name",
				".fileextension",
				true,
				@"Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(
				@"Directory Name\Subdir Name\File Name",
				".fileextension",
				false,
				@"Directory Name\Subdir Name\File Name.fileextension");

			CheckAddFileExtension(
				@"Directory Name\Subdir Name\File Name.fileextension",
				".fileextension",
				true,
				@"Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(
				@"Directory Name\Subdir Name\File Name.fileextension",
				".fileextension",
				false,
				@"Directory Name\Subdir Name\File Name.fileextension.fileextension");

			// Only filename
			CheckAddFileExtension(
				@"File Name",
				".fileextension",
				true,
				@"File Name.fileextension");
			CheckAddFileExtension(
				@"File Name",
				".fileextension",
				false,
				@"File Name.fileextension");

			CheckAddFileExtension(
				@"File Name.fileextension",
				".fileextension",
				true,
				@"File Name.fileextension");
			CheckAddFileExtension(
				@"File Name.fileextension",
				".fileextension",
				false,
				@"File Name.fileextension.fileextension");
		}

		private static void CheckAddFileExtension(string path, string extension, bool ignoreIfAlreadyThere, string expected)
		{
			DoAddFileExtension(path, extension, ignoreIfAlreadyThere, expected);
			DoAddFileExtension(path.TrimAll(), extension, ignoreIfAlreadyThere, expected.TrimAll());
			DoAddFileExtension(path.FixDirectorySeparatorChars('\\'), extension, ignoreIfAlreadyThere, expected.FixDirectorySeparatorChars('\\'));
			DoAddFileExtension(path.FixDirectorySeparatorChars('/'), extension, ignoreIfAlreadyThere, expected.FixDirectorySeparatorChars('/'));
		}

		private static void DoAddFileExtension(string path, string extension, bool ignoreIfAlreadyThere, string expected)
		{
			Assert.AreEqual(expected, path.AddFileExtension(extension, ignoreIfAlreadyThere));
		}

		#endregion
	}

}
