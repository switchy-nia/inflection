namespace Inflection.Tests;

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
        };
        Inflections inflections = new Inflections(profile);
        string result = inflections.Speak("This is a test.");
        // Assert.Equal("T-T-T-This i-i-i-is a-a-a-a test.", result);
        Assert.Matches(@"(T-)+This (i-)+is (a-)+a (t-)+test.", result);
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

    [Fact]
    public void TestTicks()
    {
        var profile = new Profile
        {
            TicksEnabled = true,
            TickChance = 100,
            Ticks = {
                "tick"
            },
            TickCooldown = 3
        };
        var inflection = new Inflections(profile);
        string result = inflection.Speak("One two three four five six seven eight nine");
        Assert.Equal("One two three tick four five six tick seven eight nine tick", result);
    }

    [Fact]
    public void TestMute()
    {
        var profile = new Profile
        {
            MuteEnabled = true
        };
        var inflect = new Inflections(profile);
        var result = inflect.Speak("This is a long text string that doesn't have much except for (a single OOC) and *waves* everything else should be cut");
        Assert.Equal("(a single OOC)*waves*", result);
    }

    [Fact]
    public void TestPatterns()
    {
        var profile = new Profile
        {
            PatternsEnabled = true,
            Patterns = { new WordPatterns(@"five", "Replacement Success") }
        };
        var inflect = new Inflections(profile);
        var result = inflect.Speak("One two three four five six seven eight nine");
        Assert.Equal("One two three four Replacement Success six seven eight nine", result);


        profile = new Profile
        {
            PatternsEnabled = true,
            Patterns = { new WordPatterns(@"(n)([ao])", "$1y$2") }
        };
        inflect = new Inflections(profile);
        result = inflect.Speak("No, it is not true that natalie is a cat that says nya");
        Assert.Equal("Nyo, it is nyot true that nyatalie is a cat that says nya", result);

        profile = new Profile
        {
            PatternsEnabled = true,
            Patterns = { new WordPatterns(@"(n)([ao])", "$1y$2"), new WordPatterns(@"alie\b", "ily") }
        };
        inflect = new Inflections(profile);
        result = inflect.Speak("No, it is not true that natalie is a cat that says nya");
        Assert.Equal("Nyo, it is nyot true that nyatily is a cat that says nya", result);
    }
}
