using System;
using System.Security.AccessControl;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace SwapRings
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (e.Button.ToString() == "Z") {
                SwapRings();
            }
        }

        private void SwapRings()
        {
            var ringIndexes = GetInventoryRingIndexes();

            if (ringIndexes.Count < 1)
            {
                return;
            }

            var firstRingIndex = ringIndexes[0];
            SwapRing(true, firstRingIndex);
            Game1.playSound("toolSwap");

            if (ringIndexes.Count < 2)
            {
                return;
            }

            var secondRingIndex = ringIndexes[1];
            SwapRing(false, secondRingIndex);
            // We already played a sound.
        }

        // This does not include equipped rings.
        private List<int> GetInventoryRingIndexes()
        {
            List<int> ringIndexes = new List<int>();

            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                var item = Game1.player.Items[i];

                if (item is Ring ring)
                {
                    ringIndexes.Add(i);
                }
            }

            return ringIndexes;
        }

        private void SwapRing(bool left, int newRingIndex)
        {
            var newItem = Game1.player.Items[newRingIndex];
            if (newItem is Ring newRing)
            {
                if (left)
                {
                    var oldRing = Game1.player.leftRing.Value;
                    Game1.player.Equip(newRing, Game1.player.leftRing);
                    Game1.player.Items[newRingIndex] = oldRing;
                }
                else
                {
                    var oldRing = Game1.player.rightRing.Value;
                    Game1.player.Equip(newRing, Game1.player.rightRing);
                    Game1.player.Items[newRingIndex] = oldRing;
                }
            }
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
