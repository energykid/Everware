using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Everware.Content.Hell;

public class HellPodTileEntity : ModTileEntity
{
    public float Rotation = 0f;
    public bool Placed = false;
    public override void OnKill()
    {
        SoundEngine.PlaySound(Assets.Sounds.Tile.HellPodBreak.Asset with { PitchVariance = 0.1f }, new Vector2((Position.X + 1) * 16, (Position.Y + 1) * 16));
    }
    public override bool IsTileValidForEntity(int x, int y)
    {
        return Main.tile[Position].TileType == ModContent.TileType<HellPod>();
    }
    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Main.tile[Position].Get<HellPodData>().Rotation);
    }
    public override void NetReceive(BinaryReader reader)
    {
        Main.tile[Position].Get<HellPodData>().Rotation = reader.ReadSingle();
    }
    public override void SaveData(TagCompound tag)
    {
        tag.Set("Rotation", Main.tile[Position].Get<HellPodData>().Rotation);
    }
    public override void LoadData(TagCompound tag)
    {
        Main.tile[Position].Get<HellPodData>().Rotation = tag.Get<float>("Rotation");
        Placed = true;
    }
    public override void Update()
    {
        if (!Placed)
        {
            Main.tile[Position].Get<HellPodData>().Rotation = MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
            Placed = true;
        }

        int i = Position.X;
        int j = Position.Y;

        Vector2 p2 = new Vector2(i * 16, j * 16);

        int topDistance = 0;
        int bottomDistance = 0;

        int referenceTopDistance = 0;
        int referenceBottomDistance = 0;

        // if the distance has been calculated last frame, get the distances to compare to this frame
        if (Main.tile[i, j].Get<HellPodData>().DistanceYetCalculated)
        {
            referenceTopDistance = Main.tile[i, j].Get<HellPodData>().DistanceTop;
            referenceBottomDistance = Main.tile[i, j].Get<HellPodData>().DistanceBottom;
        }

        // distance calculator
        HellPod.DoThingALot(i, j, (ii, jj) =>
        {
            if (jj < p2.Y) topDistance = (int)Math.Abs(jj - p2.Y);
            else bottomDistance = (int)Math.Abs(jj - p2.Y);
        });

        // save the distance between the center and either end
        // this is so that we can check if it's changed
        // if it HAS changed, then we kill the pod early because that means the player has broken one of the tiles holding it up
        Main.tile[i, j].Get<HellPodData>().DistanceTop = topDistance;
        Main.tile[i, j].Get<HellPodData>().DistanceBottom = bottomDistance;

        // if the distance was calculated last frame, these values will both be more than 0, therefore we do the actual comparison and act accordingly
        if (referenceTopDistance != 0 && referenceBottomDistance != 0)
        {
            // if either distance is different, just damage the pod 3 times so it breaks and runs all related behavior
            if (referenceTopDistance != topDistance || referenceBottomDistance != bottomDistance)
            {
                for (int x = 0; x < 3; x++)
                    HellPod.DamagePod(i, j);
            }
        }

        Main.tile[i, j].Get<HellPodData>().DistanceYetCalculated = true;

        if (Main.tile[Position].TileFrameY > 18 * 3 * 2)
        {
            Kill(Position.X, Position.Y);
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    WorldGen.KillTile(Position.X + x, Position.Y + y);
                }
            }
        }
    }
}

public class HellPod : EverMultitile
{
    public override string Texture => "Everware/Assets/Textures/Hell/HellPod";
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

        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<HellPodTileEntity>().Generic_HookPostPlaceMyPlayer;

        TileObjectData.newTile.UsesCustomCanPlace = true;
    }
    public override void PlaceInWorld(int i, int j, Item item)
    {
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
        float frame = (float)Math.Floor((float)Main.tile[i, j].TileFrameY / (18 * 3));
        SoundEngine.PlaySound(Assets.Sounds.Tile.HellPodCrack.Asset with { PitchVariance = 0.2f, Pitch = frame * 0.4f }, new Vector2(i * 16, j * 16));

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
        }
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        DamagePod(i, j);

        return false;
    }
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        noBreak = true;

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        float rot = Main.tile[i, j].Get<HellPodData>().Rotation;

        var Placed = Assets.Textures.Hell.HellPodPlaced.Asset;
        var Glow = Assets.Textures.Hell.HellPodGlow.Asset;
        var Light = Assets.Textures.Hell.HellPodLight.Asset;

        Tile tile = Main.tile[i, j];

        Vector2 fr = new Vector2((float)Math.Floor(tile.TileFrameX / 54f), (float)Math.Floor(tile.TileFrameY / 54f));

        Vector2 p = new Vector2(i * 16, j * 16) + new Vector2(0, -fr.Y).RotatedBy(rot) + new Vector2(0, 2);
        Vector2 p2 = new Vector2(i * 16, j * 16) + new Vector2(0, 2);

        spriteBatch.Draw(Light.Value, p2 - Main.screenPosition + new Vector2(24), Light.Frame(), Color.Black.MultiplyRGBA(new(1f, 1f, 1f, 0.1f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.3f, SpriteEffects.None, 0);
        spriteBatch.Draw(Light.Value, p2 - Main.screenPosition + new Vector2(24), Light.Frame(), Color.OrangeRed.MultiplyRGBA(new(0.2f, 0.2f, 0.2f, 0f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.2f, SpriteEffects.None, 0);

        Rectangle frame = Placed.Frame(2, 3, (int)fr.X, (int)fr.Y);

        Vector2 origin = new Vector2(24f, 24f).RotatedBy(-rot);

        spriteBatch.Draw(Placed.Value, p - Main.screenPosition + new Vector2(24), frame, Lighting.GetColor((p / 16).ToPoint()), rot, origin, 1f, SpriteEffects.None, 0);
        spriteBatch.Draw(Glow.Value, p - Main.screenPosition + new Vector2(24), frame, Color.White, rot, origin, 1f, SpriteEffects.None, 0);

        if (Main.tile[i, j].TileFrameX % (18 * 3) < 8 && Main.tile[i, j].TileFrameY % (18 * 3) < 8)
        {
            var Asset = Assets.Textures.Hell.HellPodStalk.Asset;

            // draw function
            DoThingALot(i, j, (ii, jj) =>
            {
                int frameX = 0;
                int frameY = 0;
                Rectangle frame = Asset.Frame(2, 2, frameX, frameY);
                spriteBatch.Draw(Asset.Value, new Vector2(ii, jj) - Main.screenPosition, frame, Lighting.GetColor((new Vector2(ii + 8, jj + 8) / 16).ToPoint()), rot, new Vector2(8f), 1f, SpriteEffects.None, 0);
            });
        }
    }
    public delegate void ThingToDo(int i, int j);

    /// <summary>
    /// This performs an action in a rotated column protruding outwards from this hell pod
    /// </summary>
    /// <param name="i">Center X, in tile coordinates</param>
    /// <param name="j">Center Y, in tile coordinates</param>
    /// <param name="action">The thing to do at each set of world coordinates (NOT tile coordinates because this needs more precision)</param>
    public static void DoThingALot(int i, int j, ThingToDo action)
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
public struct HellPodData : ITileData
{
    public int DistanceTop;
    public int DistanceBottom;
    public bool DistanceYetCalculated;
    public bool Enabled;
    public float Rotation;
    public int Variant;
}