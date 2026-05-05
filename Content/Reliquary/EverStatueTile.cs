using Everware.Content.Base.Tiles;
using Everware.Content.Hell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ObjectData;

namespace Everware.Content.Reliquary;

public abstract class EverStatueTile : EverMultitile
{
    public override int Width => 2;
    public override int Height => 3;
    public virtual int MaxCooldown => 60;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        AddMapEntry(new Color(155, 155, 155));
        DustType = DustID.Stone;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<EverStatueTileEntity>().Generic_HookPostPlaceMyPlayer;

        TileObjectData.newTile.UsesCustomCanPlace = true;
    }
    public override void HitWire(int i, int j)
    {
        if (TileEntity.TryGet(i, j, out EverStatueTileEntity te))
        {
            if (te.Cooldown <= 0)
            {
                Point pos = new(i, j);
                OnPulse(pos);
                te.Cooldown = MaxCooldown;
            }
        }

        base.HitWire(i, j);
    }
    public override void PlaceInWorld(int i, int j, Item item)
    {
        base.PlaceInWorld(i, j, item);
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (TileEntity.TryGet(i, j, out EverStatueTileEntity te))
        {
            te.Kill(i, j);
        }
    }
    public virtual void OnPulse(Point pos)
    {

    }
}

public class EverStatueTileEntity : ModTileEntity
{
    public int Cooldown = 0;

    public override bool IsTileValidForEntity(int x, int y)
    {
        return true;
    }

    public override void PostGlobalUpdate()
    {
        Main.NewText("a");
        Cooldown--;
    }
}