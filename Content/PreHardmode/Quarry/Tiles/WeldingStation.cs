using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;

namespace Everware.Content.PreHardmode.Quarry.Tiles;

public class WeldingStation : EverMultitile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style6x3);

        TileObjectData.newTile.DrawYOffset = 6;

        TileObjectData.addTile(Type);

        AnimationFrameHeight = 3 * 18;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        if (++frameCounter > 4)
        {
            if (++frame >= 3)
            {
                frame = 0;
            }
            frameCounter = 0;
        }
        base.AnimateTile(ref frame, ref frameCounter);
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0)
        {
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }
    }

    public static readonly Asset<Texture2D> LaserTexture1 = ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Quarry/Tiles/WeldingStation_LaserGrid");
    public static readonly Asset<Texture2D> LaserTexture2 = ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Quarry/Tiles/WeldingStation_LaserGrid2");
    public static readonly Asset<Texture2D> LaserTexture2Glow = ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Quarry/Tiles/WeldingStation_LaserGrid2_Glow");

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        float Timer = GlobalTimer.Value;
        Color LightColor = Lighting.GetColor(new Point(i, j));

        float X = Easing.InOutQuad(LaserGridTimer.LaserGridCurrentX);

        Vector2 LaserX1 = new Vector2(-10 + (X * 10), 0f);
        Vector2 LaserX2 = new Vector2(10 - (X * 10), 0f);

        Vector2 BaseVector = new Vector2(i * 16, j * 16) + new Vector2(68, 6);

        Vector2 SparkBaseVector = new Vector2(i * 16, j * 16) + new Vector2(22, 20);

        Dust d1 = Dust.NewDustPerfect(SparkBaseVector + new Vector2(-16, 0) + new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)), DustID.MinecartSpark, new Vector2(-2f, -1f).RotatedByRandom(0.5f));
        Dust d2 = Dust.NewDustPerfect(SparkBaseVector + new Vector2(16, 0) + new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)), DustID.MinecartSpark, new Vector2(2f, -1f).RotatedByRandom(0.5f));

        d1.noGravity = false;
        d2.noGravity = false;

        d1.fadeIn = 1f;
        d2.fadeIn = 1f;

        Vector2 X1 = BaseVector + LaserX1;

        Vector2 X2 = BaseVector + LaserX2;

        if (!LaserGridTimer.LaserGridOn)
        {
            spriteBatch.Draw(LaserTexture1.Value, X1 - Main.screenPosition, LightColor);
            spriteBatch.Draw(LaserTexture1.Value, X2 - Main.screenPosition, LightColor);
        }
        else
        {
            spriteBatch.Draw(LaserTexture2.Value, BaseVector + LaserX1 - Main.screenPosition, LightColor);
            spriteBatch.Draw(LaserTexture2.Value, BaseVector + LaserX2 - Main.screenPosition, LightColor);
            spriteBatch.Draw(LaserTexture2Glow.Value, BaseVector + LaserX1 - Main.screenPosition, Color.White);
            spriteBatch.Draw(LaserTexture2Glow.Value, BaseVector + LaserX2 - Main.screenPosition, Color.White);
        }
    }
    class LaserGridTimer : ModSystem
    {

        public static float LaserGridT = 0f;
        public static float LaserGridCurrentX = 0f;
        public static float LaserGridLastX = 0f;
        public static float LaserGridNextX = 0f;
        public static bool LaserGridOn = false;
        public override void PostUpdateEverything()
        {
            LaserGridT -= 1f;
            if (LaserGridT < 0)
            {
                LaserGridLastX = LaserGridNextX;
                LaserGridT = 60;
                if (LaserGridOn)
                {
                    LaserGridOn = false;
                    LaserGridNextX = 0;
                }
                else
                {
                    LaserGridOn = true;
                    LaserGridNextX = 0.65f;
                }
            }
            LaserGridCurrentX = MathHelper.Lerp(LaserGridNextX, LaserGridLastX, (LaserGridT / 60));
        }
    }
}