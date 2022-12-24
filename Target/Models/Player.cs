using System;
using Dalamud.Game.ClientState.Objects.Types;

namespace Target {
    public class Player {
        //public GameObject? Object { get; set; }
        //public string Name { get { return Object == null ? "" : Object.Name.TextValue; } }
        //public bool IsTargeting { get { return Object != null && Object.TargetObjectId == Plugin.ClientState.LocalPlayer?.ObjectId; } }
        public string Name { get; set; }
        public DateTime TargetTime { get; set; }

        public Player(string name) {
            Name = name;
            TargetTime = DateTime.Now;
        }
    }
}
