using Everware.Common;
using Everware.Content.Base.Items;
using Everware.Utils;
using System.IO;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

[AutoloadEquip(EquipType.Face)]
public class AtlasCrown : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/AtlasCrown";
    public override int Rarity => 6;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToAccessory(38, 34);
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<AtlasCrownEffects>().Active = true;
    }
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ChiselablesList.AllChiselables.Add(new(ItemID.KingStatue, Type, ItemID.SoulofMight, 10));
    }
}

public class AtlasCrownHeldItemPacket : EverPacket
{
    public int ItemSpeed = 0;
    public int ItemDamage = 0;
    public int Player = 0;
    public bool Cooldown = false;
    public override void Read(Mod mod, BinaryReader reader, int playerID)
    {
        Player = reader.ReadInt32();
        ItemSpeed = reader.ReadInt32();
        ItemDamage = reader.ReadInt32();
        Cooldown = reader.ReadBoolean();

        AtlasCrownEffects plr = Main.player[Player].GetModPlayer<AtlasCrownEffects>();

        plr.HeldItemSpeed = ItemSpeed;
        plr.HeldItemDamage = ItemDamage;
        plr.Cooldown = Cooldown;

        if (Main.netMode == NetmodeID.Server) EverwarePacketHandler.SendPacket(this);
    }
    public override void Write(ModPacket packet)
    {
        packet.Write(Player);
        packet.Write(ItemSpeed);
        packet.Write(ItemDamage);
        packet.Write(Cooldown);
    }
}

public class AtlasCrownEffects : ModPlayer
{
    public bool Active = false;
    public bool Cooldown = false;

    int Timer = 0;
    public int HeldItemSpeed = 0;
    public int HeldItemDamage = 0;

    public override void ResetEffects()
    {
        Active = false;
    }
    public override void PostUpdate()
    {
        if (Cooldown)
        {
            Cooldown = Player.ownedProjectileCounts[ModContent.ProjectileType<TileCluster>()] != 0;
            return;
        }

        if (Active && Player.HeldItem.DamageType != DamageClass.Default
            && Player.HeldItem.pick == 0
            && Player.HeldItem.axe == 0
            && Player.HeldItem.hammer == 0)
        {
            if (Player.HeldItem.useAnimation != HeldItemSpeed || Player.HeldItem.OriginalDamage != HeldItemDamage)
            {
                Timer = 1;
                RemovePreviousTileClusters();

                HeldItemDamage = Player.HeldItem.OriginalDamage;
                HeldItemSpeed = Player.HeldItem.useAnimation;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    EverwarePacketHandler.SendPacket(new AtlasCrownHeldItemPacket() { Player = Player.whoAmI, ItemSpeed = HeldItemSpeed, ItemDamage = HeldItemDamage });
            }
            if (Player.controlUp && !Cooldown)
            {
                if (Timer % (Math.Max(HeldItemSpeed, 2)) == 0 && !Player.ItemAnimationActive)
                {
                    int MaxClusters = (int)MathHelper.Lerp(10f, 3f, (float)HeldItemSpeed / 60f);
                    int ClusterSize = 1 + (int)Math.Ceiling((float)HeldItemSpeed / 12f);

                    if (Player.ownedProjectileCounts[ModContent.ProjectileType<TileCluster>()] < MaxClusters && Main.myPlayer == Player.whoAmI)
                    {
                        Vector2 pos = (Player.Center + new Vector2(Main.rand.NextFloat(-250, 250), 0)).Grounded() + new Vector2(0, 20);
                        int proj = Projectile.NewProjectile(new EntitySource_Parent(Player, "Atlas' Crown cluster"), pos, new Vector2((pos.X - Player.Center.X) / 20f, -25), ModContent.ProjectileType<TileCluster>(),
                            Player.HeldItem.OriginalDamage, 5f, Player.whoAmI, (int)pos.X / 16, (int)pos.Y / 16, ClusterSize + Main.rand.NextFloat(1.5f));

                        if (Main.projectile[proj].ModProjectile is TileCluster cluster)
                        {
                            cluster.MaxClusterIndex = MaxClusters;
                            cluster.ClusterIndex = Player.ownedProjectileCounts[ModContent.ProjectileType<TileCluster>()];
                        }
                    }
                }
            }
            if (Player.controlUp || Timer % (Math.Max(HeldItemSpeed, 2)) != 0)
            {
                Timer++;
            }
        }
        else
        {
            if (Player.ownedProjectileCounts[ModContent.ProjectileType<TileCluster>()] > 0) RemovePreviousTileClusters();
        }
    }
    public void RemovePreviousTileClusters()
    {
        for (int i = 0; i < Main.projectile.Length; i++)
        {
            if (Main.projectile[i].type == ModContent.ProjectileType<TileCluster>() && Main.projectile[i].owner == Player.whoAmI)
            {
                if (Main.projectile[i].ModProjectile is TileCluster cluster)
                {
                    Main.projectile[i].velocity.Y = -3;
                    cluster.Fall = true;
                    Main.projectile[i].owner = Main.player.Length - 1;
                }
            }
        }
    }
}