using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace BubblesNotifier
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public int bubblesX = 0;
        public int bubblesY = 0;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            int oldX = this.bubblesX;
            int oldY = this.bubblesY;

            int X = Game1.currentLocation.fishSplashPoint.X;
            int Y = Game1.currentLocation.fishSplashPoint.Y;

            this.bubblesX = X;
            this.bubblesY = Y;

            if (X == oldX && Y == oldY)
            {
                return;
            }

            string msg = X == 0 && Y == 0 ? "Bubbles disappeared." : $"Bubbles appeared: {X}, {Y}";
            Game1.chatBox.addMessage(msg, Color.Red);
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
