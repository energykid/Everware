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
        Point pos = new(i, j);
        while (Main.tile[pos].TileFrameX != 0)
        {
            pos.X--;
        }
        while (Main.tile[pos].TileFrameY != 0)
        {
            pos.Y--;
        }

        for (int k = 0; k < Width; k++)
        {
            for (int l = 0; l < Height; l++)
            {
                Wiring.SkipWire(pos.X + k, pos.Y + l);
            }
        }

        if (TileEntity.TryGet(i, j, out EverStatueTileEntity te))
        {
            if (te.Cooldown <= 0)
            {
                te.Cooldown = MaxCooldown;
                OnPulse(pos);
            }
        }

        base.HitWire(i, j);
    }
    public override void PlaceInWorld(int i, int j, Item item)
    {
        Point pos = new Point(i, j);

        while (Main.tile[pos].TileFrameX != 0)
        {
            pos.X--;
        }
        while (Main.tile[pos].TileFrameY != 0)
        {
            pos.Y--;
        }

        TileEntity.PlaceEntityNet(pos.X, pos.Y, ModContent.TileEntityType<EverStatueTileEntity>());
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        Main.NewText(new Point(i, j));
        if (TileEntity.TryGet(i, j, out EverStatueTileEntity te))
        {
            te.Kill(i, j);
        }
    }
    public virtual void OnPulse(Point pos)
    {

    }
    public Rectangle GetRect(Point pos)
    {
        return new Rectangle(pos.X * 16, pos.Y * 16, Width * 16, Height * 16);
    }
}

public class EverStatueTileEntity : ModTileEntity
{
    public int Cooldown = 0;

    public override bool IsTileValidForEntity(int x, int y)
    {
        return ModContent.GetModTile(Main.tile[x, y].TileType) is EverStatueTile && Main.tile[x, y].HasTile;
    }

    public override void PostGlobalUpdate()
    {
    }

    public override void Update()
    {
        Cooldown--;
    }
}