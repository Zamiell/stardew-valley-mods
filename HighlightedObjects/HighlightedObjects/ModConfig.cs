namespace HighlightedObjects
{
    public sealed class ModConfig
    {
        public string HighlightType { get; set; } = "Highlight";

        public bool HighlightCasks { get; set; } = true;
        public bool HighlightCrystalariums { get; set; } = true;
        public bool HighlightKegs { get; set; } = true;
        public bool HighlightPreservesJars { get; set; } = true;
        public bool HighlightSeedMakers { get; set; } = true;
    }
}
