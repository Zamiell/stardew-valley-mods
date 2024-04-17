using StardewModdingAPI.Utilities;

namespace VisibleArtifactSpots
{
    public sealed class ModConfig
    {
        public string HighlightType { get; set; } = "Border";
        public bool HighlightArtifactSpots { get; set; } = true;
        public bool HighlightSeedSpots { get; set; } = true;
        public bool HighlightCopperNodes { get; set; } = false;
        public bool HighlightIronNodes { get; set; } = false;
        public bool HighlightGoldNodes { get; set; } = false;
        public bool HighlightIridiumNodes { get; set; } = false;
        public bool HighlightRadioactiveNodes { get; set; } = false;
        public bool HighlightCinderNodes { get; set; } = false;
        public bool HighlightChests { get; set; } = false;
        public bool HighlightNonPlanted { get; set; } = false;
        public bool HighlightNonWatered { get; set; } = false;
        public bool HighlightNonFertilized { get; set; } = false;
    }
}
