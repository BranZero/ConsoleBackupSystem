using BackupAppTests.TestingTools;
using ConsoleBackupApp.Backup;
using ConsoleBackupApp.DataPaths;

namespace BackupAppTests;

[TestFixture]
public class BackupProcessTest
{
    private string _currentPath;
    private char _currentDrive;
    private string _archiveFolder;
    private string _testFilesFolder;
    private List<ArchiveQueue> _archiveQueues;
    private CancellationTokenSource _cancellationTokenSource;
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
    [SetUp]
    public void Setup()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _archiveQueues = [new(_currentDrive)];
    }

    [TearDown]
    public void TearDown()
    {
        _cancellationTokenSource.Dispose();
        GC.Collect();
    }

    [Test]
    public void Producer_EmptyDirectory()
    {
        // Arrange
        Queue<DataPath> dataPathsQueue = new Queue<DataPath>();
        dataPathsQueue.Enqueue(new DataPath(PathType.Directory,CopyMode.None, _testDirectories[4]));
        BackupShared backupShared = new(dataPathsQueue,_archiveQueues);
        BackupProcess backupProcess = new(backupShared, []);

        // Act
        backupProcess.Start(_cancellationTokenSource.Token); //with end if there are no DataPaths left
        ArchiveQueue archiveQueue = _archiveQueues.First();

        // Assert
        Assert.That(archiveQueue.PathsToCopy, Is.Empty);
    }

    [Test]
    public void Producer_DataPathWithInvalidPath()
    {
        // Arrange
        Queue<DataPath> dataPathsQueue = new Queue<DataPath>();
        dataPathsQueue.Enqueue(new DataPath(PathType.Directory,CopyMode.None, _testDirectories[1] + "adsjh"));
        BackupShared backupShared = new(dataPathsQueue,_archiveQueues);
        BackupProcess backupProcess = new(backupShared, []);

        // Act
        backupProcess.Start(_cancellationTokenSource.Token); //with end if there are no DataPaths left
        ArchiveQueue archiveQueue = _archiveQueues.First();

        // Assert
        Assert.That(archiveQueue.PathsToCopy, Is.Empty);
    }

    [Test]
    public void Producer_SingleFile()
    {
        // Arrange


        // Act & Assert

    }

    [Test]
    public void Producer_TwoDataPathsBothDirectories()
    {
        // Arrange


        // Act & Assert

    }
    [Test]
    public void Producer_ThreeDataPaths()
    {
        // Arrange


        // Act & Assert

    }

    [Test]
    public void Producer_TheWholeTestDirectory()
    {
        // Arrange

        // Act

        // Assert

    }
}