using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Game.ClientState;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using Dalamud.Game.Gui;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Target {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "Target";
        private const string CommandName = "/tar";
        private const string AltCommandName = "/targetpyon";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static ObjectTable Objects { get; private set; } = null!;
        [PluginService] public static TargetManager Targets { get; private set; } = null!;
        [PluginService] public static ChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static SigScanner SigScanner { get; private set; } = null!;

        public static Config Config { get; set; }
        public static Chat Chat;
        private WindowSystem Windows;
        private static MainWindow MainWindow;
        public static OverlayWindow OverlayWindow;
        private DateTime LastUpdateTime = DateTime.Now;
        private readonly ExcelSheet<ContentFinderCondition> ContentFinderConditionsSheet;
        public ContentTypes ContentType = ContentTypes.NoDuty;
        public enum ContentTypes {
            NoDuty,
            PvEDuty,
            PvPDuty
        }

        public List<Player> TargetList = new List<Player>();

        public Plugin() {
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open Target Interface."
            });
            CommandManager.AddHandler(AltCommandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open Target Interface."
            });

            PluginInterface.UiBuilder.OpenConfigUi += () => {
                MainWindow.IsOpen = true;
            };

            Config = PluginInterface.GetPluginConfig() as Config ?? new Config();
            Config.Initialize(PluginInterface);

            Chat = new Chat(SigScanner);
            Windows = new WindowSystem(Name);
            MainWindow = new MainWindow(this) { IsOpen = false };
            OverlayWindow = new OverlayWindow(this) { IsOpen = false };
            Windows.AddWindow(MainWindow);
            Windows.AddWindow(OverlayWindow);

            PluginInterface.UiBuilder.Draw += Windows.Draw;
            Framework.Update += Framework_Update;
            ChatGui.ChatMessage += OnChatMessage;
            ClientState.TerritoryChanged += OnTerritoryChanged;
        }

        private void OnTerritoryChanged(object? sender, ushort e) {
            if(ContentFinderConditionsSheet == null) { return; }
            ContentFinderCondition content = ContentFinderConditionsSheet.FirstOrDefault(c => c.TerritoryType.Row == ClientState.TerritoryType);
            if(content == null) {
                ContentType = ContentTypes.NoDuty;
            } else {
                if(content.PvP) {
                    ContentType = ContentTypes.PvPDuty;
                } else {
                    ContentType = ContentTypes.PvEDuty;
                }
            }
        }

        private void OnChatMessage(Dalamud.Game.Text.XivChatType type, uint senderId, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled) {
            if(isHandled || !Config.Enabled || ClientState.LocalPlayer == null) { return; }

            if(message.TextValue.Contains("[TargetPyon]")) {
                isHandled = true;
            }
        }

        private unsafe void Framework_Update(Framework framework) {
            if(Config.Enabled && ClientState.LocalPlayer != null) {
                if(LastUpdateTime.AddMilliseconds(Config.UpdateMs) < DateTime.Now) {
                    LastUpdateTime = DateTime.Now;

                    if(!OverlayWindow.IsOpen) { OverlayWindow.IsOpen = true; }
                    foreach(GameObject o in Objects) {
                        if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player || o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc) {
                            if(o.TargetObjectId == ClientState.LocalPlayer.ObjectId && o.Name.TextValue != ClientState.LocalPlayer.Name.TextValue) {
                                bool playSound = false;
                                if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) {
                                    if(ContentType == ContentTypes.PvPDuty) {
                                        bool isEnemy = false;
                                        if(o is PlayerCharacter p) {
                                            isEnemy = p.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.Hostile);
                                        }
                                        playSound = (isEnemy && Config.PvPEnemyAlert) || (!isEnemy && Config.PvPAllyAlert);
                                    } else {
                                        playSound = (ContentType == ContentTypes.PvEDuty && Config.PvEAllyAlert) || (ContentType == ContentTypes.NoDuty && Config.NoDutyAllyAlert);
                                    }
                                } else if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc) {
                                    if(ContentType == ContentTypes.PvPDuty || (ContentType == ContentTypes.NoDuty && !Config.NoDutyEnemyAlert) || (ContentType == ContentTypes.PvEDuty && !Config.PvEEnemyAlert)) {
                                        continue;
                                    }
                                    playSound = true;
                                }

                                Player? tlP = TargetList.Find(x => x.Name == o.Name.TextValue);
                                if(tlP != null) {
                                    if(tlP.TargetTime.AddSeconds(10) < DateTime.Now && playSound) {
                                        PlaySound(tlP.Name, Config.SoundID);
                                    }
                                    tlP.TargetTime = DateTime.Now;
                                } else {
                                    tlP = new Player(o.Name.TextValue);
                                    if(playSound) { PlaySound(tlP.Name, Config.SoundID); }
                                    if(TargetList.Count + 1 > Config.MaxPlayers) { try { TargetList.RemoveAt(TargetList.Count - 1); } catch { } }
                                    TargetList.Add(tlP);
                                }
                                TargetList.Sort((x, y) => y.TargetTime.CompareTo(x.TargetTime));
                            }
                        }
                    }

                    if(Config.DisplayTime != 0) {
                        TargetList.RemoveAll(x => x.TargetTime.AddMinutes(Config.DisplayTime) < DateTime.Now);
                    }
                }
            }
        }

        public void PlaySound(string name, int soundID) {
            if(soundID > 0 && soundID <= 16) {
                Chat.SendMessage($"/echo [TargetPyon] <se.{soundID}>");
            }
            if(name != "" && Config.ChatAlert && ContentType != ContentTypes.PvPDuty) {
                Chat.SendMessage($"/echo Targeted by {name}");
            }
        }

        public void Dispose() {
            ClientState.TerritoryChanged -= OnTerritoryChanged;
            ChatGui.ChatMessage -= OnChatMessage;
            Framework.Update -= Framework_Update;
            PluginInterface.UiBuilder.Draw -= Windows.Draw;
            CommandManager.RemoveHandler(CommandName);
            CommandManager.RemoveHandler(AltCommandName);
        }

        private void OnCommand(string command, string args) {
            MainWindow.IsOpen = true;
        }
    }
}
