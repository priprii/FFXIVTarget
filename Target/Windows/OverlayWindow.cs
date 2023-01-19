using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState;
using System.Linq;
using System.Collections.Generic;

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

        public unsafe override void Draw() {
            if(Plugin.Config.Enabled && Plugin.ClientState.LocalPlayer != null) {
                Flags = AutoResize;
                if(Plugin.Config.LockPosition) { Flags |= ImGuiWindowFlags.NoMove; }
                if(Plugin.Config.ClickThrough) { Flags |= ImGuiWindowFlags.NoInputs; }
                Size = new Vector2(Plugin.Config.OverlayWidth, Plugin.Config.OverlayHeight);

                if(plugin.TargetList.Count == 0) { return; }
                try {
                    foreach(Player p in plugin.TargetList) {
                        if(Plugin.Config.OnlyShowNearbyPlayers && !p.Exists) { continue; }

                        ImGui.SetWindowFontScale(Plugin.Config.FontScale);
                        ImGui.TextColored(p.Exists && p.Object?.TargetObjectId == Plugin.ClientState.LocalPlayer?.ObjectId ? Plugin.Config.TargetColour : Plugin.Config.NoTargetColour, $"[{p.TargetTime.Hour.ToString("00")}:{p.TargetTime.Minute.ToString("00")}] {p.Name}");

                        if(Plugin.Config.RClickRemove && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                            plugin.TargetList.Remove(p);
                        } else if(p.Exists) {
                            HandleOverlayItemInteractions(p.Object);

                            if(p.Object.ObjectKind == ObjectKind.Player) {
                                string tTooltip = "";

                                if(Plugin.Config.ShowTarget > 0 && p.TargetExists && p.Object.TargetObjectId != Plugin.ClientState.LocalPlayer?.ObjectId && p.Object?.TargetObject?.ObjectKind == ObjectKind.Player) {
                                    if(Plugin.Config.ShowTarget == 1) {
                                        ImGui.Indent(30 * ImGuiHelpers.GlobalScale);

                                        string dir = p.Object?.TargetObject?.TargetObjectId == p.Object?.ObjectId ? "⇔" : "→";
                                        ImGui.TextColored(p.Object?.TargetObject?.TargetObject != null && p.Object?.TargetObject?.TargetObjectId == Plugin.ClientState.LocalPlayer?.ObjectId ? Plugin.Config.TargetColour : Plugin.Config.NoTargetColour, $"{dir}{p.Object?.TargetObject?.Name.TextValue}");
                                        HandleOverlayItemInteractions(p.Object?.TargetObject);

                                        ImGui.Indent(-30 * ImGuiHelpers.GlobalScale);
                                    } else if(ImGui.IsItemHovered()) {
                                        tTooltip = $"Targeting: {p.Object?.TargetObject?.Name.TextValue}";
                                    }
                                }
                                
                                if(Plugin.Config.ShowTargeters > 0) {
                                    IEnumerable<GameObject> objs = Plugin.Objects.Where(o => o.Name.TextValue != Plugin.ClientState.LocalPlayer?.Name.TextValue && o.ObjectKind == ObjectKind.Player && o.TargetObjectId == p.Object?.ObjectId);
                                    if(objs != null && objs.Count() > 0) {
                                        if(Plugin.Config.ShowTargeters == 1) {
                                            ImGui.Indent(30 * ImGuiHelpers.GlobalScale);
                                            foreach(GameObject o in objs) {
                                                if(p.Object?.TargetObjectId != o.ObjectId || Plugin.Config.ShowTarget != 1) {
                                                    string dir = p.Object?.TargetObjectId == o.ObjectId ? "⇔" : "←";
                                                    ImGui.TextColored(Plugin.Config.NoTargetColour, $"{dir}{o.Name.TextValue}");
                                                    HandleOverlayItemInteractions(p.Object?.TargetObject);
                                                }
                                            }
                                            ImGui.Indent(-30 * ImGuiHelpers.GlobalScale);
                                        } else if(ImGui.IsItemHovered()) {
                                            if(tTooltip != "") { tTooltip += "\n"; }
                                            tTooltip += $"Targeted By:\n{string.Join('\n', objs.Select(x => x.Name.TextValue))}";
                                        }
                                    }
                                }

                                if(tTooltip != "") {
                                    ImGui.SetTooltip(tTooltip);
                                }
                            }
                        }
                    }
                } catch { }

                ImGui.Spacing();
            }
        }

        private unsafe void HandleOverlayItemInteractions(GameObject o) {
            if(o == null || !o.IsValid()) { return; }

            if(Plugin.Config.MarkerSize > 0f && o.ObjectKind == ObjectKind.Player) {
                bool isHovered = ImGui.IsItemHovered();
                if((isHovered && Plugin.Config.OnlyShowMarkerOnHover) || (!Plugin.Config.OnlyShowMarkerOnHover && o.TargetObjectId == Plugin.ClientState.LocalPlayer?.ObjectId)) {
                    if(o is PlayerCharacter pc) {
                        if(!pc.StatusFlags.HasFlag(StatusFlags.Hostile)) {
                            MarkPlayer(o, Plugin.Config.MarkerColour, Plugin.Config.MarkerSize);
                        }
                    }
                }
            }

            if(Plugin.Config.LClickTarget && ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
                Plugin.Targets.Target = o;
            } else if(Plugin.Config.MClickInspect && ImGui.IsItemClicked(ImGuiMouseButton.Middle) && o.ObjectKind == ObjectKind.Player) {
                if(Input.IsCtrlDown) {
                    Plugin.Targets.MouseOverTarget = o;
                    Plugin.Chat.SendMessage($"/check <mo>");
                } else {
                    FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* objAddr = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)o.Address;
                    if(objAddr != null && objAddr->ObjectKind == 1 && objAddr->SubKind == 4) {
                        AgentCharaCard.Instance()->OpenCharaCard(objAddr);
                    }
                }
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
