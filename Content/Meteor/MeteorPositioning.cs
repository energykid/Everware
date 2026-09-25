using System.Threading;
using Everware.Common.Systems;
using Everware.Core;
using Everware.Utils;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Meteor;

public class MeteorPositioning : ModSystem
{
    public static Point MeteorPosition = Point.Zero;

    public static int MeteorAnimationTimer = 0;
    public static bool MeteorSpawned = false;
    
    public override void PreWorldGen()
    {
        MeteorPosition = Point.Zero;
        MeteorSpawned = false;
    }
    public override void OnWorldLoad()
    {
        MeteorAnimationTimer = 0;
        CheckTime = -(60 * 20);
        MeteorPosition = Point.Zero;
        MeteorSpawned = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag.Set("MeteorPosition", MeteorPosition.ToVector2());
        tag.Set("MeteorSpawned", MeteorSpawned);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        MeteorPosition = tag.Get<Vector2>("MeteorPosition").ToPoint();
        MeteorSpawned = tag.Get<bool>("MeteorSpawned");
    }

    private static int Threshold = 400;
    private static int MaxThreshold = 400;

    private static int CheckTime;
    
    [ModSystemHooks.PostUpdateEverything]
    public static void UpdateMeteorPosition()
    {
        if (MeteorPosition == Point.Zero)
        {
            if (NPC.downedBoss2)
            {
                CheckTime++;

                if (CheckTime >= 0 && CheckTime % 60 == 0)
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
                CheckTime = -(60 * 20);
            }
        }
        else
        {
            if (!MeteorSpawned)
            {
                MeteorAnimationTimer++;
                // Meteor fall sound
                if (MeteorAnimationTimer == 1)
                {
                    SoundEngine.PlaySound(Assets.Sounds.Misc.MeteorFall.Asset);
                }
                // Meteor landing sound
                if (MeteorAnimationTimer == 70)
                {
                    SoundEngine.PlaySound(Assets.Sounds.Misc.MeteorCrash.Asset);
                }
                // Meteor landing text
                if (MeteorAnimationTimer == 78)
                {
                    Main.NewText(Mods.Everware.MeteorLanding.GetTextValue());
                    MeteorGeneration.GenerateWholeSite(MeteorPosition);
                }

                // Screen shake and effects
                if (MeteorAnimationTimer > 78 && MeteorAnimationTimer < 400)
                {
                    float intensity = MathHelper.Lerp(1f, 0f, (MeteorAnimationTimer - 60f) / 340f);
                    
                    ScreenEffects.DimScreen(intensity * 0.1f);
                    ScreenEffects.ZoomScreen(-intensity * 0.05f);
                    ScreenEffects.AddScreenShake(Main.LocalPlayer.Center, intensity * 10f, 0.8f);
                }
            }
        }
    }
    private static readonly Thread MeteorFindThread = new(() =>
    {
        int tries = 100;
        while (Threshold >= 200)
        {
            Point p = new Point(Main.spawnTileX, (int)Main.worldSurface - 200);
            p.X += WorldGen.genRand.Next((int)(Main.maxTilesX * 0.2f), (int)(Main.maxTilesX * 0.4f)) *
                   (WorldGen.genRand.NextBool() ? -1 : 1);
            p = p.Grounded(600);

            if (!IsPositionBlacklisted(p))
            {
                MeteorPosition = p;
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
    });
    public static void FindPosition()
    {
        MeteorFindThread.Start();
    }

    private static bool IsPositionBlacklisted(Point pt)
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