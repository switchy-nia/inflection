using Dalamud.Configuration;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

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
    public int ActiveProfileIndex
    {
        get
        {
            if (Profiles.Count == 0)
            {
                return -1;
            }
            else
                return Profiles.FindIndex(0, Profiles.Count, profile => profile.Id.Equals(ActiveProfileId));
        }
    }
    public List<Profile> Profiles = new List<Profile>();
    public Profile ActiveProfile
    {
        get
        {
            var profile = Profiles.Find(profile => profile.Id.Equals(ActiveProfileId));
            if (profile == null)
            {
                Plugin.Log.Debug($"ProfileID {ActiveProfileId} is null returning default");
                return Profiles[0];
            }
            else
            {
                return profile;
            }
        }
    }

    public bool Locked { get; set; } = false;
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public static List<Profile> BuiltInProfiles()
    {
        List<Profile> profiles = new List<Profile>() {
            new Profile {
                Label = "Empty",
                Readonly = true,
            },

            new Profile {
                Label = "nervous",
                Readonly = true,
                StutterEnabled = true,
                StutterChance = 25,
                MaxStutterSeverity = 3,
                StutterCooldown = 3
            },
            new Profile {
                Label = "small voice",
                Readonly = true,
                VoiceType = VoiceVolume.Small,
            },

            new Profile {
                Label = "BIG TALK",
                Readonly = true,
                VoiceType = VoiceVolume.Big,
            },

            new Profile {
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
                    new WordPatterns( @"^[aiouaiouee](n)([aoeiu])", "$1y$2"),
                }
            },

            new Profile {
                Label = "Bimbo",
                Readonly = true,
                TicksEnabled = true,
                Ticks = new HashSet<string>() {
                    "umm", "like", "lol", "ummmm", "*giggles*"
                },
                SentenceEndingEnabled = true,
                SentenceEndings = { "ya know ♥", "or whatever ♥", "and stuff ♥" },
                TickChance = 3,
                TickCooldown = 4,
                PatternsEnabled = true,
                Patterns = {
                    new WordPatterns(@"\d\d\d+", "lots"),
                    new WordPatterns(@"bility\b", "bilty"),
                    new WordPatterns(@"tible\b", "tidle"),
                    new WordPatterns(@"tes\b", "ties"),
                    new WordPatterns(@"ces\b", "cies"),
                    new WordPatterns(@"ges\b", "gies"),
                    new WordPatterns(@"uter\b", "tuer"),
                    new WordPatterns(@"\bth(?!i)", "d"),
                    new WordPatterns(@"ph\B", "f"),
                    new WordPatterns(@"ee", "ea"),
                    new WordPatterns(@"ou", "u"),
                    new WordPatterns(@"([sv])e\b", "$1"),
                    new WordPatterns(@"([b-df-hj-npv-z])r\B/", "$1w"),
                    new WordPatterns(@"ss\b", "z")
                }
            },

            new Profile {
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
                Label = "Mute",
                Readonly = true,
                MuteEnabled = true
            },
        };
        return profiles;
    }
    internal void CreateNewProfile()
    {
        Plugin.Log.Debug($"Attempting to create Profile");
        var newpf = new Profile();
        newpf.Label = "New Profile" + Profiles.FindAll(p => p.Label.StartsWith(newpf.Label)).Count;
        Profiles.Add(newpf);
        SetActiveProfile(newpf.Id);
        Save();
    }

    internal void CreateCopyProfile(Guid id)
    {
        Plugin.Log.Debug($"Making a new copy of {id}");
        var newpf = ActiveProfile.DeepCopy();
        var count = Profiles.FindAll(p => p.Label.StartsWith(newpf.Label)).Count;
        newpf.Label = newpf.Label + "(copy " + count + ")";
        Profiles.Add(newpf);
        SetActiveProfile(newpf.Id);
        Save();
    }

    internal void DeleteProfile()
    {
        if (ActiveProfile.Readonly)
        {
            Plugin.Log.Debug($"Attempting to delete active readonly profile");
            return;
        }
        Plugin.Log.Debug($"Deleting active profile");
        int to_delete = ActiveProfileIndex;
        SetActiveProfile(Profiles[0].Id);
        Profiles.RemoveAt(to_delete);
        Save();
    }

    internal void SetActiveProfile(Guid id)
    {
        Plugin.Log.Debug($"Attempting to set {id}");
        var p = Profiles.Find(v => v.Id.Equals(id));
        if (p != null)
        {
            this.ActiveProfileId = id;
            Plugin.Log.Debug($"Found and setting {this.ActiveProfileId} as ActiveProfileId");
        }
    }

    public Configuration()
    {
        if (this.Profiles.Count > 0)
            this.ActiveProfileId = this.Profiles[0].Id;
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
        // Hackily serializes to json and back in order to make a full "deep copy" of the profile
        // This is low effort, low maintenance and -- while lower performance -- it's not going to be used enough to justify
        // A huge amoutn of effort
        string jsonprofile = JsonConvert.SerializeObject(this);
        if (jsonprofile == null)
        {
            Plugin.Log.Error($"This object apparently is null, somehow???? ");
        }
        Profile newprofile = JsonConvert.DeserializeObject<Profile>(jsonprofile!) ?? new Profile();
        newprofile.Id = Guid.NewGuid();
        newprofile.Label = newprofile.Label + " Clone";
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

    private bool InputText(string label, string input, uint length, Action<string> setter)
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
                if (ImGui.Button("x"))
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
            using (ImRaii.Table($"Custom Speech Patterns (Regex Replacement)##Custom Speech Patterns (Regex Replacement)list", 3))
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 48);
                ImGui.TableSetupColumn("Pattern");
                ImGui.TableSetupColumn("Replacement");
                ImGui.TableHeadersRow();

                ImGui.TableNextColumn();
                if (ImGui.Button("+##addpattern"))
                {
                    profile.Patterns.Add(new WordPatterns(placeholderPattern, placeholderReplacement));
                    placeholderPattern = "";
                }
                ImGui.TableNextColumn();
                ImGui.InputText("##newpattern", ref placeholderPattern, 40);
                ImGui.TableNextColumn();
                ImGui.InputText("##newreplacement", ref placeholderReplacement, 40);

                foreach (WordPatterns pattern in profile.Patterns)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.Button($"x##patterns{pattern}"))
                    {
                        profile.Patterns.Remove(pattern);
                    }
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(pattern.Pattern);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(pattern.Replacement);
                }
            }
            ImGui.EndDisabled();
        }
    }
}
