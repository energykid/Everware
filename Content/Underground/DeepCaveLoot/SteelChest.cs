using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class SteelChestTile : ChestTemplate
{
    protected override bool CanBeLocked => true;
    public override string Texture => "Everware/Assets/Textures/Underground/SteelChestTile";
    public override string HighlightTexture => "Everware/Assets/Textures/Underground/SteelChestTile_Outline";
    public override bool CanBeUnlockedNormally => true;
    protected override int ChestKeyItemId => ItemID.GoldenKey;
}
