using ConsoleBackupApp.PriorBackup;

namespace BackupAppTests;
public class BackupPriorPathTest
{
    private string _archiveFolder;
    private string _currentPath;
    private List<string> _testBackups;

    [OneTimeSetUp]
    public void SetUp()
    {
        _currentPath = Path.GetTempPath();
        _currentPath += (_currentPath[^1] != Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : "";

        _archiveFolder = _currentPath + "ArchiveTest" + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_archiveFolder);
        
        _testBackups = [];
        _testBackups.Add(_archiveFolder + "Backup_12_12_12_1" + Path.DirectorySeparatorChar);
        Directory.CreateDirectory(_testBackups[0]);
        _testBackups.Add(_archiveFolder + "Backup_1_1_1" + Path.DirectorySeparatorChar);//Doesn't exist test
        _testBackups.Add(_archiveFolder + "Backup_12_12" + Path.DirectorySeparatorChar);
        Directory.CreateDirectory(_testBackups[2]);
        _testBackups.Add(_archiveFolder + "NotABackupDirectory" + Path.DirectorySeparatorChar);
        Directory.CreateDirectory(_testBackups[3]);
    }

    [OneTimeTearDown]
    public void ConfigCleanup()
    {
        GC.Collect();
        Directory.Delete(_archiveFolder, true);
    }

    [Test]
    public void Found_PriorBackup()
    {
        // Act
        bool result = PriorBackupPath.TryGetPriorBackup(_testBackups[0], out PriorBackupPath priorBackupPath);
        DirectoryInfo directoryInfo = new(_testBackups[0]);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(_testBackups[0], Is.EqualTo(priorBackupPath.FullPath));
            Assert.That(directoryInfo.CreationTimeUtc, Is.EqualTo(priorBackupPath.CreationTime));
        }
    }

    [Test]
    public void NotFound_PriorBackup()
    {
        // Act
        bool result = PriorBackupPath.TryGetPriorBackup(_testBackups[1], out _);
        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Malformed_PriorBackup()
    {
        // Act
        bool result = PriorBackupPath.TryGetPriorBackup(_testBackups[2], out _);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Invalid_PriorBackup()
    {
        // Act
        bool result = PriorBackupPath.TryGetPriorBackup(_testBackups[3], out _);

        // Assert
        Assert.That(result, Is.False);
    }
}