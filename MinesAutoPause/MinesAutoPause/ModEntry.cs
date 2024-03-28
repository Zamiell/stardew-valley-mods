﻿using System;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace MinesAutoPause
{
    public class ModEntry : Mod
    {
        // From "Mineshaft.cs"
        enum TileType
        {
            Ladder = 173,
            Shaft = 174,
            MineCartCoal = 194,
        }

        private Vector2 lastLadderPos = Vector2.Zero;
        private Vector2 lastShaftPos = Vector2.Zero;
        private GameLocation lastLocation = Game1.currentLocation;
        private int lastNumStaircases = 0;
        private int lastNumBombs = 0;
        private int lastNumProjectiles = 0;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (Game1.player.currentLocation is not MineShaft mineShaft)
            {
                lastLadderPos = Vector2.Zero;
                lastShaftPos = Vector2.Zero;
                lastNumStaircases = 0;
                lastNumBombs = 0;
                lastNumProjectiles = 0;

                return;
            }

            if (IsDungeonBattleFloor(mineShaft))
            {
                CheckNewLadderOrShaft(mineShaft);
                CheckPlacedBombExploded(mineShaft);
                CheckExplodingAmmoExploded(mineShaft);
            }
        }

        private void CheckNewLadderOrShaft(MineShaft mineShaft)
        {
            var ladderPos = GetTilePosition(mineShaft, TileType.Ladder);
            Vector2 oldLadderPos = lastLadderPos;
            Vector2 newLadderPos = ladderPos;
            lastLadderPos = ladderPos;

            var shaftPos = GetTilePosition(mineShaft, TileType.Shaft);
            Vector2 oldShaftPos = lastShaftPos;
            Vector2 newShaftPos = shaftPos;
            lastShaftPos = shaftPos;

            int numStaircases = Game1.player.Items.CountId("(BC)71"); // Staircase ID
            int oldNumStaircases = lastNumStaircases;
            int newNumStaircases = numStaircases;
            lastNumStaircases = numStaircases;

            if (!Game1.currentLocation.Equals(lastLocation))
            {
                lastLocation = Game1.currentLocation;
                lastLadderPos = Vector2.Zero;
                lastShaftPos = Vector2.Zero;
                return;
            }

            // Do not send the alert if we are using a crafted staircase.
            if (oldNumStaircases != newNumStaircases)
            {
                return;
            }

            if (!oldLadderPos.Equals(newLadderPos))
            {
                int floorNum = GetDungeonFloorNum(mineShaft.Name);
                Notify("Ladder spawned on floor " + floorNum + "!");
                EmulatePause();
            }

            if (!oldShaftPos.Equals(newShaftPos))
            {
                int floorNum = GetDungeonFloorNum(mineShaft.Name);
                Notify("Shaft spawned on floor " + floorNum + "!");
                EmulatePause();
            }
        }

        private void CheckPlacedBombExploded(MineShaft mineShaft)
        {
            int numBombs = GetNumTemporaryBombs(mineShaft);
            int oldNumBombs = lastNumBombs;
            int newNumBombs = numBombs;
            lastNumBombs = numBombs;

            if (oldNumBombs > newNumBombs)
            {
                EmulatePause();
            }
        }

        private int GetNumTemporaryBombs(GameLocation location)
        {
            int numBombs = 0;

            foreach (var sprite in location.TemporarySprites)
            {
                // The parent tile indexes are harded to match the 3 types of bombs.
                // See: TemporaryAnimatedSprite.GetTemporaryAnimatedSprite
                if (sprite.initialParentTileIndex == 286 || sprite.initialParentTileIndex == 287 || sprite.initialParentTileIndex == 288)
                {
                    numBombs++;
                }
            }

            return numBombs;
        }

        // We just check for any ammo being removed (assuming that all ammo is exploding ammo).
        private void CheckExplodingAmmoExploded(GameLocation location)
        {
            int numProjectiles = location.projectiles.Count;
            int oldNumProjectiles = lastNumProjectiles;
            int newNumProjectiles= numProjectiles;
            lastNumProjectiles = numProjectiles;

            if (oldNumProjectiles > 0 && newNumProjectiles == 0)
            {
                EmulatePause();
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e) {
            EmulatePause(true);
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation is not MineShaft mineShaft)
            {
                return;
            }

            if (IsDungeonBattleFloor(mineShaft))
            {
                EmulatePause();
                NotifyIfFloorHasLadderOrShaft(mineShaft);
                NotifyIfFloorHasMineCartCoal(mineShaft);
            }
        }

        // - "Mine" is the top floor.
        // - "UndergroundMine1" is the first floor.
        // - The Skull Caverns starts at "UndergroundMine121" for the first floor, and so on.
        public bool IsDungeonBattleFloor(MineShaft mineShaft)
        {
            if (!mineShaft.Name.StartsWith("UndergroundMine"))
            {
                return false;
            }


            // We don't need to pause on the specific mines floors with chests on them.
            if (IsMineEmptyFloor(mineShaft))
            {
                return false;
            }

            return true;
        }

        public int GetDungeonFloorNum(string locationName)
        {
            var floorNum = GetDungeonFloorNumRaw(locationName);

            // In Skull Caverns, floor 121 is floor 1.
            if (floorNum > 120)
            {
                floorNum -= 120;
            }

            return floorNum;
        }

        public int GetDungeonFloorNumRaw(string locationName)
        {
            string pattern = @"\d+";
            Match match = Regex.Match(locationName, pattern);

            if (!match.Success)
            {
                return 0;
            }

            string numericPart = match.Value;

            if (!int.TryParse(numericPart, out int floorNum))
            {
                return 0;
            }

            return floorNum;
        }

        private bool IsMineEmptyFloor(MineShaft mineShaft)
        {
            int floorNum = GetDungeonFloorNum(mineShaft.Name);
            return floorNum % 10 == 0 && !IsSkullCaverns(mineShaft);
        }

        public bool IsSkullCaverns(MineShaft mineShaft)
        {
            int floorNumRaw = GetDungeonFloorNumRaw(mineShaft.Name);
            return floorNumRaw > 120;
        }

        private void EmulatePause(bool force = false)
        {
            // Do not pause if the day is over.
            // - Starts at 600, which is 6 AM.
            // - Only increments every 10 minutes, meaning that the second value is 610, which is 6:10 AM.
            // - 1300 is 1 PM.
            // - 2600 is the last possible value.
            if ((Game1.timeOfDay <= 600 || Game1.timeOfDay >= 2600) && !force)
            {
                return;
            }

            Game1.activeClickableMenu = new GameMenu();
        }

        private void NotifyIfFloorHasLadderOrShaft(MineShaft mineShaft)
        {
            int floorNum = GetDungeonFloorNum(mineShaft.Name);

            var ladderPos = GetTilePosition(mineShaft, TileType.Ladder);
            if (ladderPos != Vector2.Zero)
            {
                Notify("Floor " + floorNum + " has an pre-existing ladder!");
            }

            var shaftPos = GetTilePosition(mineShaft, TileType.Shaft);
            if (shaftPos != Vector2.Zero)
            {
                Notify("Floor " + floorNum + " has an pre-existing shaft!");
            }
        }

        private void NotifyIfFloorHasMineCartCoal(MineShaft mineShaft)
        {
            int floorNum = GetDungeonFloorNum(mineShaft.Name);

            var mineCartPos = GetTilePosition(mineShaft, TileType.MineCartCoal);
            if (mineCartPos != Vector2.Zero)
            {
                Notify("Floor " + floorNum + " has an mine cart with coal!");
            }
        }

        // Based on the "Joys of Efficiency" mod with some changes:
        // https://github.com/pomepome/JoysOfEfficiency/blob/master/JoysOfEfficiency/Huds/MineHud.cs
        private static Vector2 GetTilePosition(MineShaft mineShaft, TileType tileType)
        {
            for (int i = 0; i < mineShaft.Map.GetLayer("Buildings").LayerWidth; i++)
            {
                for (int j = 0; j < mineShaft.Map.GetLayer("Buildings").LayerHeight; j++)
                {
                    int index = mineShaft.getTileIndexAt(new Point(i, j), "Buildings");
                    Vector2 loc = new Vector2(i, j);
                    if (mineShaft.Objects.ContainsKey(loc) || mineShaft.terrainFeatures.ContainsKey(loc))
                    {
                        continue;
                    }

                    if (index == (int)tileType)
                    {
                        return loc;
                    }
                }
            }

            return Vector2.Zero;
        }

        private void Notify(string msg)
        {
            if (Game1.timeOfDay <= 600 || Game1.timeOfDay >= 2600)
            {
                return;
            }

            Game1.chatBox.addMessage(msg, Color.Red);
            Game1.playSound("cowboy_powerup");
        }

        private void Log(string msg)
        {
            this.Monitor.Log(msg, LogLevel.Debug);
        }
    }
}
