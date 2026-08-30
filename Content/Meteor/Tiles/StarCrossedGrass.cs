using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class StarCrossedGrassTile : EverTile
{
    public static RenderTargetLease BlueGlow;
    public override void Load()
    {
        base.Load();

        ThreadUtils.RunOnMainThread(() =>
        {
            BlueGlow = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH));
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
        AddMapEntry(new Color(250, 200, 219));
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

        using (BlueGlow.Scope(clearColor: Color.Black))
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
                        var asset = Assets.Textures.Misc.SmallGlow.Asset;
                        Main.EntitySpriteDraw(asset.Value, (a.ToVector2() * 16) + new Vector2(8, 8) - Main.screenPosition, asset.Frame(), new Color(1f, 1f, 1f, 0f), 0f, asset.Frame().Size() / 2f, 2f, SpriteEffects.None);
                    }
                }
            }
            Main.spriteBatch.End();
        }

        var eff = Assets.Effects.Meteor.StarCrossedGlow.CreateGlow();

        eff.Parameters.BlueGlow = BlueGlow.Target;
        eff.Parameters.Color = [
            Color.Blue.ToVector4(),
            Color.Blue.ToVector4(),
            Color.LightBlue.ToVector4() * new Vector4(0.7f, 0.7f, 1f, 1f),
            Color.CornflowerBlue.ToVector4() * new Vector4(0.7f, 0.7f, 1f, 1f),
            Color.Lerp(Color.LightSkyBlue, Color.White, (float)Math.Sin(GlobalTimer.Value / 20f)).ToVector4()
        ];

        eff.Apply();

        Main.spriteBatch.Begin(sb with { CustomEffect = eff.Shader });

        base.ExtraDrawEverything();
    }
    public override void ExtraDrawSingleTile(int i, int j)
    {
        var asset = Assets.Textures.Meteor.Tiles.StarCrossedGrassGlow.Asset;
        DrawingUtils.DrawSlopedTile(Main.spriteBatch, asset, i, j, Color.White, Vector2.Zero);
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