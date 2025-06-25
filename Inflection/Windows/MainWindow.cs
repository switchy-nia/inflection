using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace Inflection.Windows;

public class MainWindow : Window, IDisposable
{
    private enum WhichMenu
    {
        Main, Profile
    }
    private Plugin plugin;
    private ProfileEditor profileEditor = new ProfileEditor();
    private WhichMenu menu = WhichMenu.Main;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Aiko's Inflection")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class
        // ImGui.TextUnformatted($"The random config bool is {plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");
        using (ImRaii.TabBar("Profile Settings##main"))
        {
            if (ImGui.BeginTabItem("Main Settings##mainsettings"))
            {
                drawMain();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Edit Active Profile##profileEdit"))
            {
                if (profileEditor.Draw(plugin.Configuration.ActiveProfile))
                {
                    plugin.Configuration.Save();
                }
                ImGui.EndTabItem();
            }
        }
        // if (ImGui.Button("Show Settings"))
        // {
        //     plugin.ToggleConfigUI();
        // }
        //
        // ImGui.Spacing();
        //
        // // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        // // ImRaii takes care of this after the scope ends.
        // // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
        // using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        // {
        //     // Check if this child is drawing
        //     if (child.Success)
        //     {
        //         ImGuiHelpers.ScaledDummy(20.0f);
        //
        //         // Example for other services that Dalamud provides.
        //         // ClientState provides a wrapper filled with information about the local player object and client.
        //
        //         var localPlayer = Plugin.ClientState.LocalPlayer;
        //         if (localPlayer == null)
        //         {
        //             ImGui.TextUnformatted("Our local player is currently not loaded.");
        //             return;
        //         }
        //
        //         if (!localPlayer.ClassJob.IsValid)
        //         {
        //             ImGui.TextUnformatted("Our current job is currently not valid.");
        //             return;
        //         }
        //
        //         // ExtractText() should be the preferred method to read Lumina SeStrings,
        //         // as ToString does not provide the actual text values, instead gives an encoded macro string.
        //         ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");
        //
        //         // Example for quarrying Lumina directly, getting the name of our current area.
        //         var territoryId = Plugin.ClientState.TerritoryType;
        //         if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
        //         {
        //             ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
        //         }
        //         else
        //         {
        //             ImGui.TextUnformatted("Invalid territory.");
        //         }
        //     }
        // }
    }
    private void drawMain()
    {
        int current = plugin.Configuration.ActiveProfileIndex;
        // Maybe write out some welcome text.
        ImGui.TextWrapped("""
                Hello and welcome to this little plugin of mine. It was originally made for my darling Aiko, but if you have it installed you are very welcome to play around with it.

                To get started, select one of the prebuilt profiles below.
                If you want some customization, you can duplicate a profile and then edit it in the tab above.

                Many apologies for the messy UI. This will hopefully be temporary <3

                - Switchy Nia
                """);

        if (ImGui.Button("New Blank Profile"))
        {
            Plugin.Log.Debug($"Making a new empty profile");
            plugin.Configuration.CreateNewProfile();
        }

        if (ImGui.Button($"Make a copy of '{plugin.Configuration.ActiveProfile.Label}'"))
        {
            Plugin.Log.Debug($"Making a new copy of {plugin.Configuration.ActiveProfile.Id}");
            plugin.Configuration.CreateCopyProfile(plugin.Configuration.ActiveProfile.Id);
        }

        ImGui.BeginDisabled(plugin.Configuration.ActiveProfile.Readonly);
        if (ImGui.Button($"Delete {plugin.Configuration.ActiveProfile.Label}"))
        {
            Plugin.Log.Debug($"Deleting {plugin.Configuration.ActiveProfile.Id}");
            plugin.Configuration.DeleteProfile();
        }
        ImGui.EndDisabled();

        // profile selector
        var label = plugin.Configuration.ActiveProfile.Label;
        if (ImGui.BeginCombo("Profile##selector", plugin.Configuration.ActiveProfile.Label))
        {
            for (int i = 0; i < plugin.Configuration.Profiles.Count; i++)
            {
                if (ImGui.Selectable(plugin.Configuration.Profiles[i].Label, i == current))
                {
                    var id = plugin.Configuration.Profiles[i].Id;
                    plugin.SetActiveProfile(id);
                    current = plugin.Configuration.ActiveProfileIndex;
                }
            }
            ImGui.EndCombo();
        }
    }
}
