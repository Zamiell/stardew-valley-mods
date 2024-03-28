using System;
using System.Security.AccessControl;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;

namespace VisibleArtifactSpots
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            var artifactSpots = GetArtifactSpots();

            foreach (var (vec, obj) in artifactSpots)
            {
                Notify("Artifact spot in \"" + Game1.currentLocation.Name + "\" at: " + vec);
            }
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            foreach (var objectDict in Game1.currentLocation.objects)
            {
                foreach (var (vec, obj) in objectDict)
                {
                    if (obj.ItemId == "590") // Artifact spot, from "DebugCommands.cs"
                    {
                        // e.SpriteBatch.DrawString(Game1.dialogueFont, "Hello", new Vector2(200, 200), Color.White);

                        var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(vec.X * 64, vec.Y * 64));
                        var rect = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29);

                        // This draw invocation is copied from the tool hit rectangle in "Farmer.cs".
                        e.SpriteBatch.Draw(Game1.mouseCursors, pos, rect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, pos.Y / 10000f);
                    }
                }
            }
        }

        private List<(Vector2 vec, StardewValley.Object obj)> GetArtifactSpots()
        {
            var artifactSpots = new List<(Vector2 vec, StardewValley.Object obj)>();

            foreach (var objectDict in Game1.currentLocation.objects)
            {
                foreach (var (vec, obj) in objectDict)
                {
                    if (obj.ItemId == "590") // Artifact spot, from "DebugCommands.cs"
                    {
                        artifactSpots.Add((vec, obj));
                    }
                }
            }

            return artifactSpots;
        }

        private void Notify(string msg)
        {
            Game1.chatBox.addMessage(msg, Color.Red);
            Game1.playSound("cowboy_powerup");
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
