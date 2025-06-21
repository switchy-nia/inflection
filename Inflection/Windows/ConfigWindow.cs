using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;

namespace Inflection.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private bool previewStutterEnabled = false;
    private int previewStutterChance = 0;
    private int previewStutterSeverity = 0;

    public bool previewForcedPronounEnabled = false;

    private bool configDirty = false;
    private readonly CancellationTokenSource cts = new();

    private void GetLastConfig()
    {
        previewStutterEnabled = configuration.ActiveProfile.StutterEnabled;
        previewStutterChance = configuration.ActiveProfile.StutterChance;
        previewStutterSeverity = configuration.ActiveProfile.MaxStutterSeverity;

        previewForcedPronounEnabled = configuration.ActiveProfile.PronounCorrectionEnabled;
    }

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Inflection Configuration###With a constant ID")
    {
        // Flags = ImGuiWindowFlags.AlwaysAutoResize;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
        GetLastConfig();

        _ = Task.Run((Func<Task?>)(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (configDirty)
                    {
                        this.configuration.Save();
                        configDirty = false;
                    }
                    await Task.Delay(2000, cts.Token); // Wait for 2 seconds before checking again
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }), cts.Token);
    }

    public void Dispose() { }

    private void InitTextFields()
    {

    }

    public override void Draw() { }
}
