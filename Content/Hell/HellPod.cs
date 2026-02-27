/* REMOVED TEMPORARILY

using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Everware.Content.Hell;

public class HellPodTileEntity : ModTileEntity
{
    public struct HellPodLootInstance(int ID, int min, int max)
    {
        public int ItemID = ID;
        public int MinStack = min;
        public int MaxStack = max;
    }

    public float HurtIntensity = 0f;
    public bool Placed = false;

    public static List<HellPodLootInstance> PossibleLesserItems => [
        new HellPodLootInstance(ItemID.HellfireArrow, 15, 30),
        new HellPodLootInstance(ItemID.Grenade, 5, 10),
        new HellPodLootInstance(ItemID.Bomb, 7, 9),
        new HellPodLootInstance(ItemID.Dynamite, 3, 6),
        new HellPodLootInstance(ItemID.DemonTorch, 8, 13),
        new HellPodLootInstance(ItemID.Fireblossom, 2, 5),
        new HellPodLootInstance(ItemID.FireblossomSeeds, 1, 2)
        ];
    public static List<int> PossibleGreaterItems => [
    ItemID.ObsidianRose,
    ItemID.LavaCharm];

    public static int MPSyncWorkaround = 0;

    public static void DropLoot(Vector2 position)
    {
        // for some reason this method is being called precisely four times on the server every time my packet gets sent
        // it's weird and idk why it's happening, but it's consistently four times, so unless this bugs out for someone,
        // or i find a better fix, this is what we get right now
        // thanks obama
        MPSyncWorkaround++;
        if (MPSyncWorkaround % 4 == 0)
        {
            for (int i = 0; i < Main.rand.Next(3, 5); i++)
            {
                int randomSelection = Main.rand.Next(PossibleLesserItems.Count);
                int item = Item.NewItem(new EntitySource_Misc("Hell Pod loot"), new Rectangle((int)position.X - 10, (int)position.Y - 10, 20, 20), new Item(
                   PossibleLesserItems[randomSelection].ItemID, Main.rand.Next(PossibleLesserItems[randomSelection].MinStack, PossibleLesserItems[randomSelection].MaxStack)), true);
                Main.item[item].GetGlobalItem<HellPodGlobalItem>().ShouldHover = true;

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncItem, number: item);
                }
            }
            if (Main.rand.Next(100) >= 50)
            {
                int randomSelection = Main.rand.Next(PossibleGreaterItems.Count);
                int item = Item.NewItem(new EntitySource_Misc("Hell Pod loot"), new Rectangle((int)position.X - 10, (int)position.Y - 10, 20, 20), new Item(
                   PossibleGreaterItems[randomSelection], 1), true);
                Main.item[item].velocity = Vector2.Zero;
                Main.item[item].GetGlobalItem<HellPodGlobalItem>().Rare = true;
                Main.item[item].GetGlobalItem<HellPodGlobalItem>().ShouldHover = true;
                Main.item[item].GetGlobalItem<HellPodGlobalItem>().SizeTimer = 60;

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncItem, number: item);
                }
            }
        }
    }

    public static void DestroyAt(int x, int y)
    {
        float rot = 0f;

        if (TryGet(x, y, out HellPodTileEntity entity))
        {
            rot = entity.Rotation;
        }

        Vector2 CenterPosition = (new Vector2(x, y) * 16) + new Vector2(24, 24);

        if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
        {
            DropLoot(CenterPosition);
        }

        if (Main.netMode != NetmodeID.Server)
        {
            HellPod.DoThingALot(x, y, (ii, jj, num) =>
            {
                new HellPodStalkDebris(new Vector2(ii, jj), rot)
                {
                    FrameNum = HellPod.StalkFrame(num).TopLeft() / 18f,
                    FrameDelay = Math.Abs(num / 2)
                }
                .Spawn();
            });

            new HellPodPopShockwave(CenterPosition).Spawn();

            for (int i = 0; i < 5; i++)
            {
                new HellPodDebris(CenterPosition, i, rot).Spawn();
            }

            SoundEngine.PlaySound(Assets.Sounds.Tile.HellPodDestroy.Asset with { PitchVariance = 0.1f }, CenterPosition);
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Main.tile[x + i, y + j].ClearTile();
            }
        }
    }

    public override bool IsTileValidForEntity(int x, int y)
    {
        return Main.tile[Position].TileType == ModContent.TileType<HellPod>();
    }
    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Rotation);
        writer.Write(HurtIntensity);
    }
    public override void NetReceive(BinaryReader reader)
    {
        Rotation = reader.ReadSingle();
        HurtIntensity = reader.ReadSingle();
    }
    public float Rotation;
    public override void SaveData(TagCompound tag)
    {
        tag.Set("Rotation", Rotation);
        base.SaveData(tag);
    }
    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);
        float rot = tag.Get<float>("Rotation");
        if (rot != 0f)
        {
            Rotation = rot;
            Placed = true;
        }
    }
    int NetTimer = 0;
    int DistanceTop = 0;
    int DistanceBottom = 0;
    bool DistanceYetCalculated = false;
    public int Variant;
    public override void Update()
    {
        if (!Placed)
        {
            Variant = Main.rand.Next(3);
            Rotation = MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
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
        if (DistanceYetCalculated)
        {
            referenceTopDistance = DistanceTop;
            referenceBottomDistance = DistanceBottom;
        }

        // distance calculator
        HellPod.DoThingALot(i, j, (ii, jj, num) =>
        {
            if (jj < p2.Y) topDistance = (int)Math.Abs(jj - p2.Y);
            else bottomDistance = (int)Math.Abs(jj - p2.Y);
        });

        // save the distance between the center and either end
        // this is so that we can check if it's changed
        // if it HAS changed, then we kill the pod early because that means the player has broken one of the tiles holding it up
        DistanceTop = topDistance;
        DistanceBottom = bottomDistance;

        // if the distance was calculated last frame, these values will both be more than 0, therefore we do the actual comparison and act accordingly
        if (referenceTopDistance != 0 && referenceBottomDistance != 0)
        {
            // if either distance is different, just damage the pod 3 times so it breaks and runs all related behavior
            if (referenceTopDistance != topDistance || referenceBottomDistance != bottomDistance)
            {
                for (int x = 0; x < 3; x++)
                {
                    HellPod.DamagePod(i, j);
                }
            }
        }

        DistanceYetCalculated = true;
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
        AddMapEntry(new Color(250, 106, 10));
        MineResist = 100;
        DustType = -1;
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
    public static Rectangle StalkFrame(int determinor)
    {
        float rnd1 = (float)(Math.Sin((float)(determinor * 13.12436f)) * 10);
        float rnd2 = (float)(Math.Sin((float)(determinor * 11.24846f)) * 10);

        rnd1 = (float)Math.Floor(rnd1);
        rnd2 = (float)Math.Floor(rnd2);

        return Assets.Textures.Hell.HellPodStalk.Asset.Frame(2, 2, (int)Math.Abs(rnd1 % 2), (int)Math.Abs(rnd2 % 2));
    }
    public static Color BacklightColor => Color.OrangeRed.MultiplyRGBA(new(0.2f, 0.2f, 0.2f, 0f));
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Lighting.AddLight(i, j, TorchID.Torch, 0.2f);

        if (Main.tile[i, j].TileFrameX % 54 == 0 && Main.tile[i, j].TileFrameY % 54 == 0)
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
    }
    public static void DamagePod(int i, int j)
    {
        float frame = (float)Math.Floor((float)Main.tile[i, j].TileFrameY / (18 * 3));
        SoundEngine.PlaySound(frame == 0 ? Assets.Sounds.Tile.HellPodCrack.Asset : Assets.Sounds.Tile.HellPodHeavyCrack.Asset, new Vector2(i * 16, j * 16));

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

        if (TileEntity.TryGet(ii, jj, out HellPodTileEntity entity))
            entity.HurtIntensity = 1f;

        if (Main.tile[ii, jj].TileFrameY > 18 * 3 * 2)
        {
            HellPodTileEntity.DestroyAt(ii, jj);
        }
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        return true;
    }
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        noBreak = true;

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        float rot = 0f;

        float intensity = 0f;

        int var = 0;

        if (TileEntity.TryGet(i, j, out HellPodTileEntity entity))
        {
            entity.HurtIntensity *= 0.75f;
            intensity = entity.HurtIntensity;
            var = entity.Variant;
            rot = entity.Rotation;
        }

        var Placed = Assets.Textures.Hell.HellPodPlaced.Asset;
        var Glow = Assets.Textures.Hell.HellPodGlow.Asset;
        var BrightGlow = Assets.Textures.Hell.HellPodBrightGlow.Asset;
        var Light = Assets.Textures.Hell.HellPodLight.Asset;

        Tile tile = Main.tile[i, j];

        Vector2 fr = new Vector2((float)Math.Floor(tile.TileFrameX / 54f), (float)Math.Floor(tile.TileFrameY / 54f));

        Vector2 p = new Vector2(i * 16, j * 16) + new Vector2(0, -fr.Y).RotatedBy(rot) + new Vector2(0, 2);
        Vector2 p2 = new Vector2(i * 16, j * 16) + new Vector2(0, 2);

        spriteBatch.Draw(Light.Value, p2 - Main.screenPosition + new Vector2(24), Light.Frame(), Color.Black.MultiplyRGBA(new(1f, 1f, 1f, 0.5f)), GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.4f, SpriteEffects.None, 0);
        spriteBatch.Draw(Light.Value, p2 - Main.screenPosition + new Vector2(24), Light.Frame(), BacklightColor, GlobalTimer.Value / 300f, Light.Frame().Size() / 2f, 0.2f, SpriteEffects.None, 0);

        Rectangle frame = Placed.Frame(2, 3, var % 2, (int)fr.Y);

        Vector2 origin = new Vector2(24f, 24f).RotatedBy(-rot);

        Vector2 bottomPosition = Vector2.Zero;
        Vector2 topPosition = Vector2.Zero;

        if (Main.tile[i, j].TileFrameX % (18 * 3) < 8 && Main.tile[i, j].TileFrameY % (18 * 3) < 8)
        {
            var Asset = Assets.Textures.Hell.HellPodStalk.Asset;

            // draw function
            DoThingALot(i, j, (ii, jj, num) =>
            {
                Rectangle frame = StalkFrame(num);
                spriteBatch.Draw(Asset.Value, new Vector2(ii, jj) - Main.screenPosition, frame, Lighting.GetColor((new Vector2(ii + 8, jj + 8) / 16).ToPoint()), rot, new Vector2(8f), 1f, SpriteEffects.None, 0);

                if (jj < p2.Y) topPosition = new(ii, jj);
                else bottomPosition = new(ii, jj);
            });
        }

        var BaseAsset = Assets.Textures.Hell.HellPodBase.Asset;

        var baseFrame = BaseAsset.Frame(3, 1, var);
        var baseFrame2 = BaseAsset.Frame(3, 1, (var + 1) % 3);

        spriteBatch.Draw(BaseAsset.Value, topPosition - Main.screenPosition, baseFrame, Lighting.GetColor((topPosition / 16).ToPoint()), rot + MathHelper.Pi, new Vector2(baseFrame.Size().X / 2, baseFrame.Size().Y - 8), 1f, SpriteEffects.None, 0);
        spriteBatch.Draw(BaseAsset.Value, bottomPosition - Main.screenPosition, baseFrame2, Lighting.GetColor((bottomPosition / 16).ToPoint()), rot, new Vector2(baseFrame.Size().X / 2, baseFrame.Size().Y - 8), 1f, SpriteEffects.None, 0);

        spriteBatch.Draw(Placed.Value, p - Main.screenPosition + new Vector2(24), frame, Lighting.GetColor((p / 16).ToPoint()), rot, origin, 1f, SpriteEffects.None, 0);
        spriteBatch.Draw(Glow.Value, p - Main.screenPosition + new Vector2(24), frame, Color.White, rot, origin, 1f, SpriteEffects.None, 0);
        spriteBatch.Draw(Glow.Value, p - Main.screenPosition + new Vector2(24), frame, new Color(intensity, intensity, intensity, 0f), rot, origin, 1f, SpriteEffects.None, 0);
        if (intensity > 0.3f)
            spriteBatch.Draw(BrightGlow.Value, p - Main.screenPosition + new Vector2(24), frame, Color.White, rot, origin, 1f, SpriteEffects.None, 0);
    }
    public delegate void ThingToDo(int i, int j, int num);

    /// <summary>
    /// This performs an action in a rotated column protruding outwards from this hell pod
    /// </summary>
    /// <param name="i">Center X, in tile coordinates</param>
    /// <param name="j">Center Y, in tile coordinates</param>
    /// <param name="action">The thing to do at each set of world coordinates (NOT tile coordinates because this needs more precision)</param>
    public static void DoThingALot(int i, int j, ThingToDo action)
    {
        if (TileEntity.TryGet(i, j, out HellPodTileEntity entity))
        {
            float rot = entity.Rotation;

            Vector2 position = new Vector2(i * 16, j * 16);

            int xx = 16;
            int ii = 2;
            int jj = 32;
            int iterations = 0;

            Vector2 p1 = (new Vector2(i * 16, j * 16) + new Vector2(8f)) + new Vector2(16, 3).RotatedBy(rot);
            Vector2 p2 = (new Vector2(i * 16, j * 16) + new Vector2(8f)) + new Vector2(16, 32).RotatedBy(rot);

            while (!WorldGen.SolidOrSlopedTile(Main.tile[(p1 / 16).ToPoint()]))
            {
                iterations--;
                if (iterations < -70) break;

                p1 += new Vector2(0, -16).RotatedBy(rot);

                action((int)p1.X, (int)p1.Y, iterations);
            }
            iterations = 0;
            while (!WorldGen.SolidOrSlopedTile(Main.tile[(p2 / 16).ToPoint()]))
            {
                iterations++;
                if (iterations > 70) break;

                p2 += new Vector2(0, 16).RotatedBy(rot);

                action((int)p2.X, (int)p2.Y, iterations);
            }
        }
    }
}

public class HellPodGlobalProjectile : GlobalProjectile
{
    public override void Load()
    {
        EverwarePacketHandler.AddPacket(
            (mod, reader, whoAmI, identifier) =>
            {
                if (identifier == "DamageHellPodFromServer")
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        HellPod.DamagePod(x, y);

                        ModPacket p = Everware.Instance.GetPacket();
                        p.Write("DamageHellPod");
                        p.Write(x);
                        p.Write(y);
                        p.Send();
                    }
                }
                if (identifier == "DamageHellPod")
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();

                        HellPod.DamagePod(x, y);
                    }
                }
            }
        );
    }
    public override bool InstancePerEntity => true;
    public bool HasHitPod = false;
    public override void PostAI(Projectile projectile)
    {
        if (projectile.friendly && projectile.damage > 0 && !HasHitPod)
        {
            if (Main.tile[(projectile.Center / 16).ToPoint()].TileType == ModContent.TileType<HellPod>() && Main.tile[(projectile.Center / 16).ToPoint()].HasTile)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    HasHitPod = true;
                    projectile.penetrate--;
                    Point p = (projectile.Center / 16).ToPoint();
                    HellPod.DamagePod(p.X, p.Y);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    int closestPlayer = (int)Player.FindClosest(projectile.Center, 2000, 2000);

                    if (closestPlayer >= 0)
                    {
                        if (Main.LocalPlayer.whoAmI == closestPlayer)
                        {
                            HasHitPod = true;
                            projectile.penetrate--;
                            Point p = (projectile.Center / 16).ToPoint();

                            ModPacket packet = Everware.Instance.GetPacket();
                            packet.Write("DamageHellPodFromServer");
                            packet.Write((int)p.X);
                            packet.Write((int)p.Y);
                            packet.Send();
                        }
                    }
                }
            }
        }
    }
}

REMOVED TEMPORARILY */