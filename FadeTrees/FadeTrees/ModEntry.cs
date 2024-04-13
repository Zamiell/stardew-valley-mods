using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.TerrainFeatures;
using StardewValley;
using xTile.Dimensions;

namespace FadeTrees
{
    public class ModEntry : Mod
    {
        // Constants
        private const string TREE_TYPE_OAK = "1";
        private const float FADE_AMOUNT = 0.25f;

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
                    if (
                        terrainFeature is Tree tree
                        && tree.treeType.Value == TREE_TYPE_OAK
                        && !tree.hasMoss.Value
                        && (tree.tapped.Value || tree.wasShakenToday.Value)
                    )
                    {
                        tree.alpha = FADE_AMOUNT;
                    }
                }
            }
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
