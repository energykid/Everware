using Everware.Content.Base.Items;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

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

public class AtlasCrownEffects : ModPlayer
{
    public bool Active = false;

    int Timer = 0;
    int HeldItemSpeed = 0;
    int HeldItemDamage = 0;
    public override void ResetEffects()
    {
        Active = false;
    }
    public override void PostUpdate()
    {
        if (Active && Player.HeldItem.DamageType != DamageClass.Default
            && Player.HeldItem.pick != 0
            && Player.HeldItem.axe != 0
            && Player.HeldItem.hammer != 0)
        {
            if (Player.HeldItem.useAnimation != HeldItemSpeed || Player.HeldItem.OriginalDamage != HeldItemDamage)
            {
                Timer = 0;
                RemovePreviousTileClusters();

                HeldItemDamage = Player.HeldItem.OriginalDamage;
                HeldItemSpeed = Player.HeldItem.useAnimation;
            }
            Timer++;
            if (Timer % (Math.Max(HeldItemSpeed, 2)) == 0 && !Player.ItemAnimationActive)
            {
                int MaxClusters = (int)MathHelper.Lerp(10f, 3f, (float)HeldItemSpeed / 60f);
                int ClusterSize = 1 + (int)Math.Ceiling((float)HeldItemSpeed / 12f);

                if (Player.ownedProjectileCounts[ModContent.ProjectileType<TileCluster>()] < MaxClusters)
                {
                    Vector2 pos = (Player.Center + new Vector2(Main.rand.NextFloat(-250, 250), 0)).Grounded() + new Vector2(0, 20);
                    Projectile proj = Projectile.NewProjectileDirect(new EntitySource_Parent(Player, "Atlas' Crown cluster"), pos, new Vector2((pos.X - Player.Center.X) / 20f, -25), ModContent.ProjectileType<TileCluster>(),
                        Player.HeldItem.OriginalDamage, 5f, Player.whoAmI, (int)pos.X / 16, (int)pos.Y / 16, ClusterSize);

                    if (proj.ModProjectile is TileCluster cluster)
                    {
                        cluster.MaxClusterIndex = MaxClusters;
                        cluster.ClusterIndex = Player.ownedProjectileCounts[ModContent.ProjectileType<TileCluster>()];
                    }
                }
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
                    cluster.Fall = true;
                    Main.projectile[i].owner = Main.player.Length - 1;
                }
            }
        }
    }
}