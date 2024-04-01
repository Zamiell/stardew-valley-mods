using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley;

namespace MorningAutoPause
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            EmulatePause();
        }

        private void EmulatePause()
        {
            if (Game1.activeClickableMenu is null)
            {
                Game1.activeClickableMenu = new GameMenu();
            }
        }
    }
}
