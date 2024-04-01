using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

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
                () => "Hotkey",
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
                if (location.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt)
                {
                    if (IsSeed(slot1Item) && dirt.crop is null)
                    {
                        dirt.plant(slot1Item.ItemId, Game1.player, false);
                    }

                    if (IsFertilizer(slot2Item) && dirt.fertilizer is null)
                    {
                        dirt.plant(slot2Item.ItemId, Game1.player, true);
                    }

                    if (dirt.crop is not null)
                    {
                        // TODO: auto harvest
                        // dirt.performUseAction(dirt.Tile);
                    }
                }
            }
        }

        private bool IsSeed(Item? item)
        {
            if (item is null)
            {
                return false;
            }

            return (
                item.Name == "Starfruit Seeds"
                || item.Name == "Pumpkin Seeds"
                || item.Name == "Ancient Seeds"
                || item.Name == "Mixed Seeds"
                || item.Name == "Fiber Seeds"
            );
        }

        private bool IsFertilizer(Item? item)
        {
            if (item is null)
            {
                return false;
            }

            return (
                item.Name == "Basic Fertilizer"
                || item.Name == "Quality Fertilizer"
                || item.Name == "Deluxe Fertilizer"
                || item.Name == "Speed-Gro"
                || item.Name == "Deluxe Speed-Gro"
                || item.Name == "Hyper Speed-Gro"
            );
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
