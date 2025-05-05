using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace BackupAppTests.TestingTools;

public static class FileTools
{
    // Creates a temporary file with the specified content
    public static string CreateLargeTestFile(string path, string name)
    {
        string fullPath = Path.Combine(path, name);
        byte[] temp = new byte[256];
        for (short i = 0; i < temp.Length; i++)
        {
            temp[i] = (byte)i;
        }
        using FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        for (int i = 0; i < short.MaxValue; i++)
        {
            fs.Write(temp);
        }
        return fullPath;
    }

    // Creates a temporary file with the specified content and returns its fullPath
    public static string CreateTestFile(string path, string name, string content)
    {
        string fullPath = Path.Combine(path, name);
        using FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write(content);
        return fullPath;
    }

    // Checks if the content of a file matches the specified string
    public static void TestDoFilesMatch(string fileOriginalPath, string zipFile)
    {
        string originalContent = File.ReadAllText(fileOriginalPath);

        string relativeFilePathInZip = fileOriginalPath[3..];

        using ZipArchive zipArchive = ZipFile.Open(zipFile, ZipArchiveMode.Read);
        ZipArchiveEntry? entry = zipArchive.GetEntry(relativeFilePathInZip);

        Assert.That(entry, Is.Not.Null);

        using Stream stream = entry.Open();
        using StreamReader reader = new StreamReader(stream);
        string outputContent = reader.ReadToEnd();

        Assert.That(originalContent, Is.EqualTo(outputContent));
    }

    //Creates a directory and returns its path
    public static string CreateDirectory(string parentDir, string name)
    {
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(parentDir, name));
        return directoryInfo.FullName;
    }

    //Create a default testing enviroment
    public static List<string> CreateTestDirectories(string _testFilesFolder, out List<string> directoryPaths)
    {
        List<string> testFiles = new();
        directoryPaths = new();
        //root directory files
        testFiles.Add(CreateLargeTestFile(_testFilesFolder, "File1.txt"));//0
        testFiles.Add(CreateTestFile(_testFilesFolder, "FileSmall.txt", "small test file ☺"));//1

        //root directory sub directories
        string sub1 = CreateDirectory(_testFilesFolder,"sub1");
        directoryPaths.Add(sub1);
        string sub2 = CreateDirectory(_testFilesFolder,"sub2");
        directoryPaths.Add(sub2);

        //Sub1 directory sub directories
        string sub3 = CreateDirectory(sub1,"sub3");
        directoryPaths.Add(sub3);
        string sub4 = CreateDirectory(sub1,"sub4");
        directoryPaths.Add(sub4);

        //Sub2 directory files
        string context = "Start:";
        for (int i = 0; i < 15; i++)
        {
            context += Encoding.UTF8.GetString(SHA1.HashData(Encoding.UTF8.GetBytes(context)));
            testFiles.Add(CreateTestFile(sub2, $"HashFile{i}.hex", context));
        }

        //Sub3 directory sub directories
        string sub5 = CreateDirectory(sub3,"sub5");//empty directory
        directoryPaths.Add(sub5);

        //Sub3 directory files
        string context1 = "Hello,World\nHello,World2\n";
        testFiles.Add(CreateTestFile(sub3, $"hw.txt", context1));

        //Sub5 directory sub directories
        string sub6 = CreateDirectory(sub4,"sub6");
        directoryPaths.Add(sub6);

        //Sub6 directory files
        string context2 = "End:";
        for (int i = 0; i < 4; i++)
        {
            context += Encoding.UTF8.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(context2)));
            testFiles.Add(CreateTestFile(sub6, $"HashFile{i}.hex", context2));
        }

        return testFiles;
    }
}
