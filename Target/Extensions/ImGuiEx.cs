using System.Numerics;
using ImGuiNET;

namespace Target.Extensions {
    public static class ImGuiEx {
        public static bool ColorEdit4(string label, object obj, string nameofProp) {
            var p = obj.GetType().GetProperty(nameofProp);
            Vector4 x = (Vector4)(p?.GetValue(obj) ?? new Vector4());
            bool r = ImGui.ColorEdit4(label, ref x);
            p?.SetValue(obj, x);
            return r;
        }

        public static bool DragInt(string label, object obj, string nameofProp, float spd, int min, int max) {
            var p = obj.GetType().GetProperty(nameofProp);
            int x = (int)(p?.GetValue(obj) ?? 0);
            bool r = ImGui.DragInt(label, ref x, spd, min, max);
            p?.SetValue(obj, x);
            return r;
        }

        public static bool DragFloat(string label, object obj, string nameofProp, float spd, float min, float max) {
            var p = obj.GetType().GetProperty(nameofProp);
            float x = (float)(p?.GetValue(obj) ?? 0f);
            bool r = ImGui.DragFloat(label, ref x, spd, min, max);
            p?.SetValue(obj, x);
            return r;
        }

        /// <summary>Extension for converting between string/integer for display & updating of a value.</summary>
        public static void InputText(string label, ref int value, uint length = 255) {
            string strVal = value.ToString();
            ImGui.InputText(label, ref strVal, length);
            int.TryParse(strVal, out value);
        }

        public static void InputText(string label, ref double value, uint length = 255) {
            string strVal = value.ToString();
            ImGui.InputText(label, ref strVal, length);
            double.TryParse(strVal, out value);
        }

        /// <summary>Extension to enable passing property as value with referencing behaviour.</summary>
        public static bool InputText(string label, object obj, string nameofProp, uint length = 255) {
            var p = obj.GetType().GetProperty(nameofProp);
            string x = (string)(p?.GetValue(obj) ?? "");
            bool r = ImGui.InputText(label, ref x, length);
            p?.SetValue(obj, x);
            return r;
        }

        /// <summary>Extension to enable passing property as value with referencing behaviour.</summary>
        public static bool InputInt(string label, object obj, string nameofProp) {
            var p = obj.GetType().GetProperty(nameofProp);
            int x = (int)(p?.GetValue(obj) ?? 0);
            bool r = ImGui.InputInt(label, ref x);
            p?.SetValue(obj, x);
            return r;
        }

        /// <summary>Extension to enable passing property as value with referencing behaviour.</summary>
        public static bool InputFloat(string label, object obj, string nameofProp) {
            var p = obj.GetType().GetProperty(nameofProp);
            float x = (float)(p?.GetValue(obj) ?? 0f);
            bool r = ImGui.InputFloat(label, ref x);
            p?.SetValue(obj, x);
            return r;
        }

        public static bool InputDouble(string label, object obj, string nameofProp) {
            var p = obj.GetType().GetProperty(nameofProp);
            double x = (double)(p?.GetValue(obj) ?? 0d);
            bool r = ImGui.InputDouble(label, ref x);
            p?.SetValue(obj, x);
            return r;
        }

        /// <summary>Extension to enable passing property as value with referencing behaviour.</summary>
        public static bool Checkbox(string label, object obj, string nameofProp) {
            var p = obj.GetType().GetProperty(nameofProp);
            bool x = (bool)(p?.GetValue(obj) ?? false);
            bool r = ImGui.Checkbox(label, ref x);
            p?.SetValue(obj, x);
            return r;
        }
    }
}
