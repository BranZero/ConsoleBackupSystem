using BackupAppTests.TestingTools;
using ConsoleBackupApp;
using ConsoleBackupApp.DataPaths;

namespace BackupAppTests;

public class AppCommandsAddAndRemoveTests
{
    private string _currentPath;
    private string _testFilesFolder;
    private string _testFile;
    private string _testFileSpace;
    [OneTimeSetUp]
    public void Setup()
    {
        _currentPath = Path.GetFullPath(Directory.GetCurrentDirectory());
        _currentPath += (_currentPath[^1] != Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : "";
        _testFilesFolder = _currentPath + "FileTest" + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_testFilesFolder);
        _testFile = FileTools.CreateTestFile(_testFilesFolder, "test.txt", "nothing");
        _testFileSpace = FileTools.CreateTestFile(_testFilesFolder, "test space.txt", "nothingtwo");

        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [OneTimeTearDown]
    public void ConfigCleanup()
    {
        GC.Collect();
        Directory.Delete(_testFilesFolder, true);
        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [Test]
    public void Add_NoArguments_ReturnsNoArguments()
    {
        // Arrange
        string[] args = [];

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

        DataPath.Init(PathType.Directory, CopyMode.None, new ReadOnlySpan<string>(args, 2, 1), out DataPath dataPath);
        int expectedFileSize = DPF.HEADER_SIZE + DPF.ToDataRowSize(dataPath);

        // Act
        var result = AppCommands.Add(args);
        FileInfo fileInfo = new FileInfo(DataFileManager.DATA_PATH_FILE);

        // Assert
        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(fileInfo.Length, Is.EqualTo(expectedFileSize));
    }

    [Test]
    public void Add_CopyModeFile()
    {
        // Arrange
        string[] args = ["add", "-c", _testFile];

        // Act
        var result = AppCommands.Add(args);
        DataPath[] dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(1));
            Assert.That(dataPaths[0].FileCopyMode, Is.EqualTo(CopyMode.ForceCopy));
            Assert.That(dataPaths[0].SourcePath, Is.EqualTo(_testFile));
        });
    }

    [Test]
    public void Add_TwoCopyMode_InvalidOption()
    {
        // Arrange
        string[] args = ["add", "-ac", _testFile];

        // Act
        var result = AppCommands.Add(args);
        DataPath[] dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Invalid_Option));
            Assert.That(dataPaths, Has.Length.EqualTo(0));
        });
    }

    [Test]
    public void Add_ForceValidDirectoryPath_ReturnsSuccess()
    {
        // Arrange
        string[] args = { "add", "-f", "C:\\ValidDirectory\\" };

        DataPath.Init(PathType.Directory, CopyMode.None, new ReadOnlySpan<string>(args, 2, 1), out DataPath dataPath);
        int expectedFileSize = DPF.HEADER_SIZE + DPF.ToDataRowSize(dataPath);

        // Act
        var result = AppCommands.Add(args);
        FileInfo fileInfo = new FileInfo(DataFileManager.DATA_PATH_FILE);

        // Assert
        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(fileInfo.Length, Is.EqualTo(expectedFileSize));
    }

    [Test]
    public void Add_ValidDirectory_WithIgnore_ReturnsSuccess()
    {
        // Arrange
        string[] args = { "add", "-a", _testFilesFolder, "test space.txt" };

        // Act
        var result = AppCommands.Add(args);
        DataPath[] dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(1));
            Assert.That(dataPaths[0].IgnorePaths, Has.Length.EqualTo(1));
            Assert.That(dataPaths[0].IgnorePaths[0], Is.Not.Null);
            Assert.That(dataPaths[0].IgnorePaths[0], Is.EqualTo("test space.txt"));
        });

    }

    [Test]
    public void Add_ForcePathAlreadyExists_ReturnsExists()
    {
        // Arrange
        string[] args = { "add", "-f", "C:\\ExistingPath\\" };

        // Act
        var result = AppCommands.Add(args);
        var result2 = AppCommands.Add(args);

        // Assert
        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(result2, Is.EqualTo(Result.SubPath_Or_SamePath));
    }

    [Test]
    public void Add_DuplicatePaths_ReturnsExists()
    {
        // Arrange
        string[] args = { "add", "-f", "C:\\ExistingPath\\" };
        string[] args2 = { "add", "-f", "C:\\ExistingPath\\" };


        // Act
        var result = AppCommands.Add(args);
        var result2 = AppCommands.Add(args);

        // Assert
        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(result2, Is.EqualTo(Result.SubPath_Or_SamePath));
    }

    [Test]
    public void Add_SubPath_ReturnsExists()
    {
        // Arrange
        string[] args = { "add", "-f", "C:\\ExistingPath\\" };
        string[] args2 = { "add", "-f", "C:\\ExistingPath\\SubFile.txt " };

        // Act
        var result = AppCommands.Add(args);
        var result2 = AppCommands.Add(args);

        // Assert
        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(result2, Is.EqualTo(Result.SubPath_Or_SamePath));
    }

    [Test]
    public void Remove_NoArguments_ReturnsNoArguments()
    {
        // Arrange
        string[] args = [];

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

        // Act
        var result = AppCommands.Add(args);
        var result2 = AppCommands.Remove(args2);
        FileInfo fileInfo = new FileInfo(DataFileManager.DATA_PATH_FILE);

        // Assert
        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(result2, Is.EqualTo(Result.Success));
        Assert.That(fileInfo.Length, Is.EqualTo(DPF.HEADER_SIZE));
    }
}