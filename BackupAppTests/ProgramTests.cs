using ConsoleBackupApp;


namespace BackupAppTests;
public class ProgramTests
{
    [Test]
    public void SingleQuoteTest()
    {
        // Single quoted argument should be parsed as one argument
        // Arrange
        string input = "add 'C:\\Program Files\\My App\\file.txt'";

        // Act
        string[] args = Program.GetArgs(input);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(args, Has.Length.EqualTo(2));
            Assert.That(args[0], Is.EqualTo("add"));
            Assert.That(args[1], Is.EqualTo("C:\\Program Files\\My App\\file.txt"));
        });
    }

    [Test]
    public void DoubleQuoteTest()
    {
        // Double quoted argument should be parsed as one argument
        // Arrange
        string input = "add \"C:\\Program Files\\My App\\file.txt\"";

        // Act
        string[] args = Program.GetArgs(input);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(args, Has.Length.EqualTo(2));
            Assert.That(args[0], Is.EqualTo("add"));
            Assert.That(args[1], Is.EqualTo("C:\\Program Files\\My App\\file.txt"));
        });
    }

    [Test]
    public void MixedQuotesTest()
    {
        // Both single and double quoted arguments
        // Arrange
        string input = "add \"C:\\Path With Spaces\\file.txt\" \'Another Path.txt\'";

        // Act
        string[] args = Program.GetArgs(input);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(args, Has.Length.EqualTo(3));
            Assert.That(args[0], Is.EqualTo("add"));
            Assert.That(args[1], Is.EqualTo("C:\\Path With Spaces\\file.txt"));
            Assert.That(args[2], Is.EqualTo("Another Path.txt"));
        });
    }

    [Test]
    public void NoQuotesArgumentsTest()
    {
        // No quotes, normal splitting
        // Arrange
        string input = "add C:\\Test\\file.txt";

        // Act
        string[] args = Program.GetArgs(input);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(args, Has.Length.EqualTo(2));
            Assert.That(args[0], Is.EqualTo("add"));
            Assert.That(args[1], Is.EqualTo("C:\\Test\\file.txt"));
        });
    }

    [Test]
    public void BadCommandTest()
    {
        // Arrange
        string[] strings= ["help2", "me"];

        // Act
        string result = Program.Command(strings);

        // Assert
        Assert.That(result, Is.EqualTo("Invalid Command"));
    }
}