using System;
using Dalamud.Game.ClientState.Objects.Types;

namespace Target {
    public class Player {
        public GameObject? Object { get; set; }
        public string Name { get; set; }
        public DateTime TargetTime { get; set; }

        public bool Exists => Object != null && Object.IsValid() && Object.Name.TextValue == Name;
        public bool TargetExists => Exists && Object?.TargetObject != null && Object.TargetObject.IsValid();

        public Player(string name) {
            Name = name;
            TargetTime = DateTime.Now;
        }
    }
}
