namespace Eip.Tests;

public sealed class FoundationTests
{
    [Fact]
    public void CliStartsSuccessfully()
    {
        var exitCode = Cli.Program.Main();

        Assert.Equal(0, exitCode);
    }
}
