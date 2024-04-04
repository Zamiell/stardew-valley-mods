using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Crops;
using StardewValley.TerrainFeatures;

namespace PlantHotkey
{
    public class ModEntry : Mod
    {
        private ModConfig config = new();

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => config = new ModConfig(),
                save: () => this.Helper.WriteConfig(config)
            );

            configMenu.AddKeybindList(
                this.ModManifest,
                () => config.Hotkey,
                (KeybindList val) => config.Hotkey = val,
                () => "Plant Hotkey",
                () => "The hotkey to plant seeds + fertilizer."
            );
        }

        // We use the "ButtonsChanged" event instead of the "ButtonPressed" event because we want it to continually work while the button is being held down.
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (config.Hotkey.IsDown())
            {
                PlantSeedsAndFertilizer();
            }
        }

        private void PlantSeedsAndFertilizer()
        {
            Item? slot1Item = Game1.player.Items[0];
            Item? slot2Item = Game1.player.Items[1];

            // From: DebugCommand::ArtifactSpots
            GameLocation location = Game1.player.currentLocation;
            Vector2 playerTile = Game1.player.Tile;
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(playerTile);
            Vector2[] tiles = surroundingTileLocationsArray.Concat(new[] { playerTile }).ToArray();

            // We return from each successful action to avoid race condition where we can plant more seeds than we have in our inventory.
            foreach (Vector2 tile in tiles)
            {
                if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature))
                {
                    // Auto plant seeds + auto plant fertilizer + auto harvest
                    if (terrainFeature is HoeDirt hoeDirt)
                    {
                        if (IsSeed(slot1Item) && hoeDirt.crop is null)
                        {
                            bool success = hoeDirt.plant(slot1Item.ItemId, Game1.player, false);
                            if (success)
                            {
                                Game1.player.Items.ReduceId(slot1Item.ItemId, 1);
                                return;
                            }
                        }

                        if (IsFertilizer(slot2Item) && hoeDirt.fertilizer.Value is null)
                        {
                            bool success = hoeDirt.plant(slot2Item.ItemId, Game1.player, true);
                            if (success)
                            {
                                Game1.player.Items.ReduceId(slot2Item.ItemId, 1);
                                return;
                            }
                        }

                        if (hoeDirt.readyForHarvest() && hoeDirt.crop is not null && hoeDirt.crop.GetHarvestMethod() == HarvestMethod.Grab)
                        {
                            bool success = hoeDirt.performUseAction(hoeDirt.crop.tilePosition);
                            if (success)
                            {
                                return;
                            }
                        }
                    }

                    // Auto-shake normal trees (e.g. Oak Trees)
                    if (terrainFeature is Tree tree)
                    {
                        // Don't shake trees that are tapped, because that is impossible in vanilla without removing the tapper.
                        if (!tree.tapped.Value)
                        {
                            bool success = tree.performUseAction(tree.Tile);
                            if (success)
                            {
                                return;
                            }
                        }
                    }

                    // Auto-shake fruit trees
                    if (terrainFeature is FruitTree fruitTree)
                    {
                        bool success = fruitTree.performUseAction(fruitTree.Tile);
                        if (success)
                        {
                            return;
                        }
                    }
                }

                StardewValley.Object obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);

                // Auto empty nearby objects
                if (obj is not null && obj.readyForHarvest.Value)
                {
                    bool success = obj.checkForAction(Game1.player);
                    if (success)
                    {
                        return;
                    }
                }

                // Auto fill Kegs
                if (obj is not null && obj.Name == "Keg" && IsFruitOrVegetable(slot1Item))
                {
                    bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player); // This automatically decrements the item stack.
                    if (success)
                    {
                        return;
                    }
                }

                // Auto fill Furnaces
                if (obj is not null && (obj.Name == "Furnace" || obj.Name == "Heavy Furnace") && IsOre(slot1Item))
                {
                    bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player); // This automatically decrements the item stack.
                    if (success)
                    {
                        return;
                    }
                }

                // Auto-fill Crab Pots
                if (obj is not null && obj.Name == "Crab Pot" && IsBait(slot1Item))
                {
                    bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player); // This automatically decrements the item stack.
                    if (success)
                    {
                        return;
                    }
                }
            }
        }

        private bool IsSeed(Item? item)
        {
            return item is not null && item.Category == StardewValley.Object.SeedsCategory;
        }

        private bool IsFertilizer(Item? item)
        {
            return item is not null && item.Category == StardewValley.Object.fertilizerCategory;
        }

        private bool IsFruitOrVegetable(Item? item)
        {
            return item is not null && (item.Category == StardewValley.Object.FruitsCategory || item.Category == StardewValley.Object.VegetableCategory);
        }

        private bool IsOre(Item? item)
        {
            if (item == null)
            {
                return false;
            }

            return (
                item.Name == "Copper Ore"
                || item.Name == "Iron Ore"
                || item.Name == "Gold Ore"
                || item.Name == "Iridium Ore"
            );
        }

        private bool IsBait(Item? item)
        {
            // We intentionally do not use the category of bait to prevent accidentally using non-standard bait.
            return item is not null && item.Name == "Bait";
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
