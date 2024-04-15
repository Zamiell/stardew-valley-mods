using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Runtime.CompilerServices;

namespace VisibleArtifactSpots
{
    public class ModEntry : Mod
    {
        // Variables
        private ModConfig config = new();

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
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

            configMenu.AddTextOption(
                this.ModManifest,
                () => config.HighlightType,
                (string val) => config.HighlightType = val,
                () => "Highlight type",
                () => "The way to highlight spots.",
                new string[] { "Border", "Bubble" }
            );
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            CheckLocationObjects(e.SpriteBatch);
        }

        private void CheckLocationObjects(SpriteBatch spriteBatch)
        {
            foreach (StardewValley.Object obj in Game1.currentLocation.objects.Values)
            {
                if (obj.Name == "Artifact Spot" || obj.Name == "Seed Spot")
                {
                    HighlightObject(obj, spriteBatch);
                }
            }
        }

        private void HighlightObject(StardewValley.Object obj, SpriteBatch spriteBatch)
        {
            switch (config.HighlightType)
            {
                case "Border":
                    DrawRedBorderAroundObject(obj, spriteBatch);
                    break;

                case "Bubble":
                    DrawNotificationBubbleAboveObject(obj, spriteBatch);
                    break;

                default:
                    break;
            }
        }

        private void DrawRedBorderAroundObject(StardewValley.Object obj, SpriteBatch spriteBatch)
        {
            var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(obj.TileLocation.X * 64, obj.TileLocation.Y * 64));
            var rect = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29);
            var fadedWhite = new Color(255, 255, 255, 127);

            // This draw invocation is copied from the tool hit rectangle in "Farmer.cs".
            spriteBatch.Draw(Game1.mouseCursors, pos, rect, fadedWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, pos.Y / 10000f);
        }

        private void DrawNotificationBubbleAboveObject(StardewValley.Object obj, SpriteBatch spriteBatch)
        {
            Vector2 objPos = Game1.GlobalToLocal(Game1.viewport, new Vector2(obj.TileLocation.X * 64, obj.TileLocation.Y * 64));
            Vector2 pos = objPos - new Vector2(0, 32); // 1 tile above where the object is
            Rectangle destinationRectangle = new Rectangle((int)pos.X, (int)pos.Y - 32, 64, 64);

            spriteBatch.Draw(
                Game1.emoteSpriteSheet,
                destinationRectangle,
                new Rectangle(16 * 16 % Game1.emoteSpriteSheet.Width, 16 * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16),
                Color.White * 0.95f, 0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                (float)((obj.TileLocation.Y - 1) * 64) / 10000f
            );
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
