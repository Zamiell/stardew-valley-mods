using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ArtifactNotifier
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            CheckForArtifactSpots();
        }

        private void CheckForArtifactSpots()
        {
            foreach (StardewValley.Object node in Game1.currentLocation.objects.Values)
            {
                if (node.Name == "Artifact Spot")
                {
                    string msg = $"Artifact spot detected in {Game1.currentLocation.Name} at: {node.TileLocation.X}, {node.TileLocation.Y}";
                    Game1.chatBox.addMessage(msg, Color.Red);
                }
            }
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
