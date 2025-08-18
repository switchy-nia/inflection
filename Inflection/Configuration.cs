using Dalamud.Configuration;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Inflection;

// TODO: Convert the configuration to include profiles with all the contained information.
// Ensure that the profiles 
[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool InflectionEnabled = false;
    public string PreviewText = @"Tell me...for whom do you fight?
Hmph! Do you believe in Eorzea? Eorzea's unity is forged of falsehoods. Its city-states are built on deceit. And its faith is an instrument of deception.
It is naught but a cobweb of lies.To believe in Eorzea is to believe in nothing.
In Eorzea, the beast tribes often summon gods to fight in their stead─though your comrades only rarely respond in kind.Which is strange, is it not?
Are the “Twelve” otherwise engaged? I was given to understand they were your protectors.If you truly believe them your guardians, why do you not repeat the trick that served you so well at Carteneau, and call them down?
They will answer─so long as you lavish them with crystals and gorge them on aether.
Your gods are no different from those of the beasts─eikons every one.Accept but this, and you will see how Eorzea's faith is bleeding the land dry.
Nor is this unknown to your masters. Which prompts the question: why do they cling to these false deities? What drives even men of learning─even the great Louisoix─to grovel at their feet?
The answer? Your masters lack the strength to do otherwise!
For the world of man to mean anything, man must own the world.
To this end, he hath fought ever to raise himself through conflict─to grow rich through conquest.
And when the dust of battle settles, it is ever the strong who dictate the fate of the weak.
Knowing this, but a single path is open to the impotent ruler─that of false worship.A path which leads to enervation and death.
Only a man of power can rightly steer the course of civilization. And in this land of creeping mendacity, that one truth will prove its salvation.
Come, champion of Eorzea, face me! 
Your defeat shall serve as proof of my readiness to rule!
It is only right that I should take your realm. For none among you has the power to stop me!";
    // public Dictionary<string, Profile> DefaultProfile = new Dictionary<Guid, Profile>();
    // public Dictionary<Guid, Profile> CustomProfile = new Dictionary<Guid, Profile>();
    public Guid ActiveProfileId;

    [NonSerialized]
    public readonly List<Profile> BuiltInProfiles = Configuration.InitBuiltInProfiles();

    public List<Profile> CustomProfiles = new List<Profile>();

    public IEnumerable<Profile> Profiles()
    {
        return this.GetProfiles();
    }

    public Profile ActiveProfile()
    {
        foreach (var p in Profiles())
        {

            if (p.Id.Equals(ActiveProfileId))
            {
                return p;
            }
        }
        Plugin.Log.Debug($"ProfileID {ActiveProfileId} is null returning default");
        ActiveProfileId = BuiltInProfiles[0].Id;
        return BuiltInProfiles[0];
    }

    public bool Locked { get; set; } = false;
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    private static List<Profile> InitBuiltInProfiles()
    {
        List<Profile> profiles = new List<Profile>() {
            new Profile {
                Id = Guid.Parse("98d7fbbd-e68b-47dc-bb59-b07314245f7f"),
                Label = "Empty",
                Readonly = true,
            },

            new Profile {
                Id = Guid.Parse("70ea7bb5-ca01-4af5-b3fa-0cf9479a2663"),
                Label = "nervous",
                Readonly = true,
                StutterEnabled = true,
                StutterChance = 25,
                MaxStutterSeverity = 3,
                StutterCooldown = 3
            },

            new Profile {
                Id = Guid.Parse("72ccfc71-2703-497b-865a-f577ca2db107"),
                Label = "small voice",
                Readonly = true,
                VoiceType = VoiceVolume.Small,
            },

            new Profile {
                Id = Guid.Parse("7b51cc2f-7949-42b1-9c69-2f0ab9af1008"),
                Label = "BIG TALK",
                Readonly = true,
                VoiceType = VoiceVolume.Big,
            },

            new Profile {
                Id = Guid.Parse("9528347d-0f04-4f33-9289-0c7bdf9f9e88"),
                Label = "Catgirl",
                Readonly = true,
                TicksEnabled = true,
                WordReplacementEnabled = true,
                StutterEnabled = true,
                SentenceStartEnabled = false,
                SentenceEndingEnabled = false,
                CompelledSpeechWords = new HashSet<string>() {},
                WordReplacement = new Dictionary<string, string>() {
                    { "i", "kitty" },
                    { "me", "kitty" },
                    { "i'm", "kitty is"},
                    { "im", "kitty is" },
                    { "myself", "this kitty" },
                },

                Ticks = new HashSet<string>() {
                    "mew", "meow", "mrow"
                },
                TickChance = 20,
                TickCooldown = 7,
                StutterChance = 3,
                MaxStutterSeverity = 2,
                StutterCooldown = 10,

                PatternsEnabled = true,
                Patterns = {
                    new WordPatterns( @"\b(me)", "meow"),
                    new WordPatterns( @"([n])([aoeu])", "$1y$2"),
                }
            },
            new Profile {
                Id = Guid.Parse("0d4e1a14-e1f0-425c-a553-02babbc59328"),
                Label = "Lisp",
                Readonly = true,
                PatternsEnabled = true,
                Patterns = {
                    new WordPatterns("[Ss][Hh]?", "th", 80),
                    new WordPatterns("([^Cc])[Cc]([EI])", "$1th$2", 70),
                }
            },
            new Profile {
                Id = Guid.Parse("4e018e31-fe2f-4820-ab96-8dc29af20c49"),
                Label = "Dweeb",
                Readonly = true,
                TicksEnabled = true,
                Ticks = new HashSet<string> () {
                    "uhh.", "umm..", "er..", "-oh umm..."
                },
                TickChance = 15,
                TickCooldown = 4,
                StutterEnabled = true,
                StutterCooldown = 3,
                MaxStutterSeverity = 3,
                StutterChance = 20,
                PatternsEnabled = true,
                Patterns = {
                    new WordPatterns("[Ss][Hh]?", "th", 80),
                    new WordPatterns("^[Cc][Cc]([EI])", "th$1", 70),
                }
            },
            new Profile {
                Id = Guid.Parse("2e5b4421-548f-402e-9724-9daf99c37bc0"),
                Label = "Bimbo",
                Readonly = true,
                TicksEnabled = true,
                Ticks = new HashSet<string>() {
                    "umm", "like", "ummmm", "*giggles*", "so", "sooooooooo", "totally", "♥", "♥♥♥"
                },
                SentenceEndingEnabled = true,
                SentenceEndings = { "ya know ♥", "or whatever ♥", "and stuff ♥", "lol ♥" },
                TickChance = 20,
                TickCooldown = 5,
                PatternsEnabled = true,
                Patterns = {
                    new WordPatterns(@"\d\d\d+", "lots", 75),
                    new WordPatterns(@"bility\b", "bilty", 50),
                    new WordPatterns(@"tible\b", "tidle", 50),
                    new WordPatterns(@"tes\b", "ties", 25),
                    new WordPatterns(@"ces\b", "cies", 50),
                    new WordPatterns(@"ges\b", "gies", 50),
                    new WordPatterns(@"uter\b", "tuer", 25),
                    new WordPatterns(@"m([aieou])n", "n$1m", 50),
                    new WordPatterns(@"n([aieou])m", "m$1n", 50),
                    new WordPatterns(@"\bth(?!i)", "d", 25),
                    new WordPatterns(@"ph\B", "f", 65),
                    new WordPatterns(@"ee", "ea", 50),
                    new WordPatterns(@"ou", "u", 25),
                    new WordPatterns(@"([sv])e\b", "$1", 25),
                    new WordPatterns(@"([b-df-hj-npv-z])r\B/", "$1w", 25),
                    new WordPatterns(@"que\b", "ck", 60),
                    new WordPatterns(@"gi\B", "ji", 50),
                    new WordPatterns(@"thl", "thal", 40),
                    new WordPatterns(@"ght", "t", 60),
                    new WordPatterns(@"[aieou]t\b", "d", 50),
                    new WordPatterns(@"s\b", "z", 56)
                }
            },

            new Profile {
                Id = Guid.Parse("54f78c37-712b-4d5a-a2fe-e459e06a4da7"),
                Label = "UwU",
                Readonly = true,
                SentenceEndingEnabled = true,
                SentenceEndings = {
                    "(・`ω´・)", ";;w;;", "owo", "UwU", ">w<", "^w^"
                },
                PatternsEnabled = true,
                Patterns = {
                    new WordPatterns("(?:r|l)", "w"),
                    new WordPatterns("(?:R|L)", "W"),
                    new WordPatterns("n([aeiou])", "ny$1"),
                    new WordPatterns("N([aeiou])", "Ny$1"),
                    new WordPatterns("N([AEIOU])", "NY$1"),
                    new WordPatterns("ove", "uv"),
                }

            },

            new Profile {
                Id = Guid.Parse("c6f70987-1222-46c7-b1a7-70886fd87f74"),
                Label = "Meow meow",
                Readonly = true,
                CompelledSpeechEnabled = true,
                CompelledSpeechWords = {
                    "meow"
                }
            },

            new Profile {
                Id = Guid.Parse("9de490a1-8a76-4391-b346-ac067c8cac11"),
                Label = "Mute",
                Readonly = true,
                MuteEnabled = true
            },
            new Profile {
                Id = Guid.Parse("1d257033-f252-4195-8472-167328c2dc25"),
                Label = "Morse Code",
                Readonly = true,
                PatternsEnabled = true,
                GlobalPatterns = {
                    ("[^a-zA-Z0-9 ]", ""),
                    (" ", "\\ "),
                    ("[Aa]", ".- "),
                    ("[Bb]", "-... "),
                    ("[Cc]", "-.-. "),
                    ("[Dd]", "-.. "),
                    ("[Ee]", ". "),
                    ("[Ff]", "..-. "),
                    ("[Gg]", "--. "),
                    ("[Hh]", ".... "),
                    ("[Ii]", ".. "),
                    ("[Jj]", ".--- "),
                    ("[Kk]", "-.- "),
                    ("[Ll]", ".-.. "),
                    ("[Mm]", "-- "),
                    ("[Nn]", "-. "),
                    ("[Oo]", "--- "),
                    ("[Pp]", ".--. "),
                    ("[Qq]", "--.- "),
                    ("[Rr]", ".-. "),
                    ("[Ss]", "... "),
                    ("[Tt]", "- "),
                    ("[Uu]", "..- "),
                    ("[Vv]", "...- "),
                    ("[Ww]", ".-- "),
                    ("[Xx]", "-..- "),
                    ("[Yy]", "-.-- "),
                    ("[Zz]", "--.. "),
                    ("0", "----- "),
                    ("1", ".---- "),
                    ("2", "..--- "),
                    ("3", "...-- "),
                    ("4", "....- "),
                    ("5", "..... "),
                    ("6", "-.... "),
                    ("7", "--... "),
                    ("8", "---.. "),
                    ("9", "----. "),
                }
            }
        };
        return profiles;
    }

    internal void CreateNewProfile()
    {
        Plugin.Log.Debug($"Attempting to create Profile");
        var newpf = new Profile();
        int count = 0;
        foreach (var p in Profiles())
        {
            if (p.Label.StartsWith("New Profile"))
            {
                count++;
            }
        }
        newpf.Label = count == 0 ? "New Profile" : $"New Profile {count}";
        CustomProfiles.Add(newpf);
        SetActiveProfile(newpf.Id);
        Save();
    }

    internal void CreateCopyProfile(Guid id)
    {
        Plugin.Log.Debug($"Making a new copy of {id}");
        var newpf = ActiveProfile().DeepCopy();
        int count = 0;
        foreach (var p in Profiles())
        {
            newpf.Label = Regex.Replace(newpf.Label, @"\(copy \d*\)", "").Trim();
            if (p.Label.StartsWith(newpf.Label))
            {
                count++;
            }
        }

        newpf.Label = $"{newpf.Label} (copy {count})";
        CustomProfiles.Add(newpf);
        SetActiveProfile(newpf.Id);
        Save();
    }

    internal void DeleteProfile()
    {
        if (ActiveProfile().Readonly)
        {
            Plugin.Log.Debug($"Attempting to delete built-in profile");
            return;
        }
        Plugin.Log.Debug($"Deleting active profile");
        var to_delete = CustomProfiles.Find(profile => profile.Id.Equals(ActiveProfileId));
        if (to_delete == null)
        {
            Plugin.Log.Debug($"Something went terribly wrong with this configuration because the active profile is already deleted");
        }
        else
        {
            CustomProfiles.Remove(to_delete);
        }
        SetActiveProfile(BuiltInProfiles[0].Id);
        Save();
    }

    internal void SetActiveProfile(Guid id)
    {
        Plugin.Log.Debug($"Attempting to set {id}");
        foreach (var p in Profiles())
        {
            if (p.Id.Equals(id))
            {
                this.ActiveProfileId = id;
                Plugin.Log.Debug($"Found and setting {this.ActiveProfileId} as ActiveProfileId");
            }
        }
    }

    // Returns the profiles from each set as an enumerable for those operations.
    public IEnumerable<Profile> GetProfiles()
    {
        foreach (var profile in BuiltInProfiles)
        {
            yield return profile;
        }
        foreach (var custom in CustomProfiles)
        {
            yield return custom;
        }
    }
    public Configuration()
    {
        this.ActiveProfileId = this.BuiltInProfiles[0].Id;
    }
}

public struct WordPatterns
{
    public string Pattern;
    public string Replacement;
    public int Chance;
    public WordPatterns(string pattern, string replacement, int chance = 100)
    {
        Pattern = pattern;
        Replacement = replacement;
        Chance = chance;
    }
}


public enum VoiceVolume
{
    Normal, Small, Big
}

[Serializable]
public class Profile
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
    public bool Readonly { get; set; } = false;
    public bool CompelledSpeechEnabled { get; set; } = false;
    public bool TicksEnabled { get; set; } = false;
    public bool WordReplacementEnabled { get; set; } = false;
    public bool StutterEnabled { get; set; } = false;
    public bool SentenceStartEnabled { get; set; } = false;
    public bool SentenceEndingEnabled { get; set; } = false;
    public bool MuteEnabled { get; set; } = false;

    public VoiceVolume VoiceType { get; set; } = VoiceVolume.Normal;

    public HashSet<string> CompelledSpeechWords = new HashSet<string>();
    public HashSet<string> Ticks = new HashSet<string>();

    public HashSet<string> SentenceStarts = new HashSet<string>();
    public HashSet<string> SentenceEndings = new HashSet<string>();

    public Dictionary<string, string> WordReplacement = new Dictionary<string, string>() { };

    public int TickChance = 100;
    public int TickCooldown = 5;

    public int StutterChance = 10;
    public int MaxStutterSeverity = 3;
    public int StutterCooldown = 3;

    public bool PatternsEnabled = false;
    public HashSet<WordPatterns> Patterns = new HashSet<WordPatterns>() { };
    public HashSet<(string, string)> GlobalPatterns = new HashSet<(string, string)>() { };

    public Profile()
    {
        this.Id = Guid.NewGuid();
    }

    /// Creates a shallow copy of the profile members (acts as a &mut ref)
    public Profile ShallowCopy()
    {
        return (Profile)MemberwiseClone();
    }
    /// Creates a new profile from itself.
    public Profile DeepCopy()
    {
        // Serializes to json and back in order to make a full "deep copy" of the profile
        // This is low effort, low maintenance and -- while lower performance -- it's not going to be used enough to justify anything more.
        string jsonprofile = JsonSerializer.Serialize(this);
        if (jsonprofile == null)
        {
            Plugin.Log.Error($"This object apparently is null, somehow???? ");
        }
        Profile newprofile = JsonSerializer.Deserialize<Profile>(jsonprofile!) ?? new Profile();
        newprofile.Id = Guid.NewGuid();
        newprofile.Readonly = false;
        Plugin.Log.Info($"Returning new profile {newprofile.Id} labeled {newprofile.Label}");
        return newprofile;
    }
}
class ProfileEditor
{
    private string placeholderStart = "";
    private string placeholderEnding = "";
    private string placeholderPattern = "";
    private string placeholderReplacement = "";
    private int placeholderPercentage = 100;
    private string addWordReplacementKey = "";
    private string addWordReplacementValue = "";
    private string addTick = "";
    private string addCompelledWord = "";
    /// Returns if save has been pressed.
    public bool Draw(Profile profile)
    {
        bool changed = false;
        if (ImGui.CollapsingHeader($"Profile##{profile.Id}"))
        {
            ImGui.BeginDisabled(profile.Readonly);
            if (profile.Readonly)
                ImGui.TextUnformatted("This is a built-in profile and cannot be changed. To customize your speech patters, create a custom profile from the main menu to change these settings.");
            changed |= InputText($"Label##{profile.Id}label", profile.Label, 64, v => profile.Label = v);
            ImGui.EndDisabled();
        }
        this.DrawTogglesRows(profile);
        this.DrawTicksRows(profile);
        this.DrawStutterRows(profile);
        this.DrawSentenceConfigRows(profile);
        this.DrawCustomPatternsConfigRows(profile);
        return changed;
    }

    private bool InputText(string label, string input, int length, Action<string> setter)
    {
        var tmp = input;
        if (ImGui.InputText(label, ref tmp, length) && tmp != input)
        {
            setter(tmp);
            return true;
        }
        return false;
    }

    private void InputInt(string label, int current, Action<int> setter)
    {
        var tmp = current;
        if (ImGui.InputInt(label, ref tmp) && tmp != current)
        {
            setter(tmp);
        }
    }
    private void Checkbox(string label, bool current, Action<bool> setter)
    {
        var tmp = current;
        if (ImGui.Checkbox(label, ref tmp) && tmp != current)
        {
            setter(tmp);
        }
    }

    // Organizational Helpers to define specific portions of the UI Widgets.
    private void DrawWordListWidget(string name, HashSet<string> words, ref string placeholderword)
    {
        ImGui.TextUnformatted($"{name}");
        using (ImRaii.Table($"{name}##{name}list", 2))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 48);
            ImGui.TableSetupColumn("Word");
            ImGui.TableHeadersRow();

            ImGui.TableNextColumn();
            if (ImGui.Button("+##add"))
            {
                words.Add(placeholderword);
                placeholderword = "";
            }
            ImGui.TableNextColumn();
            ImGui.InputText("##newword", ref placeholderword, 40);

            foreach (string word in words)
            {
                ImGui.TableNextColumn();
                if (ImGui.Button($"x##{word}"))
                {
                    words.Remove(word);
                }
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(word);
            }
        }
    }

    private void DrawDictionaryWidget(string name, Dictionary<string, string> dict, ref string placeholderKey, ref string placeholderValue)
    {
        ImGui.Text($"{name}");
        using (ImRaii.Table($"{name}###{name}dictionary", 3))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 48);
            ImGui.TableSetupColumn("Noun (Not Case Sensitive)");
            ImGui.TableSetupColumn("Replacement (case matters)");
            ImGui.TableHeadersRow();

            foreach ((string k, string v) in dict)
            {
                ImGui.TableNextColumn();
                if (ImGui.Button($"x##delete{k}"))
                {
                    dict.Remove(k);
                }
                var old_value = v;
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(k);
                ImGui.TableNextColumn();
                InputText($"##{k}", dict[k], 20, v => dict[k] = v);
            }

            ImGui.TableNextColumn();
            if (ImGui.Button($"+##{name}new"))
            {
                dict.Add(placeholderKey, placeholderValue);
                placeholderKey = "";
                placeholderValue = "";
            }
            ImGui.TableNextColumn();
            ImGui.InputText($"##{name}inputkey", ref placeholderKey, 20);
            ImGui.TableNextColumn();
            ImGui.InputText($"##{name}inputvalue", ref placeholderValue, 20);
        }
    }

    private void DrawTogglesRows(Profile profile)
    {
        if (ImGui.CollapsingHeader($"Feature Toggles##{profile.Id}feature_toggles"))
        {
            ImGui.BeginDisabled(profile.Readonly);
            Checkbox("Mute Enabled", profile.MuteEnabled, v => profile.MuteEnabled = v);
            Checkbox("Compelled Speech Enabled", profile.CompelledSpeechEnabled, v => profile.CompelledSpeechEnabled = v);
            Checkbox("Ticks Enabled", profile.TicksEnabled, v => profile.TicksEnabled = v);
            Checkbox("Pronoun Correction Enabled", profile.WordReplacementEnabled, v => profile.WordReplacementEnabled = v);
            Checkbox("Stutter Enabled", profile.StutterEnabled, v => profile.StutterEnabled = v);
            Checkbox("Global Patterns Enabled", profile.PatternsEnabled, v => profile.PatternsEnabled = v);
            var tmp = profile.VoiceType;

            if (ImGui.BeginCombo($"Voice Volume##{profile.Id}voicetype", tmp.ToString()))
            {
                if (ImGui.Selectable($"Normal##{profile.Id}voicetype", tmp == VoiceVolume.Normal))
                {
                    profile.VoiceType = VoiceVolume.Normal;
                }
                if (ImGui.Selectable($"small##{profile.Id}voicetype", tmp == VoiceVolume.Small))
                {
                    profile.VoiceType = VoiceVolume.Small;
                }
                if (ImGui.Selectable($"BIG##{profile.Id}voicetype", tmp == VoiceVolume.Big))
                {
                    profile.VoiceType = VoiceVolume.Big;
                }
                ImGui.EndCombo();
            }

            Checkbox("Sentence Start Enabled", profile.SentenceStartEnabled, v => profile.SentenceStartEnabled = v);
            Checkbox("Sentence Ending Enabled", profile.SentenceEndingEnabled, v => profile.SentenceEndingEnabled = v);
            ImGui.EndDisabled();
        }
    }

    private void DrawTicksRows(Profile profile)
    {
        if (ImGui.CollapsingHeader($"Vocal Ticks##{profile.Id}ticks"))
        {
            ImGui.BeginDisabled(profile.Readonly);
            InputInt("Minimum Words between Ticks", profile.TickCooldown, v => profile.TickCooldown = v);
            this.DrawWordListWidget("Possible Ticks", profile.Ticks, ref addTick);
            // int tempTickChance = (int)(profile.TickChance * 100);
            // InputInt("Tick Chance %", (int)(profile.TickChance * 100), v =>
            //     profile.TickChance = (float)(v / 100.0));
            // int tempTickMaxPortionOfSpeech = (int)(profile.TickMaxPortionOfSpeech * 100);
            // InputInt("Tick Portion of Speech", (int)(profile.TickMaxPortionOfSpeech * 100), v =>
            //     profile.TickMaxPortionOfSpeech = (float)(v / 100.0));
            ImGui.EndDisabled();
        }
    }

    private void DrawStutterRows(Profile profile)
    {
        if (ImGui.CollapsingHeader($"Stutter Configuration##{profile.Id}stutter"))
        {
            ImGui.BeginDisabled(profile.Readonly);
            InputInt("Stutter Chance (%)", profile.StutterChance, v => profile.StutterChance = v);
            InputInt("Stutter Severity", profile.MaxStutterSeverity, v => profile.MaxStutterSeverity = v);
            InputInt("Stutter Cooldown (words)", profile.StutterCooldown, v => profile.StutterCooldown = v);
            ImGui.EndDisabled();
        }
    }

    private void DrawSentenceConfigRows(Profile profile)
    {
        if (ImGui.CollapsingHeader($"General Sentence Configuration##{profile.Id}general"))
        {
            ImGui.BeginDisabled(profile.Readonly);
            this.DrawWordListWidget("Sentence Beginnings", profile.SentenceStarts, ref placeholderStart);
            this.DrawWordListWidget("Sentence Endings", profile.SentenceEndings, ref placeholderEnding);
            ImGui.Separator();
            this.DrawDictionaryWidget("Word Replacements", profile.WordReplacement, ref addWordReplacementKey, ref addWordReplacementValue);
            ImGui.Separator();
            this.DrawWordListWidget("Compelled Speech Word List", profile.CompelledSpeechWords, ref addCompelledWord);
            ImGui.EndDisabled();
        }
    }
    private void DrawCustomPatternsConfigRows(Profile profile)
    {
        if (ImGui.CollapsingHeader($"Custom Patterns##{profile.Id}patterns"))
        {
            ImGui.BeginDisabled(profile.Readonly);
            ImGui.TextUnformatted($"Custom Speech Patterns (Regex Replacement)");
            using (ImRaii.Table($"Custom Speech Patterns (Regex Replacement)##Custom Speech Patterns (Regex Replacement)list", 4))
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 48);
                ImGui.TableSetupColumn("Pattern");
                ImGui.TableSetupColumn("Replacement");
                ImGui.TableSetupColumn("% Chance");
                ImGui.TableHeadersRow();

                ImGui.TableNextColumn();
                if (ImGui.Button("+##addpattern"))
                {
                    profile.Patterns.Add(new WordPatterns(placeholderPattern, placeholderReplacement, placeholderPercentage));
                    placeholderPattern = "";
                    placeholderReplacement = "";
                    placeholderPercentage = 100;
                }
                ImGui.TableNextColumn();
                ImGui.InputText("##newpattern", ref placeholderPattern, 40);
                ImGui.TableNextColumn();
                ImGui.InputText("##newreplacement", ref placeholderReplacement, 40);
                ImGui.TableNextColumn();
                if (ImGui.InputInt("##newpercentage", ref placeholderPercentage))
                {
                    placeholderPercentage = Math.Clamp(placeholderPercentage, 0, 100);
                }

                // Hack because HashSet doesn't allow for easy enumeration.
                // Tolerable workaround because UI is getting completely redone soon:tm:
                int i = 0;
                foreach (WordPatterns pattern in profile.Patterns)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.Button($"x##{profile.Id}{i}"))
                    {
                        profile.Patterns.Remove(pattern);
                    }
                    i++;
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(pattern.Pattern);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(pattern.Replacement);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{pattern.Chance}");
                }
            }
            ImGui.EndDisabled();
        }
    }
}
