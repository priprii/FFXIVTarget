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
using System.Threading;
using XivCommon;

namespace Target {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "Target";
        private const string CommandName = "/tar";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static ObjectTable Objects { get; private set; } = null!;
        [PluginService] public static TargetManager Targets { get; private set; } = null!;

        public static Config Config { get; set; }
        private static XivCommonBase XIVCommon;
        private WindowSystem Windows;
        private static MainWindow MainWindow;
        public static OverlayWindow OverlayWindow;
        private DateTime LastUpdateTime = DateTime.Now;

        public List<Player> TargetList = new List<Player>();

        public Plugin() {
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open Target Interface."
            });

            PluginInterface.UiBuilder.OpenConfigUi += () => {
                MainWindow.IsOpen = true;
            };

            Config = PluginInterface.GetPluginConfig() as Config ?? new Config();
            Config.Initialize(PluginInterface);

            XIVCommon = new XivCommonBase();
            Windows = new WindowSystem(Name);
            MainWindow = new MainWindow(this) { IsOpen = false };
            OverlayWindow = new OverlayWindow(this) { IsOpen = false };
            Windows.AddWindow(MainWindow);
            Windows.AddWindow(OverlayWindow);

            PluginInterface.UiBuilder.Draw += Windows.Draw;

            Framework.Update += Framework_Update;
        }

        private unsafe void Framework_Update(Framework framework) {
            if(Config.Enabled && ClientState.LocalPlayer != null) {
                if(LastUpdateTime.AddMilliseconds(Config.UpdateMs) < DateTime.Now) {
                    LastUpdateTime = DateTime.Now;

                    if(!OverlayWindow.IsOpen) { OverlayWindow.IsOpen = true; }
                    foreach(GameObject o in Objects) {
                        if(o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) {
                            if(o.TargetObjectId == ClientState.LocalPlayer.ObjectId) {
                                Player? tlP = TargetList.Find(x => x.Name == o.Name.TextValue);
                                if(tlP != null) {
                                    if(tlP.TargetTime.AddSeconds(10) < DateTime.Now) {
                                        PlaySound(tlP.Name);
                                    }
                                    tlP.TargetTime = DateTime.Now;
                                } else {
                                    tlP = new Player(o.Name.TextValue);
                                    PlaySound(tlP.Name);
                                    if(TargetList.Count + 1 > Config.MaxPlayers) { try { TargetList.RemoveAt(TargetList.Count - 1); } catch { } }
                                    TargetList.Add(tlP);
                                }
                                TargetList.Sort((x, y) => y.TargetTime.CompareTo(x.TargetTime));
                            }
                        }
                    }
                }
            }
        }

        private void PlaySound(string name) {
            XIVCommon.Functions.Chat.SendMessage($"/echo Targeted by {name} <se.16>");
        }

        public void Dispose() {
            Framework.Update -= Framework_Update;
            PluginInterface.UiBuilder.Draw -= Windows.Draw;
            CommandManager.RemoveHandler(CommandName);
            XIVCommon.Dispose();
        }

        private void OnCommand(string command, string args) {
            MainWindow.IsOpen = true;
        }
    }
}
