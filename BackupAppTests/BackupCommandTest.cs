using System.IO.Compression;
using BackupAppTests.TestingTools;
using ConsoleBackupApp;
using ConsoleBackupApp.Backup;
using ConsoleBackupApp.DataPaths;

namespace BackupAppTests;

public class BackupCommandTest
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
    }

    [OneTimeTearDown]
    public void ConfigCleanup()
    {
        GC.Collect();
        Directory.Delete(_archiveFolder, true);
        Directory.Delete(_testFilesFolder, true);
    }

    [TearDown]
    public void TearDown()
    {
        GC.Collect();
    }

    [Test]
    public void BackupSingle_DataPath()
    {
        // Arrange
        string folder = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath = Path.Combine(folder, _currentDrive + ".zip");

        List<DataPath> dataPaths = new();
        dataPaths.Add(new DataPath(PathType.File, CopyMode.None, _testFiles[1]));

        // Act
        BackupController backupController = new(folder, dataPaths, []);
        Result result = backupController.Start();
        using ZipArchive zipArchive = ZipFile.OpenRead(zipPath);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(Directory.Exists(folder), Is.True);
            Assert.That(File.Exists(zipPath));

            Assert.That(zipArchive.Entries, Has.Count.EqualTo(1));
            FileTools.TestDoFilesMatch(_testFiles[1], zipPath);
        });
    }

    [Test]
    public void BackupEmpty_DataPath()
    {
        // Arrange
        string folder = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath = Path.Combine(folder, _currentDrive + ".zip");

        List<DataPath> dataPaths = new();

        // Act
        BackupController backupController = new(folder, dataPaths, []);
        Result result = backupController.Start();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Empty));
            Assert.That(Directory.Exists(folder), Is.False);
            Assert.That(File.Exists(zipPath), Is.False);
        });
    }

    [Test]
    public void BackupMultiple_DataPath()
    {
        // Arrange
        string folder = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath = Path.Combine(folder, _currentDrive + ".zip");

        List<DataPath> dataPaths = new();
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[1]));
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[3]));
        dataPaths.Add(new DataPath(PathType.File, CopyMode.None, _testFiles[0]));
        dataPaths.Add(new DataPath(PathType.File, CopyMode.None, _testFiles[1]));
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[2]));

        // Act
        BackupController backupController = new(folder, dataPaths, []);
        Result result = backupController.Start();
        using ZipArchive zipArchive = ZipFile.OpenRead(zipPath);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(Directory.Exists(folder), Is.True);
            Assert.That(File.Exists(zipPath));

            Assert.That(zipArchive.Entries, Has.Count.EqualTo(22));
            FileTools.TestDoFilesMatch(_testFiles[3], zipPath);
            FileTools.TestDoFilesMatch(_testFiles[21], zipPath);
        });
    }

    [Test]
    public void BackupTwice_WithPriorBackups_DataPath()
    {
        // 1st Arrange
        string folder = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath = Path.Combine(folder, _currentDrive + ".zip");

        List<DataPath> dataPaths = new();
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[1]));
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[3]));
        dataPaths.Add(new DataPath(PathType.File, CopyMode.None, _testFiles[0]));
        dataPaths.Add(new DataPath(PathType.File, CopyMode.None, _testFiles[1]));
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[2]));

        //1st Act
        BackupController backupController = new(folder, dataPaths, []);
        Result result = backupController.Start();

        //2nd Arrange
        string folder2 = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath2 = Path.Combine(folder, _currentDrive + ".zip");
        List<string> priorBackups = new();
        priorBackups.Add(folder);

        //2nd Act
        BackupController backupController2 = new(folder2, dataPaths, priorBackups);
        Result result2 = backupController2.Start();
        using ZipArchive zipArchive = ZipFile.OpenRead(zipPath2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(Directory.Exists(folder), Is.True);
            Assert.That(File.Exists(zipPath));

            Assert.That(zipArchive.Entries, Has.Count.EqualTo(0)); //Prior Backups is yet to be implemented
        });
    }

    [Test]
    public void BackupTwice_With_PriorBackups_ForceCopy_DataPath()
    {
        // 1st Arrange
        string folder = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath = Path.Combine(folder, _currentDrive + ".zip");

        List<DataPath> dataPaths = new();
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[1]));
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[3]));
        dataPaths.Add(new DataPath(PathType.File, CopyMode.None, _testFiles[0]));
        dataPaths.Add(new DataPath(PathType.File, CopyMode.ForceCopy, _testFiles[1]));
        dataPaths.Add(new DataPath(PathType.Directory, CopyMode.None, _testDirectories[2]));

        //1st Act
        BackupController backupController = new(folder, dataPaths, []);
        Result result = backupController.Start();

        //2nd Arrange
        string folder2 = BackupCommandHelper.FindBackupPathName(_archiveFolder);
        string zipPath2 = Path.Combine(folder, _currentDrive + ".zip");
        List<string> priorBackups = new();
        priorBackups.Add(folder);

        //2nd Act
        BackupController backupController2 = new(folder2, dataPaths, priorBackups);
        Result result2 = backupController2.Start();
        using ZipArchive zipArchive = ZipFile.OpenRead(zipPath2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(Result.Success));
            Assert.That(Directory.Exists(folder), Is.True);
            Assert.That(File.Exists(zipPath));

            Assert.That(zipArchive.Entries, Has.Count.EqualTo(1)); //Prior Backups is yet to be implemented
            FileTools.TestDoFilesMatch(_testFiles[1], zipPath);
        });
    }
}