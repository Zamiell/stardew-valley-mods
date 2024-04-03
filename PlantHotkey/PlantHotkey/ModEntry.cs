using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
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

            foreach (Vector2 tile in tiles)
            {
                // Auto plant seeds + auto plant fertilizer + auto harvest
                if (location.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt)
                {
                    if (IsSeed(slot1Item) && dirt.crop is null)
                    {
                        bool success = dirt.plant(slot1Item.ItemId, Game1.player, false);
                        if (success)
                        {
                            Game1.player.Items.ReduceId(slot1Item.ItemId, 1);
                        }
                    }

                    if (IsFertilizer(slot2Item) && dirt.fertilizer.Value is null)
                    {
                        bool success = dirt.plant(slot2Item.ItemId, Game1.player, true);
                        if (success)
                        {
                            Game1.player.Items.ReduceId(slot2Item.ItemId, 1);
                        }
                    }

                    if (dirt.readyForHarvest() && dirt.crop is not null && dirt.crop.GetHarvestMethod() == HarvestMethod.Grab)
                    {
                        dirt.performUseAction(dirt.crop.tilePosition);
                    }
                }

                StardewValley.Object obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);

                // Auto empty nearby objects
                if (obj is not null && obj.readyForHarvest.Value)
                {
                    obj.checkForAction(Game1.player);
                }

                // Auto fill Kegs
                if (obj is not null && obj.Name == "Keg" && IsFruit(slot1Item))
                {
                    bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player);
                    if (success)
                    {
                        Game1.player.Items.ReduceId(slot1Item.ItemId, 1);
                    }
                }

                // Auto fill Furnaces
                if (obj is not null && obj.Name == "Furnace" && IsOre(slot1Item))
                {
                    bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player);
                    if (success)
                    {
                        Game1.player.Items.ReduceId(slot1Item.ItemId, 1);
                    }
                }

                // Auto-fill Crab Pots
                if (obj is not null && obj.Name == "Crab Pot" && IsBait(slot1Item))
                {
                    bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player);
                    if (success)
                    {
                        Game1.player.Items.ReduceId(slot1Item.ItemId, 1);
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

        private bool IsFruit(Item? item)
        {
            return item is not null && item.Category == StardewValley.Object.FruitsCategory;
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
