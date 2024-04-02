using StardewModdingAPI.Utilities;

namespace PlantHotkey
{
    public sealed class ModConfig
    {
        public KeybindList Hotkey { get; set; } = KeybindList.Parse("LeftControl");
    }
}
