//#define PathToolsSupportDoubleBackslashPaths

using System;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.FileSystemToolbox
{

	public class Test_PathTools : ExtenityTestBase
	{
		#region Alter File Name

		[Test]
		public static void AddPrefixToFileName()
		{
			// With extension
			CheckPathWithPrefix(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                    @"Prefix-",
			                    @"C:\Directory Name\Subdir Name\{0}File Name.fileextension",
			                    CheckInWindows);

			CheckPathWithPrefix(@"\Directory Name\Subdir Name\File Name.fileextension",
			                    @"Prefix-",
			                    @"\Directory Name\Subdir Name\{0}File Name.fileextension",
			                    CheckInWindows);
			CheckPathWithPrefix(@"/Directory Name/Subdir Name/File Name.fileextension",
			                    @"Prefix-",
			                    @"/Directory Name/Subdir Name/{0}File Name.fileextension",
			                    CheckInWindowsAndUnix);
			CheckPathWithPrefix(@"Directory Name\Subdir Name\File Name.fileextension",
			                    @"Prefix-",
			                    @"Directory Name\Subdir Name\{0}File Name.fileextension",
			                    CheckInWindows);
			CheckPathWithPrefix(@"Directory Name/Subdir Name/File Name.fileextension",
			                    @"Prefix-",
			                    @"Directory Name/Subdir Name/{0}File Name.fileextension",
			                    CheckInWindowsAndUnix);

			CheckPathWithPrefix(@"C:\File Name.fileextension",
			                    @"Prefix-",
			                    @"C:\{0}File Name.fileextension",
			                    CheckInWindows);
			CheckPathWithPrefix(@"\File Name.fileextension",
			                    @"Prefix-",
			                    @"\{0}File Name.fileextension",
			                    CheckInWindows);
			CheckPathWithPrefix(@"/File Name.fileextension",
			                    @"Prefix-",
			                    @"/{0}File Name.fileextension",
			                    CheckInWindowsAndUnix);
			CheckPathWithPrefix(@"File Name.fileextension",
			                    @"Prefix-",
			                    @"{0}File Name.fileextension",
			                    CheckInWindowsAndUnix);

			// Without extension
			CheckPathWithPrefix(@"C:\Directory Name\Subdir Name\File Name",
			                    @"Prefix-",
			                    @"C:\Directory Name\Subdir Name\{0}File Name",
			                    CheckInWindows);

			CheckPathWithPrefix(@"\Directory Name\Subdir Name\File Name",
			                    @"Prefix-",
			                    @"\Directory Name\Subdir Name\{0}File Name",
			                    CheckInWindows);
			CheckPathWithPrefix(@"/Directory Name/Subdir Name/File Name",
			                    @"Prefix-",
			                    @"/Directory Name/Subdir Name/{0}File Name",
			                    CheckInWindowsAndUnix);
			CheckPathWithPrefix(@"Directory Name\Subdir Name\File Name",
			                    @"Prefix-",
			                    @"Directory Name\Subdir Name\{0}File Name",
			                    CheckInWindows);
			CheckPathWithPrefix(@"Directory Name/Subdir Name/File Name",
			                    @"Prefix-",
			                    @"Directory Name/Subdir Name/{0}File Name",
			                    CheckInWindowsAndUnix);

			CheckPathWithPrefix(@"C:\File Name",
			                    @"Prefix-",
			                    @"C:\{0}File Name",
			                    CheckInWindows);
			CheckPathWithPrefix(@"\File Name",
			                    @"Prefix-",
			                    @"\{0}File Name",
			                    CheckInWindows);
			CheckPathWithPrefix(@"/File Name",
			                    @"Prefix-",
			                    @"/{0}File Name",
			                    CheckInWindowsAndUnix);
			CheckPathWithPrefix(@"File Name",
			                    @"Prefix-",
			                    @"{0}File Name",
			                    CheckInWindowsAndUnix);
		}

		[Test]
		public static void AddSuffixToFileName()
		{
			// With extension
			CheckPathWithSuffix(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                    @"-Suffix",
			                    @"C:\Directory Name\Subdir Name\File Name{0}.fileextension",
			                    CheckInWindows);

			CheckPathWithSuffix(@"\Directory Name\Subdir Name\File Name.fileextension",
			                    @"-Suffix",
			                    @"\Directory Name\Subdir Name\File Name{0}.fileextension",
			                    CheckInWindows);
			CheckPathWithSuffix(@"/Directory Name/Subdir Name/File Name.fileextension",
			                    @"-Suffix",
			                    @"/Directory Name/Subdir Name/File Name{0}.fileextension",
			                    CheckInWindowsAndUnix);
			CheckPathWithSuffix(@"Directory Name\Subdir Name\File Name.fileextension",
			                    @"-Suffix",
			                    @"Directory Name\Subdir Name\File Name{0}.fileextension",
			                    CheckInWindows);
			CheckPathWithSuffix(@"Directory Name/Subdir Name/File Name.fileextension",
			                    @"-Suffix",
			                    @"Directory Name/Subdir Name/File Name{0}.fileextension",
			                    CheckInWindowsAndUnix);

			CheckPathWithSuffix(@"C:\File Name.fileextension",
			                    @"-Suffix",
			                    @"C:\File Name{0}.fileextension",
			                    CheckInWindows);
			CheckPathWithSuffix(@"\File Name.fileextension",
			                    @"-Suffix",
			                    @"\File Name{0}.fileextension",
			                    CheckInWindows);
			CheckPathWithSuffix(@"\File Name.fileextension",
			                    @"-Suffix",
			                    @"\File Name{0}.fileextension",
			                    CheckInWindowsAndUnix);
			CheckPathWithSuffix(@"File Name.fileextension",
			                    @"-Suffix",
			                    @"File Name{0}.fileextension",
			                    CheckInWindowsAndUnix);

			// Without extension
			CheckPathWithSuffix(@"C:\Directory Name\Subdir Name\File Name",
			                    @"-Suffix",
			                    @"C:\Directory Name\Subdir Name\File Name{0}",
			                    CheckInWindows);

			CheckPathWithSuffix(@"\Directory Name\Subdir Name\File Name",
			                    @"-Suffix",
			                    @"\Directory Name\Subdir Name\File Name{0}",
			                    CheckInWindows);
			CheckPathWithSuffix(@"/Directory Name/Subdir Name/File Name",
			                    @"-Suffix",
			                    @"/Directory Name/Subdir Name/File Name{0}",
			                    CheckInWindowsAndUnix);
			CheckPathWithSuffix(@"Directory Name\Subdir Name\File Name",
			                    @"-Suffix",
			                    @"Directory Name\Subdir Name\File Name{0}",
			                    CheckInWindows);
			CheckPathWithSuffix(@"Directory Name/Subdir Name/File Name",
			                    @"-Suffix",
			                    @"Directory Name/Subdir Name/File Name{0}",
			                    CheckInWindowsAndUnix);

			CheckPathWithSuffix(@"C:\File Name",
			                    @"-Suffix",
			                    @"C:\File Name{0}",
			                    CheckInWindows);
			CheckPathWithSuffix(@"\File Name",
			                    @"-Suffix",
			                    @"\File Name{0}",
			                    CheckInWindows);
			CheckPathWithSuffix(@"/File Name",
			                    @"-Suffix",
			                    @"/File Name{0}",
			                    CheckInWindowsAndUnix);
			CheckPathWithSuffix(@"File Name",
			                    @"-Suffix",
			                    @"File Name{0}",
			                    CheckInWindowsAndUnix);
		}

		private static void CheckPathWithPrefix(string path, string addition, string expectedPath, int platforms)
		{
			if (IsNotInPlatform(platforms))
				return;
			DoCheckPathWithPrefix(path,           addition,           expectedPath);
			DoCheckPathWithPrefix(path.TrimAll(), addition.TrimAll(), expectedPath.TrimAll());
		}

		private static void CheckPathWithSuffix(string path, string addition, string expectedPath, int platforms)
		{
			if (IsNotInPlatform(platforms))
				return;
			DoCheckPathWithSuffix(path,           addition,           expectedPath);
			DoCheckPathWithSuffix(path.TrimAll(), addition.TrimAll(), expectedPath.TrimAll());
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
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"C:\");
			CheckRemoveFirstDirectoryThrows<ArgumentNullException>("");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"\\");
#endif

			// Without filename
			CheckRemoveFirstDirectory(@"C:\Directory Name\Subdir Name\",
			                          @"C:\Subdir Name\");
			CheckRemoveFirstDirectory(@"C:\Directory Name\",
			                          @"C:\");
			CheckRemoveFirstDirectory(@"Directory Name\Subdir Name\",
			                          @"Subdir Name\");
			CheckRemoveFirstDirectory(@"Directory Name\",
			                          @"");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectory(@"\\Directory Name\Subdir Name\",
			                          @"\\Subdir Name\");
			CheckRemoveFirstDirectory(@"\\Directory Name\",
			                          @"\\");
#endif

			// With extension
			CheckRemoveFirstDirectory(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                          @"C:\Subdir Name\File Name.fileextension");
			CheckRemoveFirstDirectory(@"C:\Directory Name\File Name.fileextension",
			                          @"C:\File Name.fileextension");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"C:\File Name.fileextension");
			CheckRemoveFirstDirectory(@"Directory Name\Subdir Name\File Name.fileextension",
			                          @"Subdir Name\File Name.fileextension");
			CheckRemoveFirstDirectory(@"Directory Name\File Name.fileextension",
			                          @"File Name.fileextension");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"File Name.fileextension");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectory(@"\\Directory Name\Subdir Name\File Name.fileextension",
			                          @"\\Subdir Name\File Name.fileextension");
			CheckRemoveFirstDirectory(@"\\Directory Name\File Name.fileextension",
			                          @"\\File Name.fileextension");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"\\File Name.fileextension");
#endif

			// Without extension
			CheckRemoveFirstDirectory(@"C:\Directory Name\Subdir Name\File Name",
			                          @"C:\Subdir Name\File Name");
			CheckRemoveFirstDirectory(@"C:\Directory Name\File Name",
			                          @"C:\File Name");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"C:\File Name");
			CheckRemoveFirstDirectory(@"Directory Name\Subdir Name\File Name",
			                          @"Subdir Name\File Name");
			CheckRemoveFirstDirectory(@"Directory Name\File Name",
			                          @"File Name");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"File Name");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveFirstDirectory(@"\\Directory Name\Subdir Name\File Name",
			                          @"\\Subdir Name\File Name");
			CheckRemoveFirstDirectory(@"\\Directory Name\File Name",
			                          @"\\File Name");
			CheckRemoveFirstDirectoryThrows<InvalidOperationException>(@"\\File Name");
#endif
		}

		[Test]
		public static void RemoveLastDirectoryFromPath()
		{
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"C:\");
			CheckRemoveLastDirectoryThrows<ArgumentNullException>("");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"\\");
#endif

			// Without filename
			CheckRemoveLastDirectory(@"C:\Directory Name\Subdir Name\",
			                         @"C:\Directory Name\");
			CheckRemoveLastDirectory(@"C:\Directory Name\",
			                         @"C:\");
			CheckRemoveLastDirectory(@"Directory Name\Subdir Name\",
			                         @"Directory Name\");
			CheckRemoveLastDirectory(@"Directory Name\",
			                         @"");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectory(@"\\Directory Name\Subdir Name\",
			                         @"\\Directory Name\");
			CheckRemoveLastDirectory(@"\\Directory Name\",
			                         @"\\");
#endif

			// With extension
			CheckRemoveLastDirectory(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                         @"C:\Directory Name\File Name.fileextension");
			CheckRemoveLastDirectory(@"C:\Directory Name\File Name.fileextension",
			                         @"C:\File Name.fileextension");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"C:\File Name.fileextension");
			CheckRemoveLastDirectory(@"Directory Name\Subdir Name\File Name.fileextension",
			                         @"Directory Name\File Name.fileextension");
			CheckRemoveLastDirectory(@"Directory Name\File Name.fileextension",
			                         @"File Name.fileextension");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"File Name.fileextension");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectory(@"\\Directory Name\Subdir Name\File Name.fileextension",
			                         @"\\Directory Name\File Name.fileextension");
			CheckRemoveLastDirectory(@"\\Directory Name\File Name.fileextension",
			                         @"\\File Name.fileextension");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"\\File Name.fileextension");
#endif

			// Without extension
			CheckRemoveLastDirectory(@"C:\Directory Name\Subdir Name\File Name",
			                         @"C:\Directory Name\File Name");
			CheckRemoveLastDirectory(@"C:\Directory Name\File Name",
			                         @"C:\File Name");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"C:\File Name");
			CheckRemoveLastDirectory(@"Directory Name\Subdir Name\File Name",
			                         @"Directory Name\File Name");
			CheckRemoveLastDirectory(@"Directory Name\File Name",
			                         @"File Name");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"File Name");
#if PathToolsSupportDoubleBackslashPaths
			CheckRemoveLastDirectory(@"\\Directory Name\Subdir Name\File Name",
			                         @"\\Directory Name\File Name");
			CheckRemoveLastDirectory(@"\\Directory Name\File Name",
			                         @"\\File Name");
			CheckRemoveLastDirectoryThrows<InvalidOperationException>(@"\\File Name");
#endif
		}

		private static void CheckRemoveFirstDirectory(string path, string expectedPath)
		{
			DoCheckRemoveFirstDirectory(path,                                  expectedPath);
			DoCheckRemoveFirstDirectory(path.TrimAll(),                        expectedPath.TrimAll());
			DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('\\'), expectedPath.FixDirectorySeparatorChars('\\'));
			DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('/'),  expectedPath.FixDirectorySeparatorChars('/'));
		}

		private static void CheckRemoveFirstDirectoryThrows<T>(string path) where T : Exception
		{
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path,                                  null));
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path.TrimAll(),                        null));
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('\\'), null));
			Assert.Throws<T>(() => DoCheckRemoveFirstDirectory(path.FixDirectorySeparatorChars('/'),  null));
		}

		private static void CheckRemoveLastDirectory(string path, string expectedPath)
		{
			DoCheckRemoveLastDirectory(path,                                  expectedPath);
			DoCheckRemoveLastDirectory(path.TrimAll(),                        expectedPath.TrimAll());
			DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('\\'), expectedPath.FixDirectorySeparatorChars('\\'));
			DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('/'),  expectedPath.FixDirectorySeparatorChars('/'));
		}

		private static void CheckRemoveLastDirectoryThrows<T>(string path) where T : Exception
		{
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path,                                  null));
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path.TrimAll(),                        null));
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('\\'), null));
			Assert.Throws<T>(() => DoCheckRemoveLastDirectory(path.FixDirectorySeparatorChars('/'),  null));
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
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"C:\",
			                                                             CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"\",
			                                                             CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"/",
			                                                             CheckInWindowsAndUnix);
			CheckGetParentDirectoryNameThrows<ArgumentNullException>(@"",
			                                                         CheckInWindowsAndUnix);
			CheckGetParentDirectoryNameThrows<ArgumentNullException>(null,
			                                                         CheckInWindowsAndUnix);
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"\\",
			                                                             CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"//",
			                                                             CheckInWindowsAndUnix);
#endif

			// Without filename
			CheckGetParentDirectoryName(@"C:\Directory Name\Subdir Name\",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"C:\Directory Name\",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name\Subdir Name\",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name/Subdir Name/",
			                            @"Subdir Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryName(@"Directory Name\",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name/",
			                            @"Directory Name",
			                            CheckInWindowsAndUnix);
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryName(@"\\Directory Name\Subdir Name\",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"//Directory Name/Subdir Name/",
			                            @"Subdir Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryName(@"\\Directory Name\",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"//Directory Name/",
			                            @"Directory Name",
			                            CheckInWindows);
#endif

			// With extension
			CheckGetParentDirectoryName(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"C:\Directory Name\File Name.fileextension",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"C:\File Name.fileextension",
			                                                             CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name\Subdir Name\File Name.fileextension",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name/Subdir Name/File Name.fileextension",
			                            @"Subdir Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryName(@"Directory Name\File Name.fileextension",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name/File Name.fileextension",
			                            @"Directory Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"File Name.fileextension",
			                                                             CheckInWindowsAndUnix);
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryName(@"\\Directory Name\Subdir Name\File Name.fileextension",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"//Directory Name/Subdir Name/File Name.fileextension",
			                            @"Subdir Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryName(@"\\Directory Name\File Name.fileextension",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"//Directory Name/File Name.fileextension",
			                            @"Directory Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"\\File Name.fileextension",
			                                                             CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"//File Name.fileextension",
			                                                             CheckInWindowsAndUnix);
#endif

			// Without extension
			CheckGetParentDirectoryName(@"C:\Directory Name\Subdir Name\File Name",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"C:\Directory Name\File Name",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"C:\File Name",
			                                                             CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name\Subdir Name\File Name",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name/Subdir Name/File Name",
			                            @"Subdir Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryName(@"Directory Name\File Name",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"Directory Name/File Name",
			                            @"Directory Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"File Name",
			                                                             CheckInWindowsAndUnix);
#if PathToolsSupportDoubleBackslashPaths
			CheckGetParentDirectoryName(@"\\Directory Name\Subdir Name\File Name",
			                            @"Subdir Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"//Directory Name/Subdir Name/File Name",
			                            @"Subdir Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryName(@"\\Directory Name\File Name",
			                            @"Directory Name",
			                            CheckInWindows);
			CheckGetParentDirectoryName(@"//Directory Name/File Name",
			                            @"Directory Name",
			                            CheckInWindowsAndUnix);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"\\File Name",
			                                                             CheckInWindows);
			CheckGetParentDirectoryNameThrows<InvalidOperationException>(@"//File Name",
			                                                             CheckInWindowsAndUnix);
#endif
		}

		private static void CheckGetParentDirectoryName(string path, string expected, int platforms)
		{
			if (IsNotInPlatform(platforms))
				return;
			DoCheckGetParentDirectoryName(path, expected);
			//DoCheckGetParentDirectoryName(path.TrimAll(), expectedPath.TrimAll()); // TODO: Not sure about this. Decide if it should be included.
		}

		private static void CheckGetParentDirectoryNameThrows<T>(string path, int platforms) where T : Exception
		{
			if (IsNotInPlatform(platforms))
				return;
			Assert.Throws<T>(() => DoCheckGetParentDirectoryName(path, null));
			//Assert.Throws<T>(() => DoCheckGetParentDirectoryName(path.TrimAll(), null)); // TODO: Not sure about this. Decide if it should be included.
		}

		private static void DoCheckGetParentDirectoryName(string path, string expected)
		{
			Assert.AreEqual(expected, path.GetParentDirectoryName());
		}

		#endregion

		#region Split Path

		[Test]
		public static void SplitPath()
		{
			CheckSplitPathThrows<ArgumentNullException>("",
			                                            CheckInWindowsAndUnix);

			// With extension
			CheckSplitPath(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			               @"C:\",
			               @"Directory Name\Subdir Name\",
			               @"File Name.fileextension",
			               CheckInWindows);
			CheckSplitPath(@"\Directory Name\Subdir Name\File Name.fileextension",
			               @"\",
			               @"Directory Name\Subdir Name\",
			               @"File Name.fileextension",
			               CheckInWindows);
			CheckSplitPath(@"/Directory Name/Subdir Name/File Name.fileextension",
			               @"/",
			               @"Directory Name/Subdir Name/",
			               @"File Name.fileextension",
			               CheckInWindowsAndUnix);

			CheckSplitPath(@"C:\Directory Name\File Name.fileextension",
			               @"C:\",
			               @"Directory Name\",
			               @"File Name.fileextension",
			               CheckInWindows);
			CheckSplitPath(@"\Directory Name\File Name.fileextension",
			               @"\",
			               @"Directory Name\",
			               @"File Name.fileextension",
			               CheckInWindows);
			CheckSplitPath(@"/Directory Name/File Name.fileextension",
			               @"/",
			               @"Directory Name/",
			               @"File Name.fileextension",
			               CheckInWindowsAndUnix);

			CheckSplitPath(@"C:\File Name.fileextension",
			               @"C:\",
			               @"",
			               @"File Name.fileextension",
			               CheckInWindows);
			CheckSplitPath(@"\File Name.fileextension",
			               @"\",
			               @"",
			               @"File Name.fileextension",
			               CheckInWindows);
			CheckSplitPath(@"/File Name.fileextension",
			               @"/",
			               @"",
			               @"File Name.fileextension",
			               CheckInWindowsAndUnix);

			// Without extension
			CheckSplitPath(@"C:\Directory Name\Subdir Name\File Name",
			               @"C:\",
			               @"Directory Name\Subdir Name\",
			               @"File Name",
			               CheckInWindows);
			CheckSplitPath(@"\Directory Name\Subdir Name\File Name",
			               @"\",
			               @"Directory Name\Subdir Name\",
			               @"File Name",
			               CheckInWindows);
			CheckSplitPath(@"/Directory Name/Subdir Name/File Name",
			               @"/",
			               @"Directory Name/Subdir Name/",
			               @"File Name",
			               CheckInWindowsAndUnix);

			CheckSplitPath(@"C:\Directory Name\File Name",
			               @"C:\",
			               @"Directory Name\",
			               @"File Name",
			               CheckInWindows);
			CheckSplitPath(@"\Directory Name\File Name",
			               @"\",
			               @"Directory Name\",
			               @"File Name",
			               CheckInWindows);
			CheckSplitPath(@"/Directory Name/File Name",
			               @"/",
			               @"Directory Name/",
			               @"File Name",
			               CheckInWindowsAndUnix);

			CheckSplitPath(@"C:\File Name",
			               @"C:\",
			               @"",
			               @"File Name",
			               CheckInWindows);
			CheckSplitPath(@"\File Name",
			               @"\",
			               @"",
			               @"File Name",
			               CheckInWindows);
			CheckSplitPath(@"/File Name",
			               @"/",
			               @"",
			               @"File Name",
			               CheckInWindowsAndUnix);

			// Without filename
			CheckSplitPath(@"C:\Directory Name\Subdir Name\",
			               @"C:\",
			               @"Directory Name\Subdir Name\",
			               @"",
			               CheckInWindows);
			CheckSplitPath(@"\Directory Name\Subdir Name\",
			               @"\",
			               @"Directory Name\Subdir Name\",
			               @"",
			               CheckInWindows);
			CheckSplitPath(@"/Directory Name/Subdir Name/",
			               @"/",
			               @"Directory Name/Subdir Name/",
			               @"",
			               CheckInWindowsAndUnix);

			CheckSplitPath(@"C:\Directory Name\",
			               @"C:\",
			               @"Directory Name\",
			               @"",
			               CheckInWindows);
			CheckSplitPath(@"\Directory Name\",
			               @"\",
			               @"Directory Name\",
			               @"",
			               CheckInWindows);
			CheckSplitPath(@"/Directory Name/",
			               @"/",
			               @"Directory Name/",
			               @"",
			               CheckInWindowsAndUnix);

			CheckSplitPath(@"C:\",
			               @"C:\",
			               @"",
			               @"",
			               CheckInWindows);
			CheckSplitPath(@"\",
			               @"\",
			               @"",
			               @"",
			               CheckInWindows);
			CheckSplitPath(@"/",
			               @"/",
			               @"",
			               @"",
			               CheckInWindowsAndUnix);
		}

		private static void CheckSplitPath(string path, string expectedRoot, string expectedDirectoryWithoutRoot, string expectedFileName, int platforms)
		{
			if (IsNotInPlatform(platforms))
				return;
			DoCheckSplitPath(path,           expectedRoot,           expectedDirectoryWithoutRoot,           expectedFileName);
			DoCheckSplitPath(path.TrimAll(), expectedRoot.TrimAll(), expectedDirectoryWithoutRoot.TrimAll(), expectedFileName.TrimAll());
		}

		private static void CheckSplitPathThrows<T>(string path, int platforms) where T : Exception
		{
			if (IsNotInPlatform(platforms))
				return;
			Assert.Throws<T>(() => DoCheckSplitPath(path,           null, null, null));
			Assert.Throws<T>(() => DoCheckSplitPath(path.TrimAll(), null, null, null));
		}

		private static void DoCheckSplitPath(string path, string expectedRoot, string expectedDirectoryWithoutRoot, string expectedFileName)
		{
			path.SplitPath(out var root, out var directoryWithoutRoot, out var fileName);
			Assert.AreEqual(expectedRoot,                 root);
			Assert.AreEqual(expectedDirectoryWithoutRoot, directoryWithoutRoot);
			Assert.AreEqual(expectedFileName,             fileName);
		}

		#endregion

		#region Extension

		[Test]
		public static void AddFileExtension()
		{
			CheckAddFileExtension(@"C:\Directory Name\Subdir Name\File Name",
			                      ".fileextension",
			                      true,
			                      @"C:\Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(@"C:\Directory Name\Subdir Name\File Name",
			                      ".fileextension",
			                      false,
			                      @"C:\Directory Name\Subdir Name\File Name.fileextension");

			CheckAddFileExtension(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                      ".fileextension",
			                      true,
			                      @"C:\Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(@"C:\Directory Name\Subdir Name\File Name.fileextension",
			                      ".fileextension",
			                      false,
			                      @"C:\Directory Name\Subdir Name\File Name.fileextension.fileextension");

			// Rootless
			CheckAddFileExtension(@"Directory Name\Subdir Name\File Name",
			                      ".fileextension",
			                      true,
			                      @"Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(@"Directory Name\Subdir Name\File Name",
			                      ".fileextension",
			                      false,
			                      @"Directory Name\Subdir Name\File Name.fileextension");

			CheckAddFileExtension(@"Directory Name\Subdir Name\File Name.fileextension",
			                      ".fileextension",
			                      true,
			                      @"Directory Name\Subdir Name\File Name.fileextension");
			CheckAddFileExtension(@"Directory Name\Subdir Name\File Name.fileextension",
			                      ".fileextension",
			                      false,
			                      @"Directory Name\Subdir Name\File Name.fileextension.fileextension");

			// Only filename
			CheckAddFileExtension(@"File Name",
			                      ".fileextension",
			                      true,
			                      @"File Name.fileextension");
			CheckAddFileExtension(@"File Name",
			                      ".fileextension",
			                      false,
			                      @"File Name.fileextension");

			CheckAddFileExtension(@"File Name.fileextension",
			                      ".fileextension",
			                      true,
			                      @"File Name.fileextension");
			CheckAddFileExtension(@"File Name.fileextension",
			                      ".fileextension",
			                      false,
			                      @"File Name.fileextension.fileextension");
		}

		private static void CheckAddFileExtension(string path, string extension, bool ignoreIfAlreadyThere, string expected)
		{
			DoAddFileExtension(path,                                  extension, ignoreIfAlreadyThere, expected);
			DoAddFileExtension(path.TrimAll(),                        extension, ignoreIfAlreadyThere, expected.TrimAll());
			DoAddFileExtension(path.FixDirectorySeparatorChars('\\'), extension, ignoreIfAlreadyThere, expected.FixDirectorySeparatorChars('\\'));
			DoAddFileExtension(path.FixDirectorySeparatorChars('/'),  extension, ignoreIfAlreadyThere, expected.FixDirectorySeparatorChars('/'));
		}

		private static void DoAddFileExtension(string path, string extension, bool ignoreIfAlreadyThere, string expected)
		{
			Assert.AreEqual(expected, path.AddFileExtension(extension, ignoreIfAlreadyThere));
		}

		#endregion

		#region Platforms

		private static readonly int CheckInWindows        = 1 << 1;
		private static readonly int CheckInUnix           = 1 << 2;
		private static readonly int CheckInWindowsAndUnix = CheckInWindows | CheckInUnix;

		private static bool IsNotInPlatform(int platforms)
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					return (platforms & CheckInUnix) == 0;
				case PlatformID.Win32NT:
					return (platforms & CheckInWindows) == 0;
				default:
					throw new NotImplementedException();
			}
		}

		#endregion
	}

}