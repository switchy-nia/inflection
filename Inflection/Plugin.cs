using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using Dalamud.Utility.Signatures;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Dalamud.Utility;
using Dalamud.Memory;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Inflection.Windows;

namespace Inflection;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider InteropProvider { get; private set; } = null!;
    private const string ConfigCommandName = "/inflection";
    private const string EnableProfileCommandName = "/inflectionset";

    public Inflection.Inflections speech = null!;
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Aiko's Inflection");
    // private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private unsafe delegate byte ProcessChatInputDelegate(IntPtr uiModule, byte** message, IntPtr a3);
    [Signature("E8 ?? ?? ?? ?? FE 86 ?? ?? ?? ?? C7 86 ?? ?? ?? ?? ?? ?? ?? ??", DetourName = nameof(ProcessChatInputDetour), Fallibility = Fallibility.Auto)]
    private Hook<ProcessChatInputDelegate> ProcessChatInputHook { get; set; } = null!;
    private readonly List<string> channelaliases = new List<string>()
    {
        "/t", "/tell", "/s", "/say", "/p", "/party", "/a", "/alliance", "/y", "/yell", "/sh", "/shout", "/fc", "/freecompany", "/n", "/novice", "/cwl1", "/cwlinkshell1", "/cwl2", "/cwlinkshell2", "/cwl3", "/cwlinkshell3", "/cwl4", "/cwlinkshell4", "/cwl5", "/cwlinkshell5", "/cwl6", "/cwlinkshell6", "/cwl7", "/cwlinkshell7", "/cwl8", "/cwlinkshell8", "/l1", "/linkshell1", "/l2", "/linkshell2", "/l3", "/linkshell3", "/l4", "/linkshell4", "/l5", "/linkshell5", "/l6", "/linkshell6", "/l7", "/linkshell7", "/l8", "/linkshell8"
    };
    public Plugin()
    {
        try
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        }
        catch (Newtonsoft.Json.JsonReaderException e)
        {
            Log.Error($"Error reading configuration due to error {e.Message}");
            Log.Info("Recreating default configuration");
            Configuration = new Configuration();
        }
        catch (Exception e)
        {
            Log.Error($"Error reading configuration due to error {e.InnerException}");
            Log.Info("Recreating default configuration");
            Configuration = new Configuration();
        }

        if (Configuration.Profiles.Count() == 0)
        {
            Configuration.Profiles = Configuration.BuiltInProfiles();
            Configuration.SetActiveProfile(Configuration.Profiles.First().Id);
        }

        // ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        // WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        _configureCommands();
        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        // PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        speech = new Inflection.Inflections(Configuration.ActiveProfile);

        InteropProvider.InitializeFromAttributes(this);
        ProcessChatInputHook.Enable();
        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        Log.Information($"Inflection started successfully");
    }

    public void SetActiveProfile(Guid guid)
    {
        Configuration.SetActiveProfile(guid);
        speech = new Inflections(Configuration.ActiveProfile);
        Configuration.Save();
    }
    private void _configureCommands()
    {

        CommandManager.AddHandler(ConfigCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the main configuration"
        });
        CommandManager.AddHandler(EnableProfileCommandName, new CommandInfo(OnEnableProfileCommand)
        {
            HelpMessage = "Toggle a profile by name or UUID. If used without a parameter, it automatically sets the \"Empty\" profile"
        });
    }
    /// Taken from Gagspeak chat deteours to pull the chat information.
    private unsafe byte ProcessChatInputDetour(IntPtr uiModule, byte** message, IntPtr a3)
    {
        // Put all this shit in a try-catch loop so we can catch any possible thrown exception.
        try
        {
            // Grab the original string.
            var originalSeString = MemoryHelper.ReadSeStringNullTerminated((nint)(*message));
            var messageDecoded = originalSeString.ToString(); // the decoded message format.

            // Debug the output (remove later)
            foreach (var payload in originalSeString.Payloads)
                Log.Debug($"Message Payload [{payload.Type}]: {payload.ToString()}");

            if (string.IsNullOrWhiteSpace(messageDecoded))
            {
                Log.Debug("Message was null or whitespace, returning original.");
                return ProcessChatInputHook.Original(uiModule, message, a3);
            }

            // Create the new string to send.
            var newSeStringBuilder = new SeStringBuilder();
            var matchedCommand = "";
            var matchedChannelType = "";
            Log.Debug($"Detouring Message: {messageDecoded}");

            // We check for commands which can be either "/" or "the autotranslate moji and /"
            if (messageDecoded.StartsWith("/") || messageDecoded.StartsWith(" /"))
            {

                var command = messageDecoded.Split(" ")[0];
                matchedCommand = channelaliases.AsQueryable()
                    .FirstOrDefault(prefix => command.Equals(prefix,
                                    StringComparison.OrdinalIgnoreCase));
                if (matchedCommand.IsNullOrEmpty())
                {
                    Log.Debug($"Ignoring {messageDecoded} as the command portion {command} it is not in the channel list");
                    return ProcessChatInputHook.Original(uiModule, message, a3);
                }

                // Will use this later when returning the output. The space is only useful with a matched command so adding here.
                matchedCommand = matchedCommand.TrimEnd() + " ";

                // if tell command is matched, need extra step to protect target name
                if (matchedCommand.StartsWith("/tell") || matchedCommand.StartsWith("/t"))
                {
                    Log.Debug($"[Chat Processor]: Matched Command is a tell command");
                    var selfTellRegex = @"(?<=^|\s)/t(?:ell)?\s{1}(?<name>\S+\s{1}\S+)@\S+\s{1}\*\k<name>(?=\s|$)";
                    if (!Regex.Match(messageDecoded, selfTellRegex).Value.IsNullOrEmpty())
                    {
                        Log.Debug("[Chat Processor]: Ignoring Message as it is a self tell garbled message.");
                        return ProcessChatInputHook.Original(uiModule, message, a3);
                    }
                    // Match any other outgoing tell to preserve target name
                    var tellRegex = @"(?<=^|\s)/t(?:ell)?\s{1}(?:\S+\s{1}\S+@\S+|\<r\>)\s?(?=\S|\s|$)";
                    matchedCommand = Regex.Match(messageDecoded, tellRegex).Value;
                }
                Log.Debug($"Matched Command [{matchedCommand}] for matchedChannelType: [{matchedChannelType}]");
            }

            // If current channel message is being sent to is in list of enabled channels, translate it.
            // only obtain the text payloads from this message, as nothing else should madder.
            var textPayloads = originalSeString.Payloads.OfType<TextPayload>().ToList();

            // merge together the text of all the split text payloads.
            var originalText = string.Join("", textPayloads.Select(tp => tp.Text));

            // after we have done that, take this string and get the substring with the matched command length.
            var stringToProcess = originalText.Substring(matchedCommand.Length);

            stringToProcess = speech.Speak(stringToProcess);
            // once we have done that, garble that string, and then merge it back with the output command in front.
            var output = matchedCommand + stringToProcess;

            // append this to the newSeStringBuilder.
            newSeStringBuilder.Add(new TextPayload(output));

            // DEBUG MESSAGE: (Remove when not debugging)
            Log.Debug("Output: " + output);

            if (string.IsNullOrWhiteSpace(output))
                return 0; // Do not sent message.

            // Construct it for finalization.
            var newSeString = newSeStringBuilder.Build();

            // Verify its a legal width
            if (newSeString.TextValue.Length <= 500)
            {
                var utf8String = Utf8String.FromString(".");
                utf8String->SetString(newSeString.Encode());
                return ProcessChatInputHook.Original(uiModule, (byte**)((nint)utf8String).ToPointer(), a3);
            }
            else // return original if invalid.
            {
                Log.Error("Speak returned a variant of Message was longer than max message length!");
                return ProcessChatInputHook.Original(uiModule, message, a3);
            }
        }
        catch (Exception e)
        { // cant ever have enough safety!
            Log.Error($"Error sending message to chat box (secondary): {e}");
        }
        // return the original message untranslated
        return ProcessChatInputHook.Original(uiModule, message, a3);
    }


    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        // ConfigWindow.Dispose();
        MainWindow.Dispose();

        ProcessChatInputHook?.Disable();
        ProcessChatInputHook?.Dispose();
        CommandManager.RemoveHandler(ConfigCommandName);
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    private void OnEnableProfileCommand(string command, string args)
    {
        // Parse the command to pull the commend name out
        if (args.Length == 0)
        {
            Log.Info($"Disabling profiles");
            var id = Configuration.Profiles.Find(p => p.Label == "Empty")!.Id;
            Configuration.SetActiveProfile(id);
        }
        else
        {
            Log.Info("Enabling profile {args}");
            var profile_guid = Guid.Empty;
            if (!Guid.TryParse(args, out profile_guid))
            {
                var profile = this.Configuration.Profiles.Find(p => p.Label == args);
                profile_guid = profile != null ? profile.Id : Guid.Empty;
            }
            if (profile_guid != Guid.Empty)
            {
                this.SetActiveProfile(profile_guid);
            }
            else
            {
                Log.Warning("Enable profile failed because the GUID or Label of the profile was not found.");
            }
        }
    }
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => MainWindow.Toggle();
    // public void ToggleMainUI() => MainWindow.Toggle();
}
