using ConsoleBackupApp;
using ConsoleBackupApp.DataPaths;

namespace BackupAppTests
{
    public class AppCommandsAddAndRemoveTests
    {
        [SetUp]
        public void Setup()
        {
            // Setup code if needed
        }

        [Test]
        public void Add_NoArguments_ReturnsNoArguments()
        {
            // Arrange
            string[] args = new string[0];

            // Act
            var result = AppCommands.Add(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Too_Few_Arguments));
        }

        [Test]
        public void Add_InvalidPath_ReturnsInvalidPath()
        {
            // Arrange
            string[] args = { "add", "Z:\\InvalidPath" };

            // Act
            var result = AppCommands.Add(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Invalid_Path));
        }

        [Test]
        public void Add_ForceValidFilePath_ReturnsSuccess()
        {
            // Arrange
            string[] args = { "add", "-f", "C:\\ValidFile.txt" };
            if (File.Exists(DataPathFile.DATA_PATH_FILE))
            {
                File.Delete(DataPathFile.DATA_PATH_FILE);
            }
            DataPath.Init(PathType.Directory, CopyMode.None, new ReadOnlySpan<string>(args, 2, 1), out DataPath dataPath);
            int expectedFileSize = DataPathFile.HEADER_SIZE + dataPath.ToDataRowSize();

            // Act
            var result = AppCommands.Add(args);
            FileInfo fileInfo = new FileInfo(DataPathFile.DATA_PATH_FILE);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(fileInfo.Length, Is.EqualTo(expectedFileSize));

            // Cleanup
            File.Delete(DataPathFile.DATA_PATH_FILE);
        }

        [Test]
        public void Add_ForceValidDirectoryPath_ReturnsSuccess()
        {
            // Arrange
            string[] args = { "add", "-f", "C:\\ValidDirectory\\" };
            if (File.Exists(DataPathFile.DATA_PATH_FILE))
            {
                File.Delete(DataPathFile.DATA_PATH_FILE);
            }
            DataPath.Init(PathType.Directory, CopyMode.None, new ReadOnlySpan<string>(args, 2, 1), out DataPath dataPath);
            int expectedFileSize = DataPathFile.HEADER_SIZE + dataPath.ToDataRowSize();

            // Act
            var result = AppCommands.Add(args);
            FileInfo fileInfo = new FileInfo(DataPathFile.DATA_PATH_FILE);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(fileInfo.Length, Is.EqualTo(expectedFileSize));

            // Cleanup
            File.Delete(DataPathFile.DATA_PATH_FILE);
        }

        [Test]
        public void Add_ForceValidDirectoryPathWithIgnore_ReturnsSuccess()
        {
            // Arrange
            string[] args = { "add", "-f", "C:\\ValidDirectory\\", "C:\\ValidDirectory\\bin\\" };
            if (File.Exists(DataPathFile.DATA_PATH_FILE))
            {
                File.Delete(DataPathFile.DATA_PATH_FILE);
            }
            DataPath.Init(PathType.Directory, CopyMode.None, new ReadOnlySpan<string>(args, 2, 2), out DataPath dataPath);
            int expectedFileSize = DataPathFile.HEADER_SIZE + dataPath.ToDataRowSize();

            // Act
            var result = AppCommands.Add(args);
            FileInfo fileInfo = new FileInfo(DataPathFile.DATA_PATH_FILE);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(fileInfo.Length, Is.EqualTo(expectedFileSize));

            // Cleanup
            File.Delete(DataPathFile.DATA_PATH_FILE);
        }

        [Test]
        public void Add_ForcePathAlreadyExists_ReturnsExists()
        {
            // Arrange
            string[] args = { "add", "-f", "C:\\ExistingPath\\" };
            if (File.Exists(DataPathFile.DATA_PATH_FILE))
            {
                File.Delete(DataPathFile.DATA_PATH_FILE);
            }

            // Act
            var result = AppCommands.Add(args);
            var result2 = AppCommands.Add(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(result2, Is.EqualTo(Result.Exists));
        }

        [Test]
        public void Remove_NoArguments_ReturnsNoArguments()
        {
            // Arrange
            string[] args = new string[0];

            // Act
            var result = AppCommands.Remove(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Too_Few_Arguments));
        }

        [Test]
        public void Remove_TooManyArguments_ReturnsTooManyArguments()
        {
            // Arrange
            string[] args = { "remove", "C:\\Path1", "C:\\Path2" };

            // Act
            var result = AppCommands.Remove(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Too_Many_Arguments));
        }

        [Test]
        public void Remove_InvalidOption_ReturnsInvalidOption()
        {
            // Arrange
            string[] args = { "remove", "-x" };

            // Act
            var result = AppCommands.Remove(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Invalid_Option));
        }

        [Test]
        public void Remove_ValidPathDoesNotExist_ReturnsFailure()
        {
            // Arrange
            string[] args = { "remove", "C:\\NonExistentPath\\" };

            // Act
            var result = AppCommands.Remove(args);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Failure));
        }

        [Test]
        public void Remove_ValidPathExists_ReturnsSuccess()
        {
            // Arrange
            string[] args = { "remove", "-f", "C:\\ExistingPath\\" };
            string[] args2 = { "remove", "C:\\ExistingPath\\" };
            if (File.Exists(DataPathFile.DATA_PATH_FILE))
            {
                File.Delete(DataPathFile.DATA_PATH_FILE);
            }

            // Act
            var result = AppCommands.Add(args);
            var result2 = AppCommands.Remove(args2);
            FileInfo fileInfo = new FileInfo(DataPathFile.DATA_PATH_FILE);

            // Assert
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(result2, Is.EqualTo(Result.Success));
            Assert.That(fileInfo.Length, Is.EqualTo(DataPathFile.HEADER_SIZE));
        }
    }
}