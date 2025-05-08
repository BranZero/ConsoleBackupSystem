using System;
using ConsoleBackupApp.DataPaths;

namespace BackupAppTests
{
    public class DataPathFileTestsTest
    {
        [Test]
        public void DataPathReversableTest()
        {
            // Arrange
            DataPath[] dataPaths = [
                new DataPath(PathType.Directory, CopyMode.None, "C:\\TestPath1\\SubPath1\\"),
                new DataPath(PathType.File, CopyMode.ForceCopy, "A:\\TestPath2.txt"),
            ];

            // Act
            byte[] data = DPF.CreateFile(dataPaths);
            DataPath[] dataPaths2 = DPF.GetDataPaths(data);

            // Assert
            for (int i = 0; i < dataPaths2.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(dataPaths[i].Drive, Is.EqualTo(dataPaths2[i].Drive));
                    Assert.That(dataPaths[i].Type, Is.EqualTo(dataPaths2[i].Type));
                    Assert.That(dataPaths[i].FileCopyMode, Is.EqualTo(dataPaths2[i].FileCopyMode));
                    Assert.That(dataPaths[i].SourcePath, Is.EqualTo(dataPaths2[i].SourcePath));
                });
            }
        }
    }
}