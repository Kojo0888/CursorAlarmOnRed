using System;
using System.IO;
using CursorAlarmOnRed;
using Xunit;

public class AppConfigTests
{
    [Theory]
    [InlineData("Threshold=150", 150)]
    [InlineData("  Threshold = 77  \r\n", 77)]
    [InlineData("threshold=10", 10)]
    [InlineData("# comment\nThreshold=33", 33)]
    public void Parse_ValidText(string text, int expected) => Assert.Equal(expected, AppConfig.Parse(text));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("Threshold=")]
    [InlineData("Threshold=abc")]
    [InlineData("Threshold=-4")]
    [InlineData("Threshold=300")]
    public void Parse_BadText_GivesDefault(string text) => Assert.Equal(AppConfig.DefaultThreshold, AppConfig.Parse(text));

    [Fact] public void Format_RoundTripsThroughParse() => Assert.Equal(123, AppConfig.Parse(AppConfig.Format(123)));

    [Fact] public void Load_MissingFile_GivesDefault() =>
        Assert.Equal(AppConfig.DefaultThreshold, AppConfig.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".config")));

    [Fact] public void SaveThenLoad_RoundTrips()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".config");
        try
        {
            Assert.True(AppConfig.Save(path, 88));
            Assert.Equal(88, AppConfig.Load(path));
        }
        finally { File.Delete(path); }
    }

    [Fact] public void Save_UnwritablePath_ReturnsFalse_DoesNotThrow() =>
        Assert.False(AppConfig.Save(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "no", "dir", "x.config"), 5));
}
