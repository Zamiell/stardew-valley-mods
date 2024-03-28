﻿using Microsoft.Xna.Framework;
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
                if (obj.Name == "Artifact Spot")
                {
                    var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(obj.TileLocation.X * 64, obj.TileLocation.Y * 64));
                    var rect = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29);

                    // This draw invocation is copied from the tool hit rectangle in "Farmer.cs".
                    e.SpriteBatch.Draw(Game1.mouseCursors, pos, rect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, pos.Y / 10000f);
                }
            }
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
