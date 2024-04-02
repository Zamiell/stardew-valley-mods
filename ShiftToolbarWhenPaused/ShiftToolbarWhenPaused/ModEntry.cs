using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using static StardewValley.Minigames.MineCart.Whale;

namespace ShiftToolbarWhenPaused
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

            // We don't want the modded functionality to interfere with the vanilla functionality.
            if (Game1.activeClickableMenu is null)
            {
                return;
            }

            if (ButtonPressedMatchesConfiguredToolbarSwapButton(e.Button))
            {
                bool isLeftCtrlPressed = this.Helper.Input.IsDown(SButton.LeftControl);
                bool shiftRight = !isLeftCtrlPressed; // This matches the vanilla functionaltity; see usage of "Game1.player.shiftToolbar".
                Game1.player.shiftToolbar(shiftRight);
            }
        }

        private bool ButtonPressedMatchesConfiguredToolbarSwapButton(SButton button)
        {
            foreach (InputButton swapButton in Game1.options.toolbarSwap)
            {
                if (swapButton.ToString() == button.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
