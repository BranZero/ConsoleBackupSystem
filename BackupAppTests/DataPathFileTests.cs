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
            DataPath[] dataPaths = new DataPath[2];
            dataPaths[0].Drive = 'C';
            dataPaths[0].Type = 'd';
            dataPaths[0].Path = "C:\\TestPath1\\SubPath1\\";
            dataPaths[1].Drive = 'A';
            dataPaths[1].Type = 'f';
            dataPaths[1].Path = "A:\\TestPath2.txt";

            // Act
            byte[] data = DataPathFile.CreateDpfFile(dataPaths);
            DataPath[] dataPaths2 = DataPathFile.GetDataPaths(data);

            // Assert
            for (int i = 0; i < dataPaths2.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(dataPaths[i].Drive, Is.EqualTo(dataPaths2[i].Drive));
                    Assert.That(dataPaths[i].Type, Is.EqualTo(dataPaths2[i].Type));
                    Assert.That(dataPaths[i].Path, Is.EqualTo(dataPaths2[i].Path));
                });
            }
            
            // Cleanup
        }
    }
}