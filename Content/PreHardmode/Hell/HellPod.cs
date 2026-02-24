using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

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
    public override void PlaceInWorld(int i, int j, Item item)
    {
        Main.tile[i, j].Get<HellPodData>().Rotation = MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Main.tile[i, j].TileFrameX < 8 && Main.tile[i, j].TileFrameY % (18 * 3) < 8)
        {

        }
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (Main.tile[i, j].Get<HellPodData>().Enabled != true)
        {
            HellPodDataSaverLoader.HellPodPositions.Add(new Point(i, j));
            Main.tile[i, j].Get<HellPodData>().Rotation = MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
            Main.tile[i, j].Get<HellPodData>().Enabled = true;
        }

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
                    if (HellPodDataSaverLoader.HellPodPositions.Contains(new Point(ii + x, jj + y)))
                        HellPodDataSaverLoader.HellPodPositions.Remove(new Point(ii + x, jj + y));

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
        float rot = Main.tile[i, j].Get<HellPodData>().Rotation;

        var Placed = AssetReferences.Content.PreHardmode.Hell.HellPodPlaced.Asset;
        var Glow = AssetReferences.Content.PreHardmode.Hell.HellPodGlow.Asset;
        var Light = AssetReferences.Content.PreHardmode.Hell.HellPodLight.Asset;

        Tile tile = Main.tile[i, j];

        Vector2 fr = new Vector2((float)Math.Floor(tile.TileFrameX / 54f), (float)Math.Floor(tile.TileFrameY / 54f));

        Vector2 p = new Vector2(i * 16, j * 16) + new Vector2(0, -fr.Y).RotatedBy(rot) + new Vector2(0, 2);
        Vector2 p2 = new Vector2(i * 16, j * 16) + new Vector2(0, 2);

        spriteBatch.Draw(Light.Value, p2 - Main.screenPosition + new Vector2(48f / 2f), Light.Frame(), Color.Black.MultiplyRGBA(new(1f, 1f, 1f, 0.1f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.3f, SpriteEffects.None, 0);
        spriteBatch.Draw(Light.Value, p2 - Main.screenPosition + new Vector2(48f / 2f), Light.Frame(), Color.OrangeRed.MultiplyRGBA(new(0.2f, 0.2f, 0.2f, 0f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.2f, SpriteEffects.None, 0);

        Rectangle frame = Placed.Frame(2, 3, (int)fr.X, (int)fr.Y);

        Vector2 origin = new Vector2(24f, 24f).RotatedBy(-rot);

        spriteBatch.Draw(Placed.Value, p - Main.screenPosition + new Vector2(24), frame, Lighting.GetColor((p / 16).ToPoint()), rot, origin, 1f, SpriteEffects.None, 0);
        spriteBatch.Draw(Glow.Value, p - Main.screenPosition + new Vector2(24), frame, Color.White, rot, origin, 1f, SpriteEffects.None, 0);

        if (Main.tile[i, j].TileFrameX < 8 && Main.tile[i, j].TileFrameY % (18 * 3) < 8)
        {
            var Asset = AssetReferences.Content.PreHardmode.Hell.HellPodStalk.Asset;
            DoThingALot(i, j, (ii, jj) =>
            {
                int frameX = 0;
                int frameY = 0;
                Rectangle frame = Asset.Frame(2, 2, frameX, frameY);
                spriteBatch.Draw(Asset.Value, new Vector2(ii, jj) - Main.screenPosition, frame, Lighting.GetColor((new Vector2(ii + 8, jj + 8) / 16).ToPoint()), rot, new Vector2(8f), 1f, SpriteEffects.None, 0);
            });
        }
    }
    delegate void ThingToDo(int i, int j);
    static void DoThingALot(int i, int j, ThingToDo action)
    {
        float rot = Main.tile[i, j].Get<HellPodData>().Rotation;

        Vector2 position = new Vector2(i * 16, j * 16);

        int xx = 16;
        int ii = 2;
        int jj = 32;
        int iterations = 0;

        Vector2 p1 = (new Vector2(i * 16, j * 16) + new Vector2(8f)) + new Vector2(16, 3).RotatedBy(rot);
        Vector2 p2 = (new Vector2(i * 16, j * 16) + new Vector2(8f)) + new Vector2(16, 32).RotatedBy(rot);

        while (!WorldGen.SolidOrSlopedTile(Main.tile[(p1 / 16).ToPoint()]))
        {
            iterations++;
            if (iterations > 70) break;

            p1 += new Vector2(0, -16).RotatedBy(rot);

            action((int)p1.X, (int)p1.Y);
        }
        iterations = 0;
        while (!WorldGen.SolidOrSlopedTile(Main.tile[(p2 / 16).ToPoint()]))
        {
            iterations++;
            if (iterations > 70) break;

            p2 += new Vector2(0, 16).RotatedBy(rot);

            action((int)p2.X, (int)p2.Y);
        }
    }
}

public class HellPodGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;
    public bool HasHitPod = false;
    public override void PostAI(Projectile projectile)
    {
        if (projectile.friendly && projectile.damage > 0 && !HasHitPod)
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

public class HellPodDataSaverLoader : ModSystem
{
    public static List<Point> HellPodPositions = [];
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Set("EverwareHellPodCount", HellPodPositions.Count);
        for (int i = 0; i < HellPodPositions.Count; i++)
        {
            tag.Set("EverwareHellPod" + i.ToString() + "Position", HellPodPositions[i]);
            tag.Set("EverwareHellPod" + i.ToString() + "Rotation", Main.tile[HellPodPositions[i]].Get<HellPodData>().Rotation);
        }
    }
    public override void LoadWorldData(TagCompound tag)
    {
        for (int i = 0; i < tag.GetInt("EverwareHellPodCount"); i++)
        {
            Point pos = tag.Get<Point>("EverwareHellPod" + i.ToString() + "Position");

            if (!HellPodPositions.Contains(pos))
                HellPodPositions.Add(pos);

            Main.tile[pos].Get<HellPodData>().Enabled = true;
            Main.tile[pos].Get<HellPodData>().Rotation = tag.GetFloat("EverwareHellPod" + i.ToString() + "Rotation");

            Main.NewText(pos.ToString());
        }
    }
}

public struct HellPodData : ITileData
{
    public bool Enabled;
    public float Rotation;
    public int Variant;
}