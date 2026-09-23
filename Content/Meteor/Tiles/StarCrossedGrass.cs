using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class StarCrossedGrassTile : EverTile
{

    #region Magic Stone Paint Aura
    private readonly Dictionary<int, EverTileRenderTargetHolder> mPaintCache = [];

    public bool TryGetMPaintTexture(
        int paintColor,
        Asset<Texture2D> asset,
        [NotNullWhen(true)] out Texture2D? texture
    )
    {
        texture = null;

        if (mPaintCache.TryGetValue(paintColor, out EverTileRenderTargetHolder? holder) &&
            holder.IsReady)
        {
            texture = holder.Target;

            return true;
        }

        var newHolder = new EverTileRenderTargetHolder(paintColor, asset);

        mPaintCache[paintColor] = newHolder;

        Main.instance.TilePaintSystem._requests.Add(newHolder);

        return false;
    }
    #endregion
    public static RenderTargetLease BlueGlow;
    public override void Load()
    {
        base.Load();

        ThreadUtils.RunOnMainThread(() =>
        {
            if (Main.netMode != NetmodeID.Server)
                BlueGlow = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW / 2, offH / 2));
        });
    }

    public override void Unload()
    {
        base.Unload();

        ThreadUtils.RunOnMainThread(() =>
        {
            if (Main.netMode != NetmodeID.Server)
                BlueGlow.Dispose();
        });
    }
    ParticleLayer StreakLayer = new();
    public override void ExtraDrawPreEverything()
    {
        StreakLayer.Draw();
    }
    public class Streak : Particle
    {
        public override Asset<Texture2D> Texture => Assets.Textures.Meteor.Tiles.StarCrossedGrassStreak.Asset;
        public Streak(Vector2 pos) : base(pos + new Vector2(-8, -8), Vector2.Zero, Vector2.One, null, null)
        {
            AffectedByLight = false;
            FrameCount = new(7, 1);
        }
        public override void Update()
        {
            velocity.Y -= 0.04f;
            base.Update();
            FrameNum.X = MathHelper.Lerp(FrameNum.X, 7, 0.1f);
            if (FrameNum.X >= 6.5f) Kill();
        }
    }
    public override bool UsesExtraTarget => true;
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/StarCrossedGrassTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.YellowStarDust;
        AddMapEntry(new Color(245, 242, 150));
        Main.tileLighted[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<CharredSoilTile>();
    }
    //foliage growth
    public override void RandomUpdate(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        int grassType = tile.TileType;
        // only run foliage if theres actually space here
        if (j > 1)
        {
            Tile above = Main.tile[i, j - 1];

            if (!above.HasTile && WorldGen.genRand.NextBool(10))
            {
                bool b = WorldGen.genRand.NextBool(5);
                WorldGen.PlaceTile(i, j - 1, b ? ModContent.TileType<LargeStarCrossedFoliage>() : ModContent.TileType<StarCrossedFoliage>(), mute: true);
                if (above.HasTile)
                {
                    above.CopyPaintAndCoating(tile);

                    above.TileFrameX = (short)(WorldGen.genRand.Next(23) * 18);
                    if (b) above.TileFrameX = (short)WorldGen.genRand.Next(4);
                }

                if (Main.netMode == NetmodeID.Server && above.HasTile)
                {
                    NetMessage.SendTileSquare(-1, i, j - 1);
                }
            }
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.8f;
        g = 0.4f;
        b = 0.85f;

        var amt = MathHelper.Lerp(1f, 0.5f, (float)Math.Sin((GlobalTimer.Value / 40f) + (i / 30f) + (j / 80f)));
        var amt2 = MathHelper.Lerp(1f, 0.5f, (float)Math.Sin(((GlobalTimer.Value + 25f) / 40f) + (i / 30f) + (j / 80f)));

        r *= 0.65f;
        g *= 0.9f;

        r *= amt;
        g *= amt2;
        b *= 0.2f;

        base.ModifyLight(i, j, ref r, ref g, ref b);
    }
    public override void ExtraDrawEverything()
    {
        Main.spriteBatch.End(out var sb);

        using (BlueGlow.Scope(clearColor: new Color(0.4f, 0.7f, 1f, 0f)))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null);
            for (int i = -8; i < (Main.screenWidth / 16) + 8; i++)
            {
                for (int j = -8; j < (Main.screenHeight / 16) + 8; j++)
                {
                    Point topLeft = (Main.screenPosition / 16).ToPoint();

                    Point a = new Point(Math.Clamp(topLeft.X + i, 0, Main.maxTilesX), Math.Clamp(topLeft.Y + j, 0, Main.maxTilesY));

                    if (Main.tile[a].TileType == ModContent.TileType<MagicStoneTile>())
                    {
                        var asset = Assets.Textures.Meteor.Tiles.MagicStoneGlowRing.Asset;
                        Texture2D? texture = asset.Value;
                        int paint2 = Main.tile[a.X, a.Y].TileColor;
                        bool useColor = paint2 > PaintID.None && !TryGetMPaintTexture(paint2, asset, out texture);

                        texture ??= asset.Value;

                        Main.EntitySpriteDraw(texture, ((a.ToVector2() * 16) + new Vector2(8, 8) - Main.screenPosition) / 2f, asset.Frame(), useColor ? new Color(1f, 1f, 1f, 1f) : Color.LightBlue, 0f, asset.Frame().Size() / 2f, 1f, SpriteEffects.None);
                    }
                }
            }
            Main.spriteBatch.End();
        }

        var eff = Assets.Effects.Meteor.StarCrossedGlow.CreateGlow();

        eff.Parameters.BlueGlow = BlueGlow.Target;
        eff.Parameters.Progress = GlobalTimer.Value / 20f;
        eff.Parameters.Color = [
            Color.Black.ToVector4(),
            Color.DarkGray.ToVector4(),
            Color.DarkGray.ToVector4(),
            Color.Gray.ToVector4(),
            Color.Lerp(Color.LightGray, Color.White, (float)Math.Sin(GlobalTimer.Value / 20f)).ToVector4()
        ];

        eff.Apply();

        Main.spriteBatch.Begin(sb with { CustomEffect = eff.Shader });

        base.ExtraDrawEverything();
    }
    public override void ExtraDrawSingleTile(int i, int j)
    {
        float wind = Main.WindForVisuals + ((float)Math.Sin(GlobalTimer.Value / 30f) * 0.2f);

        var asset = Assets.Textures.Meteor.Tiles.StarCrossedGrassGlow.Asset;

        int paint = Main.tile[i, j].TileColor;

        Texture2D? texture = null;

        bool useColor = paint > PaintID.None && !TryGetPaintTexture(paint, asset, out texture);

        texture ??= asset.Value;

        DrawingUtils.DrawSlopedTile(Main.spriteBatch, texture, i, j, Color.White, new Vector2(8f, 8f) - ScreenOffset);

        if (Main.tile[i, j - 1].TileType == ModContent.TileType<StarCrossedFoliage>())
        {
            int paint2 = Main.tile[i, j - 1].TileColor;

            var asset2 = Assets.Textures.Meteor.Tiles.StarCrossedFoliage.Asset;

            Texture2D? texture2 = null;

            bool useColor2 = paint > PaintID.None && !TryGetPaintTexture(paint2, asset2, out texture2);

            texture2 ??= asset2.Value;

            float offY = 0f;
            float rot = 0f;
            if (Main.tile[i, j].TopSlope)
            { offY = 4f; rot = MathHelper.ToRadians((float)((Main.tile[i, j].LeftSlope) ? -40f : 40f)); }

            rot += (wind * 0.2f);

            DrawFoliage(Main.spriteBatch, texture2, i, j - 1, Color.White, new Vector2(8f, 24f) - ScreenOffset + new Vector2(0, offY), rot);
        }

        if (Main.tile[i, j - 1].TileType == ModContent.TileType<LargeStarCrossedFoliage>())
        {
            int paint2 = Main.tile[i, j - 1].TileColor;

            var asset2 = Assets.Textures.Meteor.Tiles.LargeStarCrossedFoliage.Asset;

            Texture2D? texture2 = null;

            bool useColor2 = paint > PaintID.None && !TryGetPaintTexture(paint2, asset2, out texture2);

            texture2 ??= asset2.Value;

            float offY = 0f;
            float rot = 0f;
            if (Main.tile[i, j].TopSlope)
            { offY = 4f; rot = MathHelper.ToRadians((float)((Main.tile[i, j].LeftSlope) ? -20f : 20f)); }

            rot += (wind * 0.2f);

            DrawLargeFoliage(Main.spriteBatch, texture2, i, j - 1, Color.White, new Vector2(8f, 24f) - ScreenOffset + new Vector2(0, offY), Main.tile[i, j - 1].TileFrameX, rot);
        }
    }

    [ModSystemHooks.PostUpdateDusts]
    public void UpdateStreaks()
    {
        StreakLayer.Update();

        for (int k = 0; k < 30; k++)
        {
            int i = Main.rand.Next(Main.screenWidth / 16);
            int j = Main.rand.Next(Main.screenHeight / 16);

            Point p = (Main.screenPosition / 16f).ToPoint();

            Point pp = p + new Point(i, j);
            Tile t = Main.tile[pp];
            Tile t1 = Main.tile[p + new Point(i, j - 1)];
            if (t.HasTile && t.TileType == Type && !t1.HasTile)
            {
                new Streak(pp.ToVector2() * 16 + new Vector2(16, 0)).Spawn(StreakLayer);
            }
        }
    }

    public static void DrawFoliage(SpriteBatch spriteBatch, Texture2D tex, int i, int j, Color color, Vector2 offset, float rot = 0f)
    {
        var fr = new Rectangle(Main.tile[i, j].TileFrameX, Main.tile[i, j].TileFrameY, 16, 16);
        spriteBatch.Draw(tex, new Vector2(i * 16, j * 16) - Main.screenPosition + new Vector2(0, -2) + offset,
        fr, Color.White, rot, new Vector2(fr.Width / 2f, fr.Height), 1f, SpriteEffects.None, 0f);
    }

    public static void DrawLargeFoliage(SpriteBatch spriteBatch, Texture2D tex, int i, int j, Color color, Vector2 offset, int frame, float rot = 0f)
    {
        var fr = tex.Frame(4, 1, frame);
        spriteBatch.Draw(tex, new Vector2(i * 16, j * 16) - Main.screenPosition + new Vector2(0, -2) + offset,
        fr, Color.White, rot, new Vector2(fr.Width / 2f, fr.Height), 1f, SpriteEffects.None, 0f);
    }
}