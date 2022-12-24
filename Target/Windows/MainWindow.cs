using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Target.Extensions;

namespace Target {
    public class MainWindow : Window {
        private readonly Plugin plugin;
        
        public MainWindow(Plugin plugin) : base("Target") {
            this.plugin = plugin;
            this.SizeCondition = ImGuiCond.Appearing;
            this.Size = new Vector2(420, 295) * ImGuiHelpers.GlobalScale;
        }

        public override void OnOpen() {
            base.OnOpen();
        }

        public override void Draw() {
            if(ImGuiEx.Checkbox("Enable###toggleEnabled", Plugin.Config, nameof(Plugin.Config.Enabled))) {
                Plugin.OverlayWindow.IsOpen = Plugin.Config.Enabled;
            }
            ImGui.SameLine();
            ImGuiEx.Checkbox("Locked###lockPos", Plugin.Config, nameof(Plugin.Config.LockPosition));
            ImGui.SameLine();
            ImGuiEx.Checkbox("ClickThrough###clickThrough", Plugin.Config, nameof(Plugin.Config.ClickThrough));

            ImGuiEx.Checkbox("Play Sound###playSound", Plugin.Config, nameof(Plugin.Config.PlaySound));
            ImGui.SameLine();
            ImGuiEx.Checkbox("LClick: Target###lClick", Plugin.Config, nameof(Plugin.Config.LClickTarget));
            ImGui.SameLine();
            ImGuiEx.Checkbox("RClick: Remove###lClick", Plugin.Config, nameof(Plugin.Config.RClickRemove));

            ImGuiEx.DragInt("Max Players###maxPlayers", Plugin.Config, nameof(Plugin.Config.MaxPlayers), 1, 1, 40);
            ImGuiEx.DragInt("Update Ms###updateMs", Plugin.Config, nameof(Plugin.Config.UpdateMs), 10, 10, 5000);
            ImGuiEx.DragInt("Width###overlayWidth", Plugin.Config, nameof(Plugin.Config.OverlayWidth), 1, 40, 400);
            ImGuiEx.DragInt("Height###overlayHeight", Plugin.Config, nameof(Plugin.Config.OverlayHeight), 1, 40, 400);
            ImGuiEx.DragFloat("BG Opacity###bgOpacity", Plugin.Config, nameof(Plugin.Config.OverlayBGOpacity), 0.05f, 0f, 1f);
            ImGuiEx.DragFloat("Font Scale###fontScale", Plugin.Config, nameof(Plugin.Config.FontScale), 0.05f, 0f, 2f);

            ImGuiEx.ColorEdit4("Target###colTar", Plugin.Config, nameof(Plugin.Config.TargetColour));
            ImGuiEx.ColorEdit4("No Target###colNoTar", Plugin.Config, nameof(Plugin.Config.NoTargetColour));

            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if(ImGui.Button("Save")) {
                Plugin.Config.Save();
            }
            ImGui.SameLine();
            if(ImGui.Button("Close")) {
                IsOpen = false;
            }
        }
    }
}
