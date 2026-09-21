using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Misc.Tiles;

public class LivingFallenStarTile : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/LivingFallenStarTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.YellowStarDust;
        AddMapEntry(new Color(255, 255, 51));

    }

}