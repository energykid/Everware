using Everware.Content.Base;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class SpiritGrassTile : EverTile
{
    ParticleLayer StreakLayer = new();
    public class Streak : Particle
    {
        public override Asset<Texture2D> Texture => Assets.Textures.Meteor.Tiles.SpiritGrassStreak.Asset;
        public Streak(Vector2 pos) : base(pos, Vector2.Zero, Vector2.One, null, null)
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
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/SpiritGrassTile";
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

        r *= amt;
        g *= amt2;
        b *= amt2;

        base.ModifyLight(i, j, ref r, ref g, ref b);
    }
    public override void ExtraDrawEverything()
    {
        StreakLayer.Draw();
        base.ExtraDrawEverything();
    }
    public override void ExtraDrawSingleTile(int i, int j)
    {
        var asset = Assets.Textures.Meteor.Tiles.SpiritGrassGlow.Asset;
        DrawingUtils.DrawSlopedTile(Main.spriteBatch, asset, i, j, new Color(0.5f, 0.5f, 0.5f, 0f), Vector2.Zero);
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