using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Text.RegularExpressions;
using xTile.Dimensions;

namespace PlantHotkey
{
    public class ModEntry : Mod
    {
        // Enums
        /// From "Mineshaft.cs"
        enum TileType
        {
            LadderUp = 115,
            LadderDown = 173,
            Shaft = 174,
            CoalSackOrMineCart = 194,
        }

        // Variables
        private ModConfig config = new();
        bool usedLadderOnThisFloor = false;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Player.Warped += this.OnWarped;
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

            if (Game1.activeClickableMenu is not null)
            {
                return;
            }

            if (config.Hotkey.IsDown())
            {
                SearchSurroundingTiles();
            }
        }

        private void SearchSurroundingTiles()
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
                // Terrain features
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
                                // We explicitly do not return since then it will fail to shake trees that are beside each other.
                                // (This is because a success can happen on every frame.)
                            }
                        }
                    }

                    // Auto-shake fruit trees
                    if (terrainFeature is FruitTree fruitTree)
                    {
                        bool success = fruitTree.performUseAction(fruitTree.Tile);
                        if (success)
                        {
                            // We explicitly do not return since then it will fail to shake trees that are beside each other.
                            // (This is because a success can happen on every frame.)
                        }
                    }
                }

                // Objects
                StardewValley.Object obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);
                if (obj is not null)
                {
                    // Auto empty nearby objects
                    if (obj.readyForHarvest.Value)
                    {
                        bool success = obj.checkForAction(Game1.player);
                        if (success)
                        {
                            return;
                        }
                    }

                    // Auto fill Kegs
                    if (obj.Name == "Keg" && IsFruitOrVegetable(slot1Item))
                    {
                        bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player); // This automatically decrements the item stack.
                        if (success)
                        {
                            return;
                        }
                    }

                    // Auto fill Furnaces
                    if ((obj.Name == "Furnace" || obj.Name == "Heavy Furnace") && IsOre(slot1Item))
                    {
                        bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player); // This automatically decrements the item stack.
                        if (success)
                        {
                            return;
                        }
                    }

                    // Auto-fill Crab Pots
                    if (obj.Name == "Crab Pot" && IsBait(slot1Item))
                    {
                        bool success = obj.performObjectDropInAction(slot1Item, false, Game1.player); // This automatically decrements the item stack.
                        if (success)
                        {
                            return;
                        }
                    }

                    // Auto-shake Tea Bushes
                    if (obj is IndoorPot indoorPot && indoorPot.bush.Value is not null)
                    {
                        bool success = indoorPot.bush.Value.performUseAction(indoorPot.TileLocation);
                        if (success)
                        {
                            // We explicitly do not return since then it will fail to shake trees that are beside each other.
                            // (This is because a success can happen on every frame.)
                        }
                    }
                }

                // Specific location things
                switch (location.Name)
                {
                    // Top floor of the mines.
                    case "Mine":
                        // The closest tile that the elevator can be clicked on is (17, 4).
                        if (tile.X == 17 && tile.Y == 4)
                        {
                            if (!usedLadderOnThisFloor)
                            {
                                usedLadderOnThisFloor = true;
                                Location tileLocation = new Location((int)tile.X, (int)tile.Y);
                                location.performAction("MineElevator", Game1.player, tileLocation);
                                return;
                            }
                        }
                        break;

                    // The tiny entrance to Skull Cavern.
                    case "SkullCave":
                        // The closest tile that the door can be clicked on is (3, 4).
                        if (tile.X == 3 && tile.Y == 4)
                        {
                            if (!usedLadderOnThisFloor)
                            {
                                usedLadderOnThisFloor = true;
                                Location tileLocation = new Location((int)tile.X, (int)tile.Y);
                                location.performAction("SkullDoor", Game1.player, tileLocation);
                                return;
                            }
                        }
                        break;
                }

                // Mine shaft things
                if (location is MineShaft mineShaft)
                {
                    int index = location.getTileIndexAt(new Point((int)tile.X, (int)tile.Y), "Buildings");
                    if (!location.Objects.ContainsKey(tile) && !location.terrainFeatures.ContainsKey(tile))
                    {
                        Location tileLocation = new Location((int)tile.X, (int)tile.Y);

                        switch (index)
                        {
                            case (int)TileType.LadderUp:
                                // We only want to automatically go up ladders when farming ore in the mines (and not in Skull Cavern).
                                if (!IsSkullCavern(location.Name))
                                {
                                    if (!usedLadderOnThisFloor)
                                    {
                                        usedLadderOnThisFloor = true;
                                        location.answerDialogueAction("ExitMine_Leave", Array.Empty<string>()); // We want to skip the annoying dialog.
                                        return;
                                    }
                                }
                                break;
                            
                            case (int)TileType.LadderDown:
                                if (!usedLadderOnThisFloor)
                                {
                                    usedLadderOnThisFloor = true;
                                    location.checkAction(tileLocation, Game1.viewport, Game1.player);
                                    return;
                                }
                                break;

                            case (int)TileType.Shaft:
                                if (!usedLadderOnThisFloor)
                                {
                                    usedLadderOnThisFloor = true;
                                    mineShaft.enterMineShaft(); // We want to skip the annoying dialog.
                                    return;
                                }
                                break;

                            case (int)TileType.CoalSackOrMineCart:
                                location.checkAction(tileLocation, Game1.viewport, Game1.player);
                                return;
                        }
                    }
                }
            }
        }

        private static bool IsSeed(Item? item)
        {
            return item is not null && item.Category == StardewValley.Object.SeedsCategory;
        }

        private static bool IsFertilizer(Item? item)
        {
            return item is not null && item.Category == StardewValley.Object.fertilizerCategory;
        }

        private static bool IsFruitOrVegetable(Item? item)
        {
            return item is not null && (item.Category == StardewValley.Object.FruitsCategory || item.Category == StardewValley.Object.VegetableCategory);
        }

        private static bool IsOre(Item? item)
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
                || item.Name == "Radioactive Ore"
            );
        }

        private static bool IsBait(Item? item)
        {
            // We intentionally do not use the category of bait to prevent accidentally using non-standard bait.
            return item is not null && item.Name == "Bait";
        }

        public static bool IsSkullCavern(string locationName)
        {
            string pattern = @"\d+";
            Match match = Regex.Match(locationName, pattern);

            if (!match.Success)
            {
                return false;
            }

            string numericPart = match.Value;

            if (!int.TryParse(numericPart, out int floorNum))
            {
                return false;
            }

            // In Skull Caverns, floor 121 is floor 1.
            return floorNum > 120;
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            usedLadderOnThisFloor = false;
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
