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
            BlueGlow = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW / 2, offH / 2));
        });
    }

    public override void Unload()
    {
        base.Unload();

        BlueGlow.Dispose();
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
        DustType = DustID.CrystalSerpent_Pink;
        AddMapEntry(new Color(245, 242, 150));
        Main.tileLighted[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<CharredSoilTile>();
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

                    Point a = new Point(topLeft.X + i, topLeft.Y + j);

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
        var asset = Assets.Textures.Meteor.Tiles.StarCrossedGrassGlow.Asset;

        int paint = Main.tile[i, j].TileColor;

        Texture2D? texture = null;

        bool useColor = paint > PaintID.None && !TryGetPaintTexture(paint, asset, out texture);

        texture ??= asset.Value;

        DrawingUtils.DrawSlopedTile(Main.spriteBatch, texture, i, j, Color.White, new Vector2(8f, 8f) - ScreenOffset);
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
                new Streak(pp.ToVector2() * 16).Spawn(StreakLayer);
            }
        }
    }
}