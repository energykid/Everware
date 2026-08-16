using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
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
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ChiselablesList.AllChiselables.Add(new(ItemID.KingStatue, Type, ItemID.SoulofMight, 10));
    }
}

public class AtlasCrownTileCluster : EverProjectile
{

}