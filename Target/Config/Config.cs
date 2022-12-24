using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace Target {

    [Serializable]
    public class Config : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public bool Enabled { get; set; } = false;
        public bool ClickThrough { get; set; } = false;
        public bool LockPosition { get; set; } = false;
        public bool LClickTarget { get; set; } = true;
        public bool RClickRemove { get; set; } = true;
        public bool PlaySound { get; set; } = true;

        public int MaxPlayers { get; set; } = 4;
        public int UpdateMs { get; set; } = 250;
        public int OverlayWidth { get; set; } = 160;
        public int OverlayHeight { get; set; } = 100;
        public float OverlayBGOpacity { get; set; } = 0.5f;
        public float FontScale { get; set; } = 1f;

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
