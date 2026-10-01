using Everware.Common;
using Everware.Common.Colors;
using Everware.Common.Systems;
using Everware.Utils;
using MonoMod.Cil;
using System.IO;
using System.Threading;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Meteor;

public static class MeteorLanding
{
    public class MeteorPositionPacket : EverPacket
    {
        public int X;
        public int Y;
        public override void Read(Mod mod, BinaryReader reader, int playerID)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();

            MeteorPosition = new Point(X, Y);
        }
        public override void Write(ModPacket packet)
        {
            packet.Write(X);
            packet.Write(Y);
        }
    }

    [OnLoad]
    private static void Load()
    {
        On_Main.DrawSunAndMoon += DrawSunAndMoon_DrawFlare;
        On_Main.DrawSurfaceBG_BackMountainsStep1 += DrawSurfaceBG_BackMountainsStep1;
        On_Main.DrawSurfaceBG_BackMountainsStep2 += DrawSurfaceBG_BackMountainsStep2;
        IL_Main.DrawSurfaceBG += DrawSurfaceBG;
        On_Main.DrawSurfaceBG += DrawSurfaceBG;

        On_Main.DrawInfernoRings += DrawInfernoRings_Shockwave;
    }

    private static void DrawInfernoRings_Shockwave(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        var sb = Main.spriteBatch;

        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        var distance = (animationTimer - 110) * 300f;

        var position = MeteorPosition.ToWorldCoordinates() - Main.screenPosition;
        position += position.DirectionTo(screenSize * 0.5f) * distance;

        var texture = Assets.Textures.Meteor.Shockwave.Asset.Value;

        var origin = texture.Size() * 0.5f;

        var scale = screenSize.Length() / texture.Height;

        var rotation = position.DirectionTo(screenSize * 0.5f).ToRotation();

        var color = sky_flash_blue * 0.4f;
        color.A = 0;

        sb.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 0f);

        color = sky_flash_yellow;
        color.A = 0;

        position -= position.DirectionTo(screenSize * 0.5f) * 140;

        sb.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.FlipVertically, 0f);
    }

    // TODO: Shill Rosemary's particle system to the Everware team.
    private record struct TrailSparkle(bool Active, Vector2 Offset, Vector2 Velocity, float RotationalVelocity, float Parallax, float TimeLeft, float Increment);

    private static readonly TrailSparkle[] sparkles_sky = new TrailSparkle[128];
    private static readonly TrailSparkle[] sparkles_far = new TrailSparkle[128];
    private static readonly TrailSparkle[] sparkles_middle = new TrailSparkle[128];
    private static readonly TrailSparkle[] sparkles_near = new TrailSparkle[256];

    private static int animationTimer;

    private static readonly Color sky_flash_blue = new Color(235, 153, 255);
    private static readonly Color sky_flash_yellow = new Color(255, 138, 112);

    private static void DrawSparkles(SpriteBatch sb, TrailSparkle[] sparkles)
    {
        var texture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;

        var origin = texture.Size() * 0.5f;

        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        var from = GetFlarePosition();

        // Best way we can respect zoom here, looks mediocre in motion but should be fine at constant zooms.
        var to = MeteorPosition.ToWorldCoordinates() - Main.screenPosition;
        {
            to -= screenSize * 0.5f;
            to *= Main.GameZoomTarget;
            to += screenSize * 0.5f;
        }

        foreach (var sparkle in sparkles)
        {
            if (!sparkle.Active)
            {
                continue;
            }

            var zoom = MathHelper.Lerp(1f, Main.GameZoomTarget, MathF.Pow(sparkle.Parallax, 1.3f));

            var position = Vector2.Lerp(from, to, MathF.Pow(sparkle.Parallax, 1.3f)) + (sparkle.Offset * zoom * MathF.Pow(sparkle.Parallax, 1.5f));

            var scale = (1f - MathF.Pow(sparkle.TimeLeft, 2.3f)) * (1f - MathF.Pow(1f - sparkle.Parallax, 3.4f));

            var bump = MathF.Sin(Terraria.Utils.Remap(sparkle.TimeLeft, 1f - (sparkle.Increment * 25f), 1f, 0f, MathF.PI));

            scale += bump * 0.85f;

            var color = Color.HslLerp(sky_flash_blue, sky_flash_yellow, sparkle.Parallax * scale) * (1f - MathF.Pow(1f - sparkle.Parallax, 2f));
            color.A = 0;

            var white = Color.White * sparkle.Parallax * scale;
            white.A = 0;

            scale *= 0.9f;

            var size = new Vector2(0.3f, 1.4f) * scale;

            var phase = (float)Main.timeForVisualEffects;
            phase *= 0.01f;

            var yScale = Terraria.Utils.Remap(MathF.Sin(phase) * sparkle.Parallax, -1f, 1f, 0.7f, 1f);

            sb.Draw(texture, position, null, color, 0f, origin, size * yScale, SpriteEffects.None, 0f);
            sb.Draw(texture, position, null, white, 0f, origin, size * yScale * 0.35f, SpriteEffects.None, 0f);

            size *= 0.7f;

            var xScale = Terraria.Utils.Remap(MathF.Cos(phase * 0.97f) * sparkle.Parallax, -1f, 1f, 0.6f, 1.05f);

            sb.Draw(texture, position, null, color, MathHelper.PiOver2, origin, size * xScale, SpriteEffects.None, 0f);
            sb.Draw(texture, position, null, white, MathHelper.PiOver2, origin, size * xScale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    private static void DrawSurfaceBG_BackMountainsStep1(On_Main.orig_DrawSurfaceBG_BackMountainsStep1 orig, Main self, double backgroundTopMagicNumber, float bgGlobalScaleMultiplier, int pushBGTopHack)
    {
        const float brightness = 0.35f;

        var alpha = MathF.Sin(Terraria.Utils.Remap(animationTimer, 50, 290, 0f, MathF.PI));

        var interpolated = Color.HslLerp(sky_flash_blue, sky_flash_yellow, alpha * brightness);

        var lightColor = interpolated * alpha * brightness;

        lightColor = Color.Max(Main.ColorOfSurfaceBackgroundsBase, lightColor);

        using var _ = Main.ColorOfSurfaceBackgroundsBase.Override(lightColor);

        orig(self, backgroundTopMagicNumber, bgGlobalScaleMultiplier, pushBGTopHack);

        DrawSparkles(Main.spriteBatch, sparkles_far);
    }

    private static void DrawSurfaceBG_BackMountainsStep2(On_Main.orig_DrawSurfaceBG_BackMountainsStep2 orig, Main self, int pushBGTopHack)
    {
        const float brightness = 0.6f;

        var alpha = MathF.Sin(Terraria.Utils.Remap(animationTimer, 59, 350, 0f, MathF.PI));

        var interpolated = Color.HslLerp(sky_flash_blue, sky_flash_yellow, alpha * brightness);

        var lightColor = interpolated * alpha * brightness;

        lightColor = Color.Max(Main.ColorOfSurfaceBackgroundsBase, lightColor);

        using var _ = Main.ColorOfSurfaceBackgroundsBase.Override(lightColor);

        orig(self, pushBGTopHack);

        DrawSparkles(Main.spriteBatch, sparkles_middle);
    }

    private static void DrawSurfaceBG(ILContext il)
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

                var alpha = MathF.Sin(Terraria.Utils.Remap(animationTimer, 65, 420, 0f, MathF.PI));

                var interpolated = Color.HslLerp(sky_flash_blue, sky_flash_yellow, alpha * brightness * 0.7f);

                var lightColor = interpolated * alpha * brightness;

                lightColor = Color.Max(Main.ColorOfSurfaceBackgroundsModified, lightColor);
                Main.ColorOfSurfaceBackgroundsModified = lightColor;
            }
        );
    }

    private const float meteor_fall_start = 42;

    private const float meteor_fall_end = 87;

    private static float GetMeteorInterpolant(float time)
    {
        var interpolator = Terraria.Utils.Remap(time, meteor_fall_start, meteor_fall_end, 0f, 1f);

        return MathF.Pow(interpolator, 2.3f);
    }

    private static void DrawSurfaceBG(On_Main.orig_DrawSurfaceBG orig, Main self)
    {
        orig(self);

        var sb = Main.spriteBatch;

        if (!Main.BackgroundEnabled)
        {
            DrawSparkles(sb, sparkles_sky);
            DrawSparkles(sb, sparkles_far);
            DrawSparkles(sb, sparkles_near);
        }

        DrawSparkles(sb, sparkles_near);

        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        var from = GetFlarePosition();

        var to = MeteorPosition.ToWorldCoordinates() - Main.screenPosition;
        {
            to -= screenSize * 0.5f;
            to *= Main.GameZoomTarget;
            to += screenSize * 0.5f;
        }

        if (animationTimer < meteor_fall_start
         || animationTimer > meteor_fall_end)
        {
            return;
        }

        var interpolator = GetMeteorInterpolant(animationTimer);

        var flatInterpolator = Terraria.Utils.Remap(animationTimer, meteor_fall_start, meteor_fall_end, 0f, 1f);

        var meteorPosition = Vector2.Lerp(from, to, interpolator);

        var meteorTexture = Assets.Textures.Meteor.Falling.Asset.Value;

        sb.Draw(meteorTexture, meteorPosition, null, Color.Black, 0f, meteorTexture.Size() * 0.5f, flatInterpolator * 2.5f, SpriteEffects.None, 0f);

        sb.End(out var ss);
        sb.Begin(ss with { SortMode = SpriteSortMode.Immediate, SamplerState = SamplerState.LinearWrap });
        {
            var effect = Assets.Effects.Meteor.MeteorLandingFire.CreateMeteorFireShader();

            effect.Parameters.StartColor = sky_flash_yellow.ToVector4();
            effect.Parameters.MiddleColor = sky_flash_blue.ToVector4();
            effect.Parameters.EndColor = new Color(166, 0, 6).ToVector4();

            effect.Parameters.Time = Main.GlobalTimeWrappedHourly * 2f;

            effect.Apply();

            var noise = Assets.Textures.Misc.PerlinNoise.Asset.Value;

            var size = new Vector2(800 * flatInterpolator, 200 * (1f - MathF.Pow(1f - flatInterpolator, 2f))) / noise.Size();
            size *= flatInterpolator * 2;

            var rotation = from.DirectionTo(to).ToRotation();

            var origin = noise.Size() * new Vector2(0.825f, 0.5f);

            sb.Draw(noise, meteorPosition, null, Color.White, rotation, origin, size, SpriteEffects.None, 0f);
        }
        sb.Restart(in ss);
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

            DrawSparkles(sb, sparkles_sky);
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

            var flarePosition = GetFlarePosition();

            var flareSize = new Vector2(0.5f, 1.4f) * flareScale;

            sb.Draw(flareTexture, flarePosition, null, color, 0f, flareOrigin, flareSize, SpriteEffects.None, 0f);

            flareSize.Y *= 0.6f;
            sb.Draw(flareTexture, flarePosition, null, color, MathHelper.PiOver2, flareOrigin, flareSize, SpriteEffects.None, 0f);
        }
    }

    private static Vector2 GetFlarePosition()
    {
        return new Vector2((1f - (MeteorPosition.X / (float)Main.maxTilesX)) * Main.screenWidth, Main.screenHeight * 0.15f);
    }

    [ModSystemHooks.ModifySunLightColor]
    private static void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        var lightColor = sky_flash_yellow * MathF.Sin(Terraria.Utils.Remap(animationTimer, 50, 690, 0f, MathF.PI));

        tileColor = Color.Max(tileColor, lightColor * 0.6f);
        backgroundColor = Color.Max(backgroundColor, lightColor * 0.2f);
    }

    public static Point MeteorPosition { get; private set; }

    public static bool MeteorSpawned { get; set; }

    [ModSystemHooks.PreWorldGen]
    private static void PreWorldGen()
    {
        MeteorPosition = Point.Zero;
        MeteorSpawned = false;
    }

    [ModSystemHooks.OnWorldLoad]
    private static void OnWorldLoad()
    {
        animationTimer = 0;
        checkTime = -(60 * 20);
        MeteorPosition = Point.Zero;
        MeteorSpawned = false;
        ClearSparkles();
    }

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload()
    {
        ClearSparkles();
    }

    [ModSystemHooks.ClearWorld]
    private static void ClearWorld()
    {
        ClearSparkles();
    }

    private static void ClearSparkles()
    {
        Array.Clear(sparkles_sky);
        Array.Clear(sparkles_far);
        Array.Clear(sparkles_middle);
        Array.Clear(sparkles_near);
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

    private static int checkTime = -(60 * 5);

    [ModSystemHooks.PostUpdateEverything]
    public static void UpdateMeteorPosition()
    {
        const int fall_duration = 120;

        UpdateSparkles(sparkles_sky);
        UpdateSparkles(sparkles_far);
        UpdateSparkles(sparkles_middle);
        UpdateSparkles(sparkles_near);

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
            else if (Main.netMode == NetmodeID.Server && Main.ActivePlayers.span.Length > 0)
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
            if (!MeteorSpawned && animationTimer > fall_duration + 60)
            {
                animationTimer = 0;
                ClearSparkles();
            }
            if (MeteorSpawned && animationTimer >= 1700)
            {
                return;
            }

            UpdateEffects();
        }

        return;

        static void UpdateSparkles(TrailSparkle[] sparkles)
        {
            for (var i = 0; i < sparkles.Length; i++)
            {
                ref var sparkle = ref sparkles[i];

                if (!sparkle.Active)
                {
                    continue;
                }

                sparkle.TimeLeft += sparkle.Increment;

                sparkle.Velocity = sparkle.Velocity.RotatedBy(sparkle.RotationalVelocity);
                sparkle.RotationalVelocity *= 0.996f;

                sparkle.Offset += sparkle.Velocity;
                sparkle.Velocity *= 0.998f;

                if (sparkle.TimeLeft < 1f)
                {
                    continue;
                }

                sparkle.Active = false;
            }
        }

        static void UpdateEffects()
        {
            animationTimer++;
            switch (animationTimer)
            {
                // Meteor fall sound
                case 1:
                    SoundEngine.PlaySound(Assets.Sounds.Misc.MeteorFall.Asset);
                break;
                // Meteor landing text
                case fall_duration + 20:
                    Main.NewText(Mods.Everware.MeteorLandingGen.GetTextValue());
                    MeteorSpawned = true;
                    // MeteorGeneration.GenerateWholeSite(MeteorPosition);
                break;
            }

            // Shockwave
            var distance = (animationTimer - 110) * 300f;

            var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

            if (distance > MeteorPosition.ToWorldCoordinates().Distance(Main.screenPosition + screenSize) - 200f
             && distance < MeteorPosition.ToWorldCoordinates().Distance(Main.screenPosition + screenSize) + 400f)
            {
                ScreenEffects.AddScreenShake(MeteorPosition.ToWorldCoordinates(), 23f, 0.99f, 150000f);
                SoundEngine.PlaySound(Assets.Sounds.Misc.MeteorCrash.Asset, MeteorPosition.ToWorldCoordinates(), attenuationDistance: 150000f);
            }

            // Star trail
            if (animationTimer < meteor_fall_start
             || animationTimer > meteor_fall_end)
            {
                return;
            }

            var priorInterpolator = GetMeteorInterpolant(animationTimer);

            var interpolator = GetMeteorInterpolant(animationTimer + 1);

            for (int i = 0; i < (int)(20 * (1f - MathF.Pow(1f - interpolator, 1.2f))); i++)
            {
                SpawnSparkle(Main.rand.NextFloat(priorInterpolator, interpolator));
            }
        }

        static void SpawnSparkle(float depth)
        {
            var target = depth switch
            {
                < 0.15f => sparkles_sky,
                < 0.2f => sparkles_far,
                < 0.4f => sparkles_middle,
                _ => sparkles_near,
            };

            var index = FindFirstInactive(target);
            if (index == -1)
            {
                return;
            }

            var offset = Main.rand.NextVector2Unit();

            target[index] = new TrailSparkle(
                true,
                offset * Main.rand.NextFloat(60f, 300f),
                offset * Main.rand.NextFloat(0.6f, 4f),
                Main.rand.NextFloat(-0.006f, 0.006f),
                depth,
                Main.rand.NextFloat(0f, 0.3f),
                Main.rand.NextFloat(0.0002f, 0.001f)
            );

            return;

            static int FindFirstInactive(TrailSparkle[] sparkles)
            {
                for (var i = 0; i < sparkles.Length; i++)
                {
                    if (sparkles[i].Active)
                    {
                        continue;
                    }

                    return i;
                }

                return -1;
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
                if (Main.dedServ)
                {
                    new MeteorPositionPacket
                    {
                        X = p.X,
                        Y = p.Y
                    }.Send();
                }
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
