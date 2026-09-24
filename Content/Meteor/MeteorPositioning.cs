using System.Threading;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor;

public static class MeteorPositioning
{
    public static Point? MeteorPosition;

    private static int Threshold = 400;
    private static int MaxThreshold = 400;

    private static int CheckTime = 0;
    
    [ModSystemHooks.PostUpdateEverything]
    public static void UpdateMeteorPosition()
    {
        if (NPC.downedBoss2 && MeteorPosition == null)
        {
            CheckTime--;

            if (CheckTime <= 0)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (Main.LocalPlayer.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY) * 16) < 3000
                        || Main.LocalPlayer.position.Y > Main.worldSurface)
                        FindPosition();
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    bool shouldCheck = true;
                    foreach (Player player in Main.player)
                    {
                        if (player.active
                            && player.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY) * 16) > 5000
                            && player.position.Y < Main.worldSurface)
                            shouldCheck = false;
                    }

                    if (shouldCheck)
                        FindPosition();
                }
            }
        }
        else
        {
            CheckTime = 60 * 60;
        }
    }

    public static void FindPosition()
    {
            Thread thread = new Thread(() =>
            {
                int tries = 100;
                while (Threshold >= 200)
                {
                    Point p = new Point(Main.spawnTileX, (int)Main.worldSurface - 200);
                    p.X += WorldGen.genRand.Next((int)(Main.maxTilesX * 0.2f), (int)(Main.maxTilesX * 0.4f)) * (WorldGen.genRand.NextBool() ? -1 : 1);
                    p = p.Grounded(600);
                    
                    Main.NewText("Meteor position not found!");
                    if (!IsPositionBlacklisted(p))
                    {
                        MeteorPosition = p;
                        // remove the next 3 lines when i'm finished testing
                        MeteorGeneration.GenerateWholeSite(MeteorPosition.Value);
                        Main.LocalPlayer.Teleport(MeteorPosition.Value.ToVector2() * 16 + new Vector2(0, -500));
                        Main.NewText("Meteor position found!");
                        break;
                    }

                    tries--;

                    if (tries == 0)
                    {
                        MaxThreshold = 300;
                        break;
                    }
                }

                if (Threshold < 200)
                {
                    MaxThreshold = 300;
                }
                    
                CheckTime = 60 * (MaxThreshold < 400 ? 60 : 30);
            })
            {
                IsBackground = true,
            };
            thread.Start();
    }

    public static bool IsPositionBlacklisted(Point pt)
    {
        // if the tile at this exact spot is blacklisted, explode the rest of this logic right now
        if (Main.tile[pt].HasTile && MeteorGeneration.BlacklistedBlocks.Contains(Main.tile[pt].TileType)) return true;

        // check every container. if its origin is less than 3 tiles away from the spot, it's blacklisted
        for (int i = 0; i < Main.chest.Length; i++)
        {
            if (Main.chest[i] != null)
            {
                if (
                    Math.Abs(Main.chest[i].x - pt.X) < 3 &&
                    Math.Abs(Main.chest[i].y - pt.Y) < 3)
                    return true;
            }
        }

        // check every currently-housed town NPC's position. if nearby, that spot's blacklisted
        for (int i = 0; i < Main.instance._npcsWithBannersToDraw.Count; i++)
        {
            NPC npc = Main.npc[Main.instance._npcsWithBannersToDraw[i]];
            if (!npc.homeless && npc.Distance(pt.ToVector2() * 16f) < 200) return true;
        }
        
        // check the slope of nearby terrain
        int slope = new PathfindingUtils.FlatnessCheck(pt, new Point(40, 15)).ApproximateTerrainFlatness();
        if (slope > Threshold)
        {
            Threshold -= 10;
            return true;
        }
        
        return false;
    }
}