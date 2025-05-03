using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BackupAppTests.TestingTools;

public static class FileTools
{
    // Creates a temporary file with the specified content
    public static void CreateLargeTestFile(string path)
    {
        byte[] temp = new byte[256];
        for (short i = 0; i < temp.Length; i++)
        {
            temp[i] = (byte)i;
        }
        using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        for (int i = 0; i < short.MaxValue; i++)
        {
            fs.Write(temp);
        }
    }

    // Creates a temporary file with the specified content
    public static void CreateTestFile(string path, string content)
    {
        using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
    }

    // Checks if the content of a file matches the specified string
    public static void TestDoFilesMatch(string fileOriginalPath, string zipFile)
    {
        string originalContent = File.ReadAllText(fileOriginalPath);
        
        string relativeFilePathInZip = fileOriginalPath[3..];

        using ZipArchive zipArchive = ZipFile.Open(zipFile,ZipArchiveMode.Read);
        ZipArchiveEntry? entry = zipArchive.GetEntry(relativeFilePathInZip);

        Assert.That(entry, Is.Not.Null);

        using Stream stream = entry.Open();
        using StreamReader reader = new StreamReader(stream);
        string outputContent = reader.ReadToEnd();

        Assert.That(originalContent, Is.EqualTo(outputContent));
    }
}
