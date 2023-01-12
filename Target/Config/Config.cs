using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace Target {

    [Serializable]
    public class Config : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public bool Enabled { get; set; } = true;
        public bool ClickThrough { get; set; } = false;
        public bool LockPosition { get; set; } = false;
        public bool LClickTarget { get; set; } = true;
        public bool RClickRemove { get; set; } = true;
        public bool MClickInspect { get; set; } = true;
        public bool ChatAlert { get; set; } = false;
        public int SoundID { get; set; } = 16;
        public bool NoDutyAllyAlert { get; set; } = true;
        public bool NoDutyEnemyAlert { get; set; } = false;
        public bool PvEAllyAlert { get; set; } = true;
        public bool PvEEnemyAlert { get; set; } = false;
        public bool PvPAllyAlert { get; set; } = false;
        public bool PvPEnemyAlert { get; set; } = true;
        public bool OnlyShowNearbyPlayers { get; set; } = false;
        public int MaxPlayers { get; set; } = 4;
        public int UpdateMs { get; set; } = 250;
        public int DisplayTime { get; set; } = 0;
        public int OverlayWidth { get; set; } = 160;
        public int OverlayHeight { get; set; } = 100;
        public float OverlayBGOpacity { get; set; } = 0.5f;
        public float FontScale { get; set; } = 1f;

        public float MarkerSize { get; set; } = 3f;
        public bool OnlyShowMarkerOnHover { get; set; } = true;
        public Vector4 MarkerColour { get; set; } = new(255 / 255.0f, 0 / 255.0f, 0 / 255.0f, 0.8f);

        public Vector4 TargetColour { get; set; } = new(220 / 255.0f, 220 / 255.0f, 220 / 255.0f, 0.8f);
        public Vector4 NoTargetColour { get; set; } = new(140 / 255.0f, 140 / 255.0f, 140 / 255.0f, 0.8f);

        [NonSerialized] private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            PluginInterface = pluginInterface;
        }

        public void Save() {
            PluginInterface!.SavePluginConfig(this);
        }
    }
}
