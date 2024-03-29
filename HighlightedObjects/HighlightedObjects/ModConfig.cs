namespace HighlightedObjects
{
    public sealed class ModConfig
    {
        public string HighlightType { get; set; } = "Highlight";

        public bool HighlightJars { get; set; } = true;
        public bool HighlightKegs { get; set; } = true;
        public bool HighlightCasks { get; set; } = true;
        public bool HighlightCrystalariums { get; set; } = true;
    }
}
