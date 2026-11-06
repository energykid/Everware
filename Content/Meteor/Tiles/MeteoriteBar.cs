using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class MeteoriteBar : MetalBarTile
{
    public override Color Color => new(255, 125, 50);
    public override string Texture =>  "Everware/Assets/Textures/Meteor/Items/MeteoriteBarPlaced";
}