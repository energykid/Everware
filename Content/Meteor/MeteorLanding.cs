using Everware.Common.Systems;
using Everware.Utils;
using System.Threading;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Everware.Common;
using Everware.Common.Colors;
using MonoMod.Cil;

namespace Everware.Content.Meteor;

public static class MeteorLanding
{
    [OnLoad]
    private static void Load()
    {
        On_Main.DrawSunAndMoon += DrawSunAndMoon_DrawFlare;
        On_Main.DrawSurfaceBG_BackMountainsStep1 += DrawSurfaceBG_BackMountainsStep1_BrightenBackground;
        On_Main.DrawSurfaceBG_BackMountainsStep2 += DrawSurfaceBG_BackMountainsStep2_BrightenBackground;
        IL_Main.DrawSurfaceBG += DrawSurfaceBG_BrightenBackground;
    }

    private static int animationTimer;

    private static readonly Color sky_flash_blue = new Color(235, 153, 255);
    private static readonly Color sky_flash_yellow = new Color(255, 138, 112);

    private static void DrawSurfaceBG_BackMountainsStep1_BrightenBackground(On_Main.orig_DrawSurfaceBG_BackMountainsStep1 orig, Main self, double backgroundTopMagicNumber, float bgGlobalScaleMultiplier, int pushBGTopHack)
    {
        const float brightness = 0.35f;

        var alpha = MathF.Sin(Terraria.Utils.Remap(animationTimer, 90, 290, 0f, MathF.PI));

        var interpolated = Color.HslLerp(sky_flash_blue, sky_flash_yellow, alpha * brightness);

        var lightColor = interpolated * alpha * brightness;

        lightColor = Color.Max(Main.ColorOfSurfaceBackgroundsBase, lightColor);

        using var _ = Main.ColorOfSurfaceBackgroundsBase.Override(lightColor);

        orig(self, backgroundTopMagicNumber, bgGlobalScaleMultiplier, pushBGTopHack);
    }

    private static void DrawSurfaceBG_BackMountainsStep2_BrightenBackground(On_Main.orig_DrawSurfaceBG_BackMountainsStep2 orig, Main self, int pushBGTopHack)
    {
        const float brightness = 0.6f;

        var alpha = MathF.Sin(Terraria.Utils.Remap(animationTimer, 88, 300, 0f, MathF.PI));

        var interpolated = Color.HslLerp(sky_flash_blue, sky_flash_yellow, alpha * brightness);

        var lightColor = interpolated * alpha * brightness;

        lightColor = Color.Max(Main.ColorOfSurfaceBackgroundsBase, lightColor);

        using var _ = Main.ColorOfSurfaceBackgroundsBase.Override(lightColor);

        orig(self, pushBGTopHack);
    }

    private static void DrawSurfaceBG_BrightenBackground(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            i => i.MatchCall<Main>(nameof(Main.DrawSurfaceBG_Forest))
        );

        c.GotoPrev(
            MoveType.After,
            i => i.MatchStsfld<Main>(nameof(Main.ColorOfSurfaceBackgroundsModified))
        );

        c.EmitDelegate(
            static () =>
            {
                const float brightness = 0.95f;

                var alpha = MathF.Sin(Terraria.Utils.Remap(animationTimer, 85, 320, 0f, MathF.PI));

                var interpolated = Color.HslLerp(sky_flash_blue, sky_flash_yellow, alpha * brightness * 0.7f);

                var lightColor = interpolated * alpha * brightness;

                lightColor = Color.Max(Main.ColorOfSurfaceBackgroundsModified, lightColor);
                Main.ColorOfSurfaceBackgroundsModified = lightColor;
            }
        );
    }

    private static void DrawSunAndMoon_DrawFlare(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
    {
        orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);

        var sb = Main.spriteBatch;

        sb.End(out var ss);
        sb.Begin(ss with { TransformMatrix = Main.BackgroundViewMatrix.EffectMatrix });
        {
            DrawFlare((0f, 60f), 1.2f, Color.LightGoldenrodYellow with { A = 0 });
            DrawFlare((20f, 75f), 1.5f, (Color.Blue * 0.4f) with { A = 0 });
        }
        sb.Restart(in ss);

        return;

        void DrawFlare((float Min, float Max) range, float scale, Color color)
        {
            var curve = MathF.Sin(Terraria.Utils.Remap(animationTimer, range.Min, range.Max, 0f, MathF.PI));

            var flareScale = (MathF.Abs(((animationTimer / range.Max * 10f) % 1) - 0.5f) * 2f) - 0.5f;
            flareScale *= 0.5f * curve;
            flareScale += curve;

            flareScale *= scale;

            var flareTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;

            var flareOrigin = flareTexture.Size() * 0.5f;

            var flarePosition = new Vector2((MeteorPosition.X / (float)Main.maxTilesX) * Main.screenWidth, Main.screenHeight * 0.15f);

            var flareSize = new Vector2(0.5f, 1.4f) * flareScale;

            sb.Draw(flareTexture, flarePosition, null, color, 0f, flareOrigin, flareSize, SpriteEffects.None, 0f);

            flareSize.Y *= 0.6f;
            sb.Draw(flareTexture, flarePosition, null, color, MathHelper.PiOver2, flareOrigin, flareSize, SpriteEffects.None, 0f);
        }
    }

    [ModSystemHooks.ModifySunLightColor]
    private static void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        var lightColor = sky_flash_yellow * MathF.Sin(Terraria.Utils.Remap(animationTimer, 90, 290, 0f, MathF.PI));

        tileColor = Color.Max(tileColor, lightColor * 0.6f);
        backgroundColor = Color.Max(backgroundColor, lightColor * 0.2f);
    }

    public static Point MeteorPosition { get; private set; }

    public static bool MeteorSpawned { get; set; }

    [ModSystemHooks.PreWorldGen]
    public static void PreWorldGen()
    {
        MeteorPosition = Point.Zero;
        MeteorSpawned = false;
    }

    [ModSystemHooks.OnWorldLoad]
    public static void OnWorldLoad()
    {
        animationTimer = 0;
        checkTime = -(60 * 20);
        MeteorPosition = Point.Zero;
        MeteorSpawned = false;
    }

    private sealed class Inner : ModSystem
    {
        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: Why is this saved as a vector?
            tag.Set(nameof(MeteorPosition), MeteorPosition.ToVector2());
            tag.Set(nameof(MeteorSpawned), MeteorSpawned);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            MeteorPosition = tag.Get<Vector2>(nameof(MeteorPosition)).ToPoint();
            MeteorSpawned = tag.Get<bool>(nameof(MeteorSpawned));
        }
    }

    private static int threshold = 400;
    private static int maxThreshold = 400;

    private static int checkTime;

    [ModSystemHooks.PostUpdateEverything]
    public static void UpdateMeteorPosition()
    {
        if (MeteorPosition == Point.Zero)
        {
            if (!NPC.downedBoss2)
            {
                checkTime = -(60 * 20);
                return;
            }

            checkTime++;

            if (checkTime < 0 || checkTime % 60 != 0)
            {
                return;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Main.LocalPlayer.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY) * 16) < 3000
                 || Main.LocalPlayer.position.Y > Main.worldSurface)
                {
                    FindPosition();
                }
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                var shouldCheck = true;
                foreach (Player player in Main.ActivePlayers)
                {
                    if (player.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY) * 16) > 5000
                     && player.position.Y < Main.worldSurface)
                        shouldCheck = false;
                }

                if (shouldCheck)
                {
                    FindPosition();
                }
            }
        }
        else
        {
            const int fall_duration = 90;

            if (!MeteorSpawned && animationTimer > fall_duration + 8)
            {
                animationTimer = 0;
            }
            if (MeteorSpawned && animationTimer >= Assets.Sounds.Misc.MeteorCrash.Asset.FrameDuration + 70)
            {
                return;
            }

            animationTimer++;
            switch (animationTimer)
            {
                // Meteor fall sound
                case 1:
                    SoundEngine.PlaySound(Assets.Sounds.Misc.MeteorFall.Asset);
                break;
                // Meteor landing sound
                case fall_duration:
                    SoundEngine.PlaySound(Assets.Sounds.Misc.MeteorCrash.Asset, MeteorPosition.ToWorldCoordinates(), attenuationDistance: 150000f);
                break;
                // Meteor landing text
                case fall_duration + 8:
                    Main.NewText(Mods.Everware.MeteorLandingGen.GetTextValue());
                    MeteorSpawned = true;
                    // MeteorGeneration.GenerateWholeSite(MeteorPosition);
                break;
                // Screen shake and effects
                case > fall_duration + 8 and < 400:
                {
                    float intensity = MathHelper.Lerp(1f, 0f, (animationTimer - 60f) / 340f);

                    ScreenEffects.DimScreen(intensity * 0.1f);
                    ScreenEffects.ZoomScreen(-intensity * 0.05f);
                    ScreenEffects.AddScreenShake(Main.LocalPlayer.Center, intensity * 10f, 0.8f);
                    break;
                }
            }
        }
    }

    private static readonly Thread meteor_locator_thread = new(() =>
    {
        var tries = 100;

        while (threshold >= 200)
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
                maxThreshold = 300;
                break;
            }
        }

        if (threshold < 200)
        {
            maxThreshold = 300;
        }

        checkTime = 60 * (maxThreshold < 400 ? 60 : 30);
    });

    public static void FindPosition()
    {
        meteor_locator_thread.Start();
    }

    private static bool IsPositionBlacklisted(Point pt)
    {
        // if the tile at this exact spot is blacklisted, explode the rest of this logic right now
        if (Main.tile[pt].HasTile && MeteorGeneration.BlacklistedBlocks.Contains(Main.tile[pt].TileType)) return true;

        // check every container. if its origin is less than 3 tiles away from the spot, it's blacklisted
        foreach (Chest chest in Main.chest)
        {
            if (chest is null)
            {
                continue;
            }

            if (Math.Abs(chest.x - pt.X) < 3
             && Math.Abs(chest.y - pt.Y) < 3)
            {
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
        if (slope > threshold)
        {
            threshold -= 10;
            return true;
        }

        return false;
    }
}
