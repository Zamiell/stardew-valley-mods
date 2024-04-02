using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.TerrainFeatures;
using StardewValley;

namespace FadeTrees
{
    public class ModEntry : Mod
    {
        // Constants
        private const string TREE_TYPE_OAK = "1";

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            CheckTrees();
        }

        private void CheckTrees()
        {
            foreach (var terrainFeatureDict in Game1.currentLocation.terrainFeatures)
            {
                foreach (var (pos, terrainFeature) in terrainFeatureDict)
                {
                    if (terrainFeature is Tree tree && tree.tapped.Value && tree.treeType.Value == TREE_TYPE_OAK)
                    {
                        tree.alpha = 0.4f;
                    }
                }
            }
        }
    }
}
