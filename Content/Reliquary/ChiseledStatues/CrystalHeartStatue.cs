using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

public class CrystalHeartStatueItem : EverStatueItem
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/CrystalHeartStatueItem";
    public override int PlacementID => ModContent.TileType<CrystalHeartStatue>();
    public override int BaseStatue => ItemID.HeartStatue;
}
public class CrystalHeartStatue : EverStatueTile
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/CrystalHeartStatue";
    public override void OnPulse(Point pos)
    {
        Main.NewText("BOOM!!!!!!!!!");
    }
}
