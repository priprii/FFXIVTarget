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
            this.Size = new Vector2(485, 455) * ImGuiHelpers.GlobalScale;
        }

        public override void OnOpen() {
            base.OnOpen();
        }

        public override void Draw() {
            ImGui.Columns(3, "", false);
            if(ImGuiEx.Checkbox("Enable###toggleEnabled", Plugin.Config, nameof(Plugin.Config.Enabled))) {
                Plugin.OverlayWindow.IsOpen = Plugin.Config.Enabled;
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Display list of players targeting you in an overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("Locked###lockPos", Plugin.Config, nameof(Plugin.Config.LockPosition));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Prevent moving the overlay when dragged.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("ClickThrough###clickThrough", Plugin.Config, nameof(Plugin.Config.ClickThrough));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Prevent click events on the overlay.");
            }
            ImGui.NextColumn();

            ImGuiEx.Checkbox("LClick: Target###lClick", Plugin.Config, nameof(Plugin.Config.LClickTarget));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Left-click name in overlay to select target.\n(ClickThrough must be disabled)");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("RClick: Remove###lClick", Plugin.Config, nameof(Plugin.Config.RClickRemove));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Right-click name in overlay to remove from the list.\n(ClickThrough must be disabled)");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("MClick: Inspect###mClick", Plugin.Config, nameof(Plugin.Config.MClickInspect));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Middle-click name in overlay to inspect.\n(ClickThrough must be disabled)");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 80);
            if(ImGuiEx.InputInt("Audio Alert###audioAlert", Plugin.Config, nameof(Plugin.Config.SoundID))) {
                if(Plugin.Config.SoundID < 0) {
                    Plugin.Config.SoundID = 0;
                } else if(Plugin.Config.SoundID > 16) {
                    Plugin.Config.SoundID = 16;
                }
                plugin.PlaySound(Plugin.Config.SoundID);
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("The audio alert to play when being targeted.\nSet to 0 to disable audio.");
            }
            ImGui.NextColumn();
            ImGui.Dummy(Vector2.Zero);
            ImGui.NextColumn();
            ImGui.Dummy(Vector2.Zero);
            ImGui.NextColumn();

            ImGuiEx.Checkbox("NoDuty Ally Alert###ndAlly", Plugin.Config, nameof(Plugin.Config.NoDutyAllyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an ally in non-duty content targets you.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvE Ally Alert###pveAlly", Plugin.Config, nameof(Plugin.Config.PvEAllyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an ally in a PvE duty targets you.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvP Ally Alert###pvpAlly", Plugin.Config, nameof(Plugin.Config.PvPAllyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an ally in a PvP duty targets you.");
            }
            ImGui.NextColumn();

            ImGuiEx.Checkbox("NoDuty Enemy Alert###ndEnemy", Plugin.Config, nameof(Plugin.Config.NoDutyEnemyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an enemy in non-duty content targets you.\nThis also toggles whether enemies appear in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvE Enemy Alert###pveEnemy", Plugin.Config, nameof(Plugin.Config.PvEEnemyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an enemy in a PvE duty targets you.\nThis also toggles whether enemies appear in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvP Enemy Alert###pvpEnemy", Plugin.Config, nameof(Plugin.Config.PvPEnemyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an enemy in a PvP duty targets you.\nThis can be pretty noisy in Frontlines,\nbut I think it's nice to know when someone wants to murder you ;w;");
            }
            ImGui.NextColumn();

            ImGui.Separator();
            ImGui.Columns(2, "", false);

            ImGuiEx.Checkbox("Only Nearby Players###onlyNearby", Plugin.Config, nameof(Plugin.Config.OnlyShowNearbyPlayers));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Only players within range will be visible in the overlay.\nThey will be hidden if too far away or in a different zone.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Max Players###maxPlayers", Plugin.Config, nameof(Plugin.Config.MaxPlayers), 1, 1, 50);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Max number of players to display in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Duration###displayTime", Plugin.Config, nameof(Plugin.Config.DisplayTime), 1, 0, 1440);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Number of minutes until a name of a player no longer targeting you is removed from the overlay.\nSet to 0 to never automatically remove names.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Update Ms###updateMs", Plugin.Config, nameof(Plugin.Config.UpdateMs), 10, 10, 5000);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Polling rate to check for players targeting you.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGuiEx.DragInt("Width###overlayWidth", Plugin.Config, nameof(Plugin.Config.OverlayWidth), 1, 40, 800);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Width of the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Height###overlayHeight", Plugin.Config, nameof(Plugin.Config.OverlayHeight), 1, 40, 800);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Height of the overlay.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGuiEx.DragFloat("BG Opacity###bgOpacity", Plugin.Config, nameof(Plugin.Config.OverlayBGOpacity), 0.01f, 0f, 1f);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Background opacity of the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragFloat("Font Scale###fontScale", Plugin.Config, nameof(Plugin.Config.FontScale), 0.01f, 0f, 2f);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Scaling of player names in the overlay.");
            }
            ImGui.NextColumn();

            ImGui.Separator();
            ImGui.Columns(2, "", false);

            ImGuiEx.Checkbox("Marker Only on Hover", Plugin.Config, nameof(Plugin.Config.OnlyShowMarkerOnHover));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("If enabled, marker on players targeting you will only be visible when you hover over their name in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragFloat("Marker Size###sizeMarker", Plugin.Config, nameof(Plugin.Config.MarkerSize), 0.05f, 0f, 20f);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Size of the marker on players targeting you.\nSet to 0 to disable.");
            }
            ImGui.NextColumn();

            ImGui.Columns(1, "", false);
            ImGuiEx.ColorEdit4("Marker Colour###colMarker", Plugin.Config, nameof(Plugin.Config.MarkerColour));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Colour of marker on players targeting you.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGuiEx.ColorEdit4("Target###colTar", Plugin.Config, nameof(Plugin.Config.TargetColour));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Colour of names in the overlay for players currently targeting you.");
            }
            ImGui.NextColumn();
            ImGuiEx.ColorEdit4("No Target###colNoTar", Plugin.Config, nameof(Plugin.Config.NoTargetColour));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Colour of names in the overlay for players previously targeting you.");
            }
            ImGui.NextColumn();

            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if(ImGui.Button("Save")) {
                Plugin.Config.Save();
                IsOpen = false;
            }
            ImGui.SameLine();
            if(ImGui.Button("Close")) {
                IsOpen = false;
            }
        }
    }
}
