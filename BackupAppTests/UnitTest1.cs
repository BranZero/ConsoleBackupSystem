using ConsoleBackupApp;


namespace BackupAppTests;
public class Tests
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void AddTest()
    {
        string result = AppCommands.Add(null).ToString();


        Assert.That(result, Is.EqualTo("Failure"));
    }

    [Test]
    public void RemoveTest()
    {
        
    }

    [Test]
    public void BackupTest()
    {
        
    }

    [Test]
    public void BadCommandTest()
    {
        string[] strings= ["help2", "me"];
        string result = Program.Command(strings);

        Assert.That(result, Is.EqualTo("Invalid Command"));
    }
}