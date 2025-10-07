using System.IO.Compression;
using BackupAppTests.TestingTools;
using ConsoleBackupApp;
using ConsoleBackupApp.DataPaths;

namespace BackupAppTests;

public class MergeCommandTests
{
    private string _currentPath;
    private char _currentDrive;
    private string _archiveFolder;
    private string _testFilesFolder;
    private List<string> _testFiles;
    private List<string> _testDirectories;
    [OneTimeSetUp]
    public void ConfigSetup()
    {
        _currentPath = Path.GetFullPath(Directory.GetCurrentDirectory());
        _currentPath += (_currentPath[^1] != Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : "";
        _currentDrive = _currentPath[0];

        _archiveFolder = _currentPath + "ArchiveTest" + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_archiveFolder);
        _testFilesFolder = _currentPath + "FileTest" + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_testFilesFolder);

        // Create test files with content
        _testFiles = FileTools.CreateTestDirectories(_testFilesFolder, out List<string> testDirectories);
        _testDirectories = testDirectories;

        //Ensure no DATA_PATH_FILE exists at start or could cause unexpected results
        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [OneTimeTearDown]
    public void ConfigCleanup()
    {
        GC.Collect();
        Directory.Delete(_archiveFolder, true);
        Directory.Delete(_testFilesFolder, true);
    }
    [SetUp]
    public void SetUp()
    {
        //1
        string[] addArgs = ["add", _testFiles[1]];
        _ = AppCommands.Add(addArgs);
        string[] backupArgs = ["backup", _archiveFolder];
        _ = AppCommands.Backup(backupArgs);
        //2
        Thread.Sleep(1000);
        string[] addArgs2 = ["add", "-c", _testDirectories[3]];
        _ = AppCommands.Add(addArgs2);
        _ = AppCommands.Backup(backupArgs);
        //3
        Thread.Sleep(3000);
        string[] removeArgs = ["remove", _testDirectories[3]];
        _ = AppCommands.Remove(removeArgs);
        string[] addArgs3 = ["add", _testDirectories[2]];
        _ = AppCommands.Add(addArgs3);
        _ = AppCommands.Backup(backupArgs);
    }

    [TearDown]
    public void TearDown()
    {
        GC.Collect();
        if (File.Exists(DataFileManager.DATA_PATH_FILE))
        {
            File.Delete(DataFileManager.DATA_PATH_FILE);
        }
    }

    [Test]
    public void MergeTest()
    {
        // Arrange
        List<string> mergeArgsList = ["merge"];
        DirectoryInfo dir = new(_archiveFolder);
        foreach (var prior in dir.GetDirectories())
        {
            mergeArgsList.Add(prior.FullName);
        }
        string[] mergeArgs = [.. mergeArgsList];

        // Act
        Result r = AppCommands.Merge(mergeArgs);
        GC.Collect();
        string zipPath = dir.GetDirectories()[0].GetFiles()[0].FullName;
        using ZipArchive zipArchive = ZipFile.OpenRead(zipPath);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(r.ResultType, Is.EqualTo(ResultType.Success));
            Assert.That(dir.GetDirectories(), Has.Length.EqualTo(1));
            Assert.That(File.Exists(zipPath));
            Assert.That(zipArchive.Entries, Has.Count.EqualTo(6));
        });
    }
}