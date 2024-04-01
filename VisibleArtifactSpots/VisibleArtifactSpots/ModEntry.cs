using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace VisibleArtifactSpots
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            foreach (var obj in Game1.currentLocation.objects.Values)
            {
                if (obj.Name == "Artifact Spot" || obj.Name == "Seed Spot")
                {
                    DrawRedRectAroundObject(obj, e.SpriteBatch);
                    DrawNotificationBubbleAboveObject(obj, e.SpriteBatch);
                }
            }
        }

        private void DrawRedRectAroundObject(StardewValley.Object obj, SpriteBatch spriteBatch)
        {
            var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(obj.TileLocation.X * 64, obj.TileLocation.Y * 64));
            var rect = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29);
            var fadedWhite = new Color(255, 255, 255, 127);

            // This draw invocation is copied from the tool hit rectangle in "Farmer.cs".
            spriteBatch.Draw(Game1.mouseCursors, pos, rect, fadedWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, pos.Y / 10000f);
        }

        private void DrawNotificationBubbleAboveObject(StardewValley.Object obj, SpriteBatch spriteBatch)
        {
            var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(obj.TileLocation.X * 64, obj.TileLocation.Y * 64));
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
