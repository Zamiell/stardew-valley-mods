using System;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace WateringCanNotifier
{
    public class ModEntry : Mod
    {
        // 48 is the highest amount of damage.
        // https://stardewvalleywiki.com/Skull_Cavern#Monsters
        private const int DANGEROUS_HEALTH = 48;

        private int WateringCanWaterOnLastFrame = 70; // Max energy
        private Netcode.NetStringList BuffIDs = new Netcode.NetStringList();
        private int LastHealth = 100; // Default starting health on a new character,
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            CheckWateringCan();
            CheckBuffWornOff();
            CheckHP();
        }

        private void CheckWateringCan()
        {
            if (Game1.player.CurrentTool is StardewValley.Tools.WateringCan wateringCan && wateringCan is not null)
            {
                if (wateringCan.WaterLeft != WateringCanWaterOnLastFrame)
                {
                    WateringCanWaterChanged(wateringCan.WaterLeft);
                }
                WateringCanWaterOnLastFrame = wateringCan.WaterLeft;
            }
        }

        private void WateringCanWaterChanged(int waterLeft)
        {
            if (waterLeft <= 0) // Water can go into the negatives
            {
                Notify("Watering can out of water!");
            }
        }

        private void CheckBuffWornOff()
        {
            if (BuffIDs.SequenceEqual(Game1.player.buffs.AppliedBuffIds))
            {
                return;
            }

            Netcode.NetStringList oldBuffIDs = BuffIDs.DeepClone();
            Netcode.NetStringList newBuffIDs = Game1.player.buffs.AppliedBuffIds;
            BuffIDs = newBuffIDs.DeepClone();

            if (oldBuffIDs.Contains("food") && !newBuffIDs.Contains("food"))
            {
                Notify("Food buff worn off!", "Cowboy_Secret");
                EmulatePause();
            }

            if (oldBuffIDs.Contains("drink") && !newBuffIDs.Contains("drink"))
            {
                Notify("Drink buff worn off!");
                EmulatePause();
            }
        }

        private void EmulatePause()
        {
            // Do not pause if the day is over.
            // - Starts at 600, which is 6 AM.
            // - Only increments every 10 minutes, meaning that the second value is 610, which is 6:10 AM.
            // - 1300 is 1 PM.
            // - 2600 is the last possible value.
            if (Game1.timeOfDay <= 600 || Game1.timeOfDay >= 2600)
            {
                return;
            }

            Game1.activeClickableMenu = new GameMenu();
        }

        private void CheckHP()
        {
            int health = Game1.player.health;
            int oldHealth= LastHealth;
            int newHealth = health;
            LastHealth = health;

            if (oldHealth != newHealth && newHealth <= DANGEROUS_HEALTH)
            {
                Notify("Health is at dangerous levels! (" + oldHealth + " < " + DANGEROUS_HEALTH + ")");
                EmulatePause();
            }
        }

        private void Notify(string msg, string customSound = "cowboy_gunload")
        {
            if (Game1.timeOfDay <= 600 || Game1.timeOfDay >= 2600)
            {
                return;
            }

            Game1.chatBox.addMessage(msg, Color.Red);
            Game1.playSound(customSound);
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
