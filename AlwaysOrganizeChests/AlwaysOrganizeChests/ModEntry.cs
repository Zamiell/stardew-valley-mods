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

namespace AlwaysOrganizeChests
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.World.ChestInventoryChanged += this.OnChestInventoryChanged;
        }
        private void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
        {
            ItemGrabMenu.organizeItemsInList(e.Chest.Items);
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
