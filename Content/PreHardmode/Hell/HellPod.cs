using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Hell;

public class HellPod : EverMultitile
{
    public override int Width => 3;
    public override int Height => 3;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.InfernoFork;
        AddMapEntry(new Color(250, 106, 10));
        MineResist = 100;

        HitSound = null;
        Main.tileNoAttach[Type] = true;
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Main.tile[i, j].TileFrameX < 8 && Main.tile[i, j].TileFrameY % (18 * 3) < 8)
        {
        }
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        return true;
    }
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Lighting.AddLight(i, j, TorchID.Torch, 0.2f);

        if (Main.tile[i, j].TileFrameX % 54 == 0 && Main.tile[i, j].TileFrameY % 54 == 0)
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
    }
    public static void DamagePod(int i, int j)
    {
        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { PitchVariance = 0.5f }, new Vector2(i * 16, j * 16));

        int ii = i;
        int jj = j;

        while (Main.tile[ii, jj].TileFrameX % 54 > 2)
        {
            ii--;
        }
        while (Main.tile[ii, jj].TileFrameY % 54 > 2)
        {
            jj--;
        }

        for (int k = 0; k < 10; k++)
        {
            Dust.NewDust(new Vector2(ii * 16, jj * 16), 16, 16, DustID.ScourgeOfTheCorruptor);
        }

        Main.tile[ii, jj].TileFrameY += 18 * 3;
        if (ii != i || jj != j)
            Main.tile[i, j].TileFrameY += 18 * 3;

        if (Main.tile[ii, jj].TileFrameY >= (18 * 3 * 3))
        {
            SoundEngine.PlaySound(SoundID.Dig, new Vector2(i * 16, j * 16));
            DoThingALot(i, j, (i, j) =>
            {
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDust(new Vector2(ii + 4, jj), 8, 16, DustID.Obsidian);
                }
            });

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    WorldGen.KillTile(ii + x, jj + y);
                }
            }
        }
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        DamagePod(i, j);

        int ii = i;
        int jj = j;

        while (Main.tile[ii, jj].TileFrameX % 54 > 2)
        {
            ii--;
        }
        while (Main.tile[ii, jj].TileFrameY % 54 > 2)
        {
            jj--;
        }

        if (Main.tile[ii, jj].TileFrameY >= (18 * 3 * 3))
        {
            WorldGen.KillTile(ii, jj);
        }

        return Main.tile[ii, jj].TileFrameY >= (18 * 3 * 3);
    }
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        noBreak = true;

        if (Main.tile[i, j].TileFrameY >= (18 * 3 * 3))
        {
            noBreak = false;
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var Placed = AssetReferences.Content.PreHardmode.Hell.HellPodPlaced.Asset;
        var Glow = AssetReferences.Content.PreHardmode.Hell.HellPodGlow.Asset;
        var Light = AssetReferences.Content.PreHardmode.Hell.HellPodLight.Asset;

        Tile tile = Main.tile[i, j];

        Vector2 fr = new Vector2((float)Math.Floor(tile.TileFrameX / 54f), (float)Math.Floor(tile.TileFrameY / 54f));

        Vector2 p = new Vector2(i * 16, j * 16) + new Vector2(0, -fr.Y);

        spriteBatch.Draw(Light.Value, p - Main.screenPosition + new Vector2(48f / 2f), Light.Frame(), Color.Black.MultiplyRGBA(new(1f, 1f, 1f, 0.3f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.4f, SpriteEffects.None, 0);
        spriteBatch.Draw(Light.Value, p - Main.screenPosition + new Vector2(48f / 2f), Light.Frame(), Color.OrangeRed.MultiplyRGBA(new(0.2f, 0.2f, 0.2f, 0.2f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.2f, SpriteEffects.None, 0);

        spriteBatch.Draw(Placed.Value, p - Main.screenPosition, Placed.Frame(2, 3, (int)fr.X, (int)fr.Y), Lighting.GetColor((p / 16).ToPoint()));
        spriteBatch.Draw(Glow.Value, p - Main.screenPosition, Glow.Frame(2, 3, (int)fr.X, (int)fr.Y), Color.White);

        if (Main.tile[i, j].TileFrameX < 8 && Main.tile[i, j].TileFrameY % (18 * 3) < 8)
        {
            var Asset = AssetReferences.Content.PreHardmode.Hell.HellPodStalk.Asset;
            DoThingALot(i, j, (ii, jj) =>
            {
                int frameX = 0;
                int frameY = 0;
                spriteBatch.Draw(Asset.Value, new Vector2(ii, jj) - Main.screenPosition, Asset.Frame(2, 2, frameX, frameY), Lighting.GetColor((new Vector2(ii + 8, jj + 8) / 16).ToPoint()));
            });
        }
    }
    delegate void ThingToDo(int i, int j);
    static void DoThingALot(int i, int j, ThingToDo action)
    {
        Vector2 position = new Vector2(i * 16, j * 16);

        int xx = 16;
        int ii = 2;
        int jj = 32;
        int iterations = 0;
        while (!WorldGen.SolidOrSlopedTile(Main.tile[new Point(i + 1, j + (ii / 16) - 1)]))
        {
            iterations++;
            if (iterations > 70) break;

            ii -= 16;

            Vector2 pos = position + new Vector2(xx, ii);

            action((int)pos.X, (int)pos.Y);
        }
        iterations = 0;
        while (!WorldGen.SolidOrSlopedTile(Main.tile[new Point(i + 1, j + (jj / 16))]))
        {
            iterations++;
            if (iterations > 70) break;

            jj += 16;

            Vector2 pos = position + new Vector2(xx, jj);

            action((int)pos.X, (int)pos.Y);
        }
    }
}

public class HellPodGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;
    public bool HasHitPod = false;
    public override void PostAI(Projectile projectile)
    {
        if (projectile.friendly)
        {
            if (Main.tile[(projectile.Center / 16).ToPoint()].TileType == ModContent.TileType<HellPod>() && Main.tile[(projectile.Center / 16).ToPoint()].HasTile)
            {
                HasHitPod = true;
                projectile.penetrate--;
                Point p = (projectile.Center / 16).ToPoint();
                HellPod.DamagePod(p.X, p.Y);
            }
        }
    }
}