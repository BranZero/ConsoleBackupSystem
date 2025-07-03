using ConsoleBackupApp.DataPaths;
using BackupAppTests.TestingTools;
using ConsoleBackupApp;

namespace BackupAppTests;

public class AppCommandsUpdateTest
{
    private string _currentPath;
    private string _testFilesFolder;
    private string _testSubFolder;
    private string _testFile;
    private const string _ignorePath = "test";

    [OneTimeSetUp]
    public void Setup()
    {
        _currentPath = Path.GetFullPath(Directory.GetCurrentDirectory());
        _currentPath += (_currentPath[^1] != Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : "";
        _testFilesFolder = _currentPath + "FileTest" + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_testFilesFolder);
        _testSubFolder = _testFilesFolder + "SubTest" + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_testSubFolder);
        _testFile = FileTools.CreateTestFile(_testFilesFolder, "update.txt", "updatecontent");

        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [SetUp]
    public void SetUp()
    {
        string[] addArgs = ["add", _testSubFolder, _ignorePath];
        Result a = AppCommands.Add(addArgs);
        string[] addArgs2 = ["add", "-c", _testFile];
        Result b = AppCommands.Add(addArgs2);
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
    public void Cleanup()
    {
        GC.Collect();
        Directory.Delete(_testFilesFolder, true);
        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [Test]
    public void UpdateCopyMode_ForceCopy_Success()
    {
        // Arrange
        string[] updateArgs = ["updatec", "-c", _testSubFolder];

        // Act
        var result = AppCommands.UpdateCopyMode(updateArgs);
        var dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            Assert.That(dataPaths[0].FileCopyMode, Is.EqualTo(CopyMode.ForceCopy));
        });
    }

    [Test]
    public void UpdateCopyMode_AllOrNone_Success()
    {
        // Arrange
        string[] updateArgs = ["updatec", "-a", _testSubFolder];

        // Act
        var result = AppCommands.UpdateCopyMode(updateArgs);
        var dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            Assert.That(dataPaths[0].FileCopyMode, Is.EqualTo(CopyMode.AllOrNone));
        });
    }

    [Test]
    public void UpdateCopyMode_None_Success()
    {
        // Arrange

        string[] updateArgs = ["updatec", _testFile];

        // Act
        var result = AppCommands.UpdateCopyMode(updateArgs);
        var dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            Assert.That(dataPaths[0].FileCopyMode, Is.EqualTo(CopyMode.None));
        });
    }

    [Test]
    public void UpdateCopyMode_InvalidPath_Failure()
    {
        // Arrange
        string[] updateArgs = ["updatec", "-c", "Z:\\DoesNotExist.txt"];

        // Act
        var result = AppCommands.UpdateCopyMode(updateArgs);

        // Assert
        Assert.That(result.ResultType, Is.EqualTo(ResultType.Not_Found));
    }

    [Test]
    public void UpdateCopyMode_InvalidOption_Failure()
    {
        // Arrange
        string[] updateArgs = ["updatec", "-x", _testFile];

        // Act
        var result = AppCommands.UpdateCopyMode(updateArgs);

        // Assert
        Assert.That(result.ResultType, Is.EqualTo(ResultType.Invalid_Option));
    }

    //Ignore Paths tests
    [Test]
    public void UpdateIgnorePaths_Add_Success()
    {
        // Arrange
        string[] updateArgs = ["updatei", "-a", _testSubFolder, "ignore1.txt", "ignore2.txt"];

        // Act
        var result = AppCommands.UpdateIgnorePaths(updateArgs);
        var dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            Assert.That(dataPaths[0].IgnorePaths, Is.Not.Null);
            Assert.That(dataPaths[0].IgnorePaths, Has.Length.EqualTo(3));
            Assert.That(dataPaths[0].IgnorePaths, Does.Contain("ignore1.txt"));
            Assert.That(dataPaths[0].IgnorePaths, Does.Contain("ignore2.txt"));
        });
    }

    [Test]
    public void UpdateIgnorePaths_Remove_Success()
    {
        // Arrange
        string[] updateArgs = ["updatei", "-r", _testSubFolder, _ignorePath];

        // Act
        Result result = AppCommands.UpdateIgnorePaths(updateArgs);
        DataPath[] dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Success));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            if (dataPaths[0].IgnorePaths != null) // both are acceptable results
            {
                Assert.That(dataPaths[0].IgnorePaths, Has.Length.EqualTo(0));
                Assert.That(dataPaths[0].IgnorePaths, Does.Not.Contain(_ignorePath));
            }
            else
            {
                Assert.That(dataPaths[0].IgnorePaths, Is.Null);
            }
        });
    }

    [Test]
    public void UpdateIgnorePaths_Remove_DontExist_Failure()
    {
        // Arrange
        string[] updateArgs = ["updatei", "-r", _testSubFolder, "left"];

        // Act
        Result result = AppCommands.UpdateIgnorePaths(updateArgs);
        DataPath[] dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Not_Found));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            Assert.That(dataPaths[0].IgnorePaths, Is.Not.Null);
            Assert.That(dataPaths[0].IgnorePaths, Has.Length.EqualTo(1));
            Assert.That(dataPaths[0].IgnorePaths, Does.Contain(_ignorePath));
        });
    }

        [Test]
    public void UpdateIgnorePaths_Add_Exist_Failure()
    {
        // Arrange
        string[] updateArgs = ["updatei", "-a", _testSubFolder, _ignorePath];

        // Act
        Result result = AppCommands.UpdateIgnorePaths(updateArgs);
        DataPath[] dataPaths = DataFileManager.GetDataPaths();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.ResultType, Is.EqualTo(ResultType.Exists));
            Assert.That(dataPaths, Has.Length.EqualTo(2));
            Assert.That(dataPaths[0].IgnorePaths, Is.Not.Null);
            Assert.That(dataPaths[0].IgnorePaths, Has.Length.EqualTo(1));
            Assert.That(dataPaths[0].IgnorePaths, Does.Contain(_ignorePath));
        });
    }

    [Test]
    public void UpdateIgnorePaths_Add_InvalidPath_Failure()
    {
        // Arrange
        string[] updateArgs = ["updatei", "-a", "Z:\\DoesNotExist.txt", "ignore.txt"];

        // Act
        var result = AppCommands.UpdateIgnorePaths(updateArgs);

        // Assert
        Assert.That(result.ResultType, Is.EqualTo(ResultType.Not_Found));
    }
}