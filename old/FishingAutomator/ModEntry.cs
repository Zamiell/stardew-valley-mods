using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace FishingAutomator
{
    internal partial class ModEntry : Mod
    {
        private bool fishHookedOnLastFrame = false;
        private int fishHookedFrame = 0;

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

            CheckNibble();
        }

        private void CheckNibble()
        {
            if (!(Game1.player.CurrentTool is StardewValley.Tools.FishingRod))
            {
                return;
            }

            var fishingRod = Game1.player.CurrentTool as StardewValley.Tools.FishingRod;

            // Do nothing if fishing rod already had auto hook enchantment.
            if (fishingRod.hasEnchantmentOfType<AutoHookEnchantment>())
            {
                return;
            }

            var fishCurrentlyHooked = IsRodCanHook(fishingRod);
            if (fishCurrentlyHooked != fishHookedOnLastFrame)
            {
                fishHookedOnLastFrame = fishCurrentlyHooked;
                if (fishCurrentlyHooked)
                {
                    fishHookedFrame = Game1.milli
                    FishHooked();
                }
            }
        }

        private void FishHooked()
        {

        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
