using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;

namespace Target {
    public class OverlayWindow : Window {
        private readonly Plugin plugin;

        private const ImGuiWindowFlags AutoResize = ImGuiWindowFlags.NoFocusOnAppearing |
                                                       ImGuiWindowFlags.NoTitleBar |
                                                       ImGuiWindowFlags.NoScrollbar |
                                                       ImGuiWindowFlags.NoCollapse |
                                                       ImGuiWindowFlags.AlwaysAutoResize;

        public OverlayWindow(Plugin plugin) : base("TargetOverlay", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
            this.plugin = plugin;
            this.SizeConstraints = new WindowSizeConstraints {
                MinimumSize = new Vector2(20, 20),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }

        public override void PreDraw() {
            var bgColor = ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg];
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(bgColor.X, bgColor.Y, bgColor.Z, Plugin.Config.OverlayBGOpacity));

            var borderColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Border];
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(borderColor.X, borderColor.Y, borderColor.Z, Plugin.Config.OverlayBGOpacity));
        }

        public override void Draw() {
            //todo: Conditions
            if(Plugin.Config.Enabled && Plugin.ClientState.LocalPlayer != null) {
                Flags = AutoResize;
                if(Plugin.Config.LockPosition) { Flags |= ImGuiWindowFlags.NoMove; }
                if(Plugin.Config.ClickThrough) { Flags |= ImGuiWindowFlags.NoInputs; }
                Size = new Vector2(Plugin.Config.OverlayWidth, Plugin.Config.OverlayHeight);

                if(plugin.TargetList.Count == 0) { return; }
                try {
                    foreach(Player p in plugin.TargetList) {
                        GameObject pO = null;
                        foreach(GameObject o in Plugin.Objects) {
                            if(o.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player && o.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc) { continue; }
                            if(o.Name.TextValue == p.Name) { pO = o; break; }
                        }
                        if(pO == null) { continue; }

                        ImGui.SetWindowFontScale(Plugin.Config.FontScale);
                        ImGui.TextColored(pO != null && pO.TargetObjectId == Plugin.ClientState.LocalPlayer?.ObjectId ? Plugin.Config.TargetColour : Plugin.Config.NoTargetColour, $"[{p.TargetTime.Hour.ToString("00")}:{p.TargetTime.Minute.ToString("00")}] {p.Name}");

                        if(Plugin.Config.MarkerSize > 0f && pO.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player && pO != null && pO.TargetObjectId == Plugin.ClientState.LocalPlayer?.ObjectId) {
                            if(pO is PlayerCharacter pc) {
                                if(!pc.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.Hostile)) {
                                    MarkPlayer(pO, Plugin.Config.MarkerColour, Plugin.Config.MarkerSize);
                                }
                            }
                        }

                        if(Plugin.Config.LClickTarget && pO != null && ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
                            Plugin.Targets.Target = pO;
                        } else if(Plugin.Config.RClickRemove && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                            plugin.TargetList.Remove(p);
                        } else if(Plugin.Config.MClickInspect && pO != null && ImGui.IsItemClicked(ImGuiMouseButton.Middle)) {
                            Plugin.Targets.MouseOverTarget = pO;
                            Plugin.XIVCommon.Functions.Chat.SendMessage($"/check <mo>");
                        }
                    }
                } catch { }

                ImGui.Spacing();
            }
        }

        private void MarkPlayer(GameObject? player, Vector4 colour, float size) {
            if(player == null) { return; }
            if(!Plugin.GameGui.WorldToScreen(player.Position, out var screenPos)) { return; }

            ImGui.PushClipRect(ImGuiHelpers.MainViewport.Pos, ImGuiHelpers.MainViewport.Pos + ImGuiHelpers.MainViewport.Size, false);
            ImGui.GetWindowDrawList().AddCircleFilled(ImGuiHelpers.MainViewport.Pos + new Vector2(screenPos.X, screenPos.Y), size, ImGui.GetColorU32(colour), 100);
            ImGui.PopClipRect();
        }

        public override void PostDraw() {
            ImGui.PopStyleColor(2);
        }
    }
}
