namespace Inflection.Tests;

using System.Text.RegularExpressions;
using Inflection;
public class InflectionTest
{
    [Fact]
    public void TestNormal()
    {
        var profile = new Profile();
        Inflections inflections = new Inflections(profile);
        string result = inflections.Speak("This is a test."); // var result = "This is a test.";
        Assert.Equal("This is a test.", result);
    }

    [Fact]
    public void TestLittleVoice()
    {
        var profile = new Profile
        {
            Label = "small voice",
            Readonly = true,
            VoiceType = VoiceVolume.Small,
        };
        Inflections inflections = new Inflections(profile);
        string result = inflections.Speak("THIS IS A TEST."); // var result = "This is a test.";
        Assert.Equal("this is a test...", result);
        result = inflections.Speak("THIS IS A TEST!"); // var result = "This is a test.";
        Assert.Equal("this is a test..!", result);
        result = inflections.Speak("THIS IS A TEST?"); // var result = "This is a test.";
        Assert.Equal("this is a test..?", result);
    }

    [Fact]
    public void TestBigVoice()
    {
        var profile = new Profile
        {
            Label = "BIG TALK",
            Readonly = true,
            VoiceType = VoiceVolume.Big,
        };
        Inflections inflections = new Inflections(profile);
        string result = inflections.Speak("This is a test."); // var result = "This is a test.";
        Assert.Equal("THIS IS A TEST!", result);
        result = inflections.Speak("This is a test!"); // var result = "This is a test.";
        Assert.Equal("THIS IS A TEST!!", result);
        result = inflections.Speak("This is a test?"); // var result = "This is a test.";
        Assert.Equal("THIS IS A TEST!?", result);
    }

    [Fact]
    public void TestNervousVoice()
    {
        var profile = new Profile
        {
            Label = "nervous",
            Readonly = true,
            StutterEnabled = true,
            StutterChance = 100,
            MaxStutterSeverity = 3,
            MaxStuttersPerSentence = 3,
        };
        Inflections inflections = new Inflections(profile);
        string result = inflections.Speak("This is a test.");
        Regex regex = new Regex(@"(T-)+This (i-)+is (a-)+a (t-)+test.");
        // Assert.Equal("T-T-T-This i-i-i-is a-a-a-a test.", result);
        Assert.True(regex.IsMatch(result));
    }

    [Fact]
    public void TestRestrictedSpeech()
    {
        var profile = new Profile
        {
            Label = "locked",
            CompelledSpeechEnabled = true,
            CompelledSpeechWords = {
                "test"
            }
        };
        Inflections inflections = new Inflections(profile);
        string result = inflections.Speak("This is a test.");
        Assert.Equal("test test test test.", result);
    }
    [Fact]
    public void TestEmote()
    {
        var profile = new Profile
        {
            CompelledSpeechEnabled = true,
            CompelledSpeechWords = { "test" }
        };
        var inflection = new Inflections(profile);
        string result = inflection.Speak("*test test test test.*");
        Assert.Equal("*test test test test.*", result);
    }
    [Fact]
    public void TestOOC()
    {
        var profile = new Profile
        {
            CompelledSpeechEnabled = true,
            CompelledSpeechWords = { "test" }
        };
        var inflection = new Inflections(profile);
        string result = inflection.Speak("(test test test test.)");
        Assert.Equal("(test test test test.)", result);
    }
}
