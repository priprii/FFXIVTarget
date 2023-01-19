using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Target.Extensions;

namespace Target {
    public class MainWindow : Window {
        private readonly Plugin plugin;

        public enum Tab { Config, Players }
        private Tab CurrentTab = Tab.Config;

        public MainWindow(Plugin plugin) : base("Target") {
            this.plugin = plugin;
            this.SizeCondition = ImGuiCond.Appearing;
            this.Size = new Vector2(500, 540) * ImGuiHelpers.GlobalScale;
        }

        public override void OnOpen() {
            base.OnOpen();
        }

        public override void Draw() {
            if(ImGui.BeginTabBar("TargetTabBar", ImGuiTabBarFlags.NoTooltip)) {
                if(ImGui.BeginTabItem("Config###Target_ConfigTab")) {
                    CurrentTab = Tab.Config;
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Players###Target_PlayersTab")) {
                    CurrentTab = Tab.Players;
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
                ImGui.Spacing();
            }

            switch(CurrentTab) {
                case Tab.Config: {
                        DrawConfig();
                        break;
                    }
                case Tab.Players: {
                        DrawPlayers();
                        break;
                    }
                default:
                    DrawConfig();
                    break;
            }
        }

        private void DrawConfig() {
            ImGui.Columns(3, "", false);
            if(ImGuiEx.Checkbox("Enable##toggleEnabled", Plugin.Config, nameof(Plugin.Config.Enabled))) {
                Plugin.OverlayWindow.IsOpen = Plugin.Config.Enabled;
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Display list of players targeting you in an overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("Locked##lockPos", Plugin.Config, nameof(Plugin.Config.LockPosition));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Prevent moving the overlay when dragged.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("ClickThrough##clickThrough", Plugin.Config, nameof(Plugin.Config.ClickThrough));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Prevent click events on the overlay.");
            }
            ImGui.NextColumn();

            ImGuiEx.Checkbox("LClick: Target##lClick", Plugin.Config, nameof(Plugin.Config.LClickTarget));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Left-click name in overlay to select target.\n(ClickThrough must be disabled)");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("RClick: Remove##lClick", Plugin.Config, nameof(Plugin.Config.RClickRemove));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Right-click name in overlay to remove from the list.\n(ClickThrough must be disabled)");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("MClick: Inspect##mClick", Plugin.Config, nameof(Plugin.Config.MClickInspect));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Middle-click name in overlay to open adventure plate.\nCtrl+Middle-click to open character window.\n(ClickThrough must be disabled)");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 80);
            if(ImGuiEx.InputInt("Audio Alert##audioAlert", Plugin.Config, nameof(Plugin.Config.SoundID))) {
                if(Plugin.Config.SoundID < 0) {
                    Plugin.Config.SoundID = 0;
                } else if(Plugin.Config.SoundID > 16) {
                    Plugin.Config.SoundID = 16;
                }
                plugin.PlaySound("");
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("The audio alert to play when being targeted.\nSet to 0 to disable audio.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("Chat Alert##chatAlert", Plugin.Config, nameof(Plugin.Config.ChatAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Output a message to the chatlog when being targeted.\nThis option is ignored in PvP duties.");
            }
            ImGui.NextColumn();
            ImGui.Dummy(Vector2.Zero);
            ImGui.NextColumn();

            ImGuiEx.Checkbox("NoDuty Ally Alert##ndAlly", Plugin.Config, nameof(Plugin.Config.NoDutyAllyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an ally in non-duty content targets you.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvE Ally Alert##pveAlly", Plugin.Config, nameof(Plugin.Config.PvEAllyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an ally in a PvE duty targets you.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvP Ally Alert##pvpAlly", Plugin.Config, nameof(Plugin.Config.PvPAllyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an ally in a PvP duty targets you.");
            }
            ImGui.NextColumn();

            ImGuiEx.Checkbox("NoDuty Enemy Alert##ndEnemy", Plugin.Config, nameof(Plugin.Config.NoDutyEnemyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an enemy in non-duty content targets you.\nThis also toggles whether enemies appear in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvE Enemy Alert##pveEnemy", Plugin.Config, nameof(Plugin.Config.PvEEnemyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an enemy in a PvE duty targets you.\nThis also toggles whether enemies appear in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.Checkbox("PvP Enemy Alert##pvpEnemy", Plugin.Config, nameof(Plugin.Config.PvPEnemyAlert));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play audio alert when an enemy in a PvP duty targets you.\nThis can be pretty noisy in Frontlines,\nbut I think it's nice to know when someone wants to murder you ;w;");
            }
            ImGui.NextColumn();

            ImGui.Separator();
            ImGui.Columns(2, "", false);

            ImGuiEx.Checkbox("Only Nearby Players##onlyNearby", Plugin.Config, nameof(Plugin.Config.OnlyShowNearbyPlayers));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Only players within range will be visible in the overlay.\nThey will be hidden if too far away or in a different zone.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Max Players##maxPlayers", Plugin.Config, nameof(Plugin.Config.MaxPlayers), 1, 1, 50);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Max number of players to display in the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Duration##displayTime", Plugin.Config, nameof(Plugin.Config.DisplayTime), 1, 0, 1440);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Number of minutes until a name of a player no longer targeting you is removed from the overlay.\nSet to 0 to never automatically remove names.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Update Ms##updateMs", Plugin.Config, nameof(Plugin.Config.UpdateMs), 10, 10, 5000);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Polling rate to check for players targeting you.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.LabelText("##showTarget", "Show Target");
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Show who each player listed in the overlay is targeting, when they're not targeting you.");
            }
            ImGui.NextColumn();

            if(ImGui.RadioButton("Disabled##st0", Plugin.Config.ShowTarget == 0)) { Plugin.Config.ShowTarget = 0; }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Do not show who each player is targeting when they're not targeting you."); }
            ImGui.SameLine();
            if(ImGui.RadioButton("Overlay##st1", Plugin.Config.ShowTarget == 1)) { Plugin.Config.ShowTarget = 1; }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Show under the player's name in the overlay."); }
            ImGui.SameLine();
            if(ImGui.RadioButton("Hover##st2", Plugin.Config.ShowTarget == 2)) { Plugin.Config.ShowTarget = 2; }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Show as a tooltip when player's name is hovered in the overlay."); }
            ImGui.NextColumn();

            ImGui.LabelText("##showTargeters", "Show Targeters");
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Show who is targeting each player listed in the overlay.");
            }
            ImGui.NextColumn();

            if(ImGui.RadioButton("Disabled##stt0", Plugin.Config.ShowTargeters == 0)) { Plugin.Config.ShowTargeters = 0; }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Do not show who is targeting each player in the overlay."); }
            ImGui.SameLine();
            if(ImGui.RadioButton("Overlay##stt1", Plugin.Config.ShowTargeters == 1)) { Plugin.Config.ShowTargeters = 1; }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Show under the player's name in the overlay."); }
            ImGui.SameLine();
            if(ImGui.RadioButton("Hover##stt2", Plugin.Config.ShowTargeters == 2)) { Plugin.Config.ShowTargeters = 2; }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Show as a tooltip when player's name is hovered in the overlay."); }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGuiEx.DragInt("Width##overlayWidth", Plugin.Config, nameof(Plugin.Config.OverlayWidth), 1, 40, 800);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Width of the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragInt("Height##overlayHeight", Plugin.Config, nameof(Plugin.Config.OverlayHeight), 1, 40, 800);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Height of the overlay.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGuiEx.DragFloat("BG Opacity##bgOpacity", Plugin.Config, nameof(Plugin.Config.OverlayBGOpacity), 0.01f, 0f, 1f);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Background opacity of the overlay.");
            }
            ImGui.NextColumn();
            ImGuiEx.DragFloat("Font Scale##fontScale", Plugin.Config, nameof(Plugin.Config.FontScale), 0.01f, 0f, 2f);
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
            ImGuiEx.DragFloat("Marker Size##sizeMarker", Plugin.Config, nameof(Plugin.Config.MarkerSize), 0.05f, 0f, 20f);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Size of the marker on players targeting you.\nSet to 0 to disable.");
            }
            ImGui.NextColumn();

            ImGui.Columns(1, "", false);
            ImGuiEx.ColorEdit4("Marker Colour##colMarker", Plugin.Config, nameof(Plugin.Config.MarkerColour));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Colour of marker on players targeting you.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGuiEx.ColorEdit4("Target##colTar", Plugin.Config, nameof(Plugin.Config.TargetColour));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Colour of names in the overlay for players currently targeting you.");
            }
            ImGui.NextColumn();
            ImGuiEx.ColorEdit4("No Target##colNoTar", Plugin.Config, nameof(Plugin.Config.NoTargetColour));
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
    
        private unsafe void DrawPlayers() {
            ImGui.Columns(6);
            ImGui.SetColumnWidth(0, 100 + 5 * ImGuiHelpers.GlobalScale); //Name
            ImGui.SetColumnWidth(1, 44 + 5 * ImGuiHelpers.GlobalScale); //Visible
            ImGui.SetColumnWidth(2, 100 + 5 * ImGuiHelpers.GlobalScale); //Targeting
            ImGui.SetColumnWidth(3, 70 + 5 * ImGuiHelpers.GlobalScale); //Target
            ImGui.SetColumnWidth(4, 70 + 5 * ImGuiHelpers.GlobalScale); //Plate
            ImGui.SetColumnWidth(5, 70 + 5 * ImGuiHelpers.GlobalScale); //Inspect

            ImGui.Text("Name");
            ImGui.NextColumn();
            ImGui.Text("Visible");
            ImGui.NextColumn();
            ImGui.Text("Targeting");
            ImGui.NextColumn();
            ImGui.Text(""); //Target
            ImGui.NextColumn();
            ImGui.Text(""); //Plate
            ImGui.NextColumn();
            ImGui.Text(""); //Inspect
            ImGui.NextColumn();

            ImGui.Separator();

            List<Dalamud.Game.ClientState.Objects.Types.GameObject> objs = Plugin.Objects.ToList();
            objs.Sort((x, y) => x.Name.TextValue.CompareTo(y.Name.TextValue));

            foreach(Dalamud.Game.ClientState.Objects.Types.GameObject o in objs) {
                if(!string.IsNullOrEmpty(o.Name.TextValue)) { //o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player
                    GameObject* gameObject = (GameObject*)o.Address;

                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text($"{o.Name.TextValue}");
                    ImGui.NextColumn();

                    ImGui.SetNextItemWidth(-1);
                    bool isVisible = IsVisible(gameObject);
                    ImGui.Checkbox("##visibleCheck", ref isVisible);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
                        if(IsVisible(gameObject)) {
                            HideGameObject(gameObject);
                        } else {
                            ShowGameObject(gameObject);
                            
                        }
                    }
                    if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Toggle visibility of this player.\nNote: This is only temporary, hidden players will be visible again if they reload into same zone as you."); }
                    ImGui.NextColumn();

                    ImGui.SetNextItemWidth(-1);
                    if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) {
                        ImGui.Text($"{(o.TargetObject != null && o.TargetObject.IsValid() ? $"{o.TargetObject.Name.TextValue}" : "")}");
                    }
                    ImGui.NextColumn();

                    ImGui.SetNextItemWidth(-1);
                    if(ImGui.Button($"Target##tBtn{o.ObjectId}")) {
                        Plugin.Targets.Target = o;
                    }
                    if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Target this player."); }
                    ImGui.NextColumn();

                    ImGui.SetNextItemWidth(-1);
                    if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) {
                        if(ImGui.Button($"Plate##pBtn{o.ObjectId}")) {
                            if(gameObject != null && gameObject->ObjectKind == 1 && gameObject->SubKind == 4) {
                                AgentCharaCard.Instance()->OpenCharaCard(gameObject);
                            }
                        }
                        if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Open this player's Adventure Plate."); }
                    }
                    ImGui.NextColumn();

                    ImGui.SetNextItemWidth(-1);
                    if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) {
                        if(ImGui.Button($"Inspect##iBtn{o.ObjectId}")) {
                            Plugin.Targets.MouseOverTarget = o;
                            Plugin.Chat.SendMessage($"/check <mo>");
                        }
                        if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Inspect this player."); }
                    }
                    ImGui.NextColumn();
                }
            }

            ImGui.Columns(1, "", false);

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

        private unsafe bool IsVisible(GameObject* thisPtr) {
            return !thisPtr->RenderFlags.TestFlag(VisibilityFlags.Invisible);
        }
        private unsafe void HideGameObject(GameObject* thisPtr) {
            if(IsVisible(thisPtr)) {
                thisPtr->DisableDraw();
                thisPtr->RenderFlags |= (int)VisibilityFlags.Invisible;
            }
        }
        private unsafe void ShowGameObject(GameObject* thisPtr) {
            if(!IsVisible(thisPtr)) {
                thisPtr->RenderFlags &= ~(int)VisibilityFlags.Invisible;
                thisPtr->EnableDraw();
            }
        }
    }
}
