using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

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
