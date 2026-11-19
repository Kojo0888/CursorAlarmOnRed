using CursorAlarmOnRed;
using Xunit;

public class RedDetectorTests
{
    [Fact] public void PureRed_AboveThreshold_IsRed() => Assert.True(RedDetector.IsRed(255, 0, 0, 200));

    [Fact] public void RedExactlyAtThreshold_IsRed() => Assert.True(RedDetector.IsRed(200, 0, 0, 200));

    [Fact] public void RedJustBelowThreshold_IsNotRed() => Assert.False(RedDetector.IsRed(199, 0, 0, 200));

    [Fact] public void DarkRed_OnlyWithLowThreshold()
    {
        Assert.True(RedDetector.IsRed(120, 10, 10, 100));
        Assert.False(RedDetector.IsRed(120, 10, 10, 200));
    }

    [Theory]
    [InlineData(255, 128, 0)]   // orange
    [InlineData(255, 255, 0)]   // yellow
    [InlineData(255, 0, 255)]   // magenta
    [InlineData(255, 150, 150)] // pink
    [InlineData(255, 255, 255)] // white
    [InlineData(200, 200, 200)] // grey
    [InlineData(0, 0, 0)]       // black
    public void NonRedShades_AreNotRed(int r, int g, int b) => Assert.False(RedDetector.IsRed(r, g, b, 0));

    [Fact] public void GreenAndBlueHalfOfRed_IsStillRed() => Assert.True(RedDetector.IsRed(254, 127, 127, 200));

    [Fact] public void Threshold0_BlackIsNotRed_ButDimRedIs()
    {
        Assert.False(RedDetector.IsRed(0, 0, 0, 0));
        Assert.True(RedDetector.IsRed(1, 0, 0, 0));
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(0, 0)]
    [InlineData(128, 128)]
    [InlineData(255, 255)]
    [InlineData(999, 255)]
    public void ClampThreshold_KeepsRange(int input, int expected) => Assert.Equal(expected, RedDetector.ClampThreshold(input));

    [Theory]
    [InlineData("200", true, 200)]
    [InlineData(" 42 ", true, 42)]
    [InlineData("0", true, 0)]
    [InlineData("255", true, 255)]
    [InlineData("256", false, 0)]
    [InlineData("-1", false, 0)]
    [InlineData("abc", false, 0)]
    [InlineData("", false, 0)]
    [InlineData("12.5", false, 0)]
    public void TryParseThreshold_ValidatesInput(string text, bool ok, int value)
    {
        Assert.Equal(ok, RedDetector.TryParseThreshold(text, out int v));
        if (ok) Assert.Equal(value, v);
    }
}
