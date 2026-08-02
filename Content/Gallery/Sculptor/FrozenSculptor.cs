using Everware.Content.Base.Tiles;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;

namespace Everware.Content.Gallery.Sculptor;

public class FrozenSculptor : EverMultitile
{
    public override string Texture => "Everware/Assets/Textures/Gallery/Sculptor/SculptorNPC";
    public override int Width => 4;
    public override int Height => 4;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        AddMapEntry(new Color(126, 194, 217));
        DustType = DustID.Ice;
        Main.tileNoAttach[Type] = true;

        MinPick = 100000000;

        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<FrozenSculptorTileEntity>().Generic_HookPostPlaceMyPlayer;

        TileObjectData.newTile.UsesCustomCanPlace = true;
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tex = Assets.Textures.Gallery.Sculptor.FrozenSculptor.Asset;

        Vector2 offset = Vector2.Zero;
        offset.Y = 4;
        Rectangle fr = tex.Frame(verticalFrames: 4, frameY: 0);

        if (TileEntity.TryGet(i, j, out FrozenSculptorTileEntity tE))
        {
            offset.X = Main.rand.NextFloat(-tE.DamageAnim, tE.DamageAnim);
            fr = tex.Frame(verticalFrames: 4, frameY: tE.DamageStage);
        }

        spriteBatch.Draw(tex.Value, (new Vector2(i + 2, j + 4) * 16f) + offset - Main.screenPosition, fr, Lighting.GetColor(new Point(i + 2, j + 3)), 0f, new Vector2(fr.Size().X / 2, fr.Size().Y), 1f, SpriteEffects.None, 0f);
    }
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
        {
            if (!TileEntity.TryGet(i, j, out FrozenSculptorTileEntity tE))
            {
                TileEntity.PlaceEntityNet(i, j, ModContent.TileEntityType<FrozenSculptorTileEntity>());
            }

            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }
        return false;
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
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
        if (TileEntity.TryGet(pos.X, pos.Y, out FrozenSculptorTileEntity tE))
        {
            tE.DamageAnim = 3f;
            tE.DamageStage++;

            SoundEngine.PlaySound(Assets.Sounds.Tile.FrozenSculptor_Crack.Asset, new Vector2(i + 2, j + 3) * 16);

            if (tE.DamageStage > 3)
            {
                fail = false;
                effectOnly = false;
            }
        }
    }
    public override void KillMultiTile(int i, int j, int frameX, int frameY)
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

        SoundEngine.PlaySound(Assets.Sounds.Tile.Sculptor_Free.Asset, new Vector2(i + 2, j + 3) * 16);

        SculptorTownNPCArrivalSystem.SculptorAvailable = true;

        Vector2 p = new Vector2(i + 2, j + 3) * 16;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.NewNPC(new EntitySource_Misc("Sculptor Spawned"), (int)p.X, (int)p.Y, ModContent.NPCType<SculptorNPC>());
    }
}
public class FrozenSculptorTileEntity : ModTileEntity
{
    public override bool IsTileValidForEntity(int x, int y)
    {
        return Main.tile[x, y].TileType == ModContent.TileType<FrozenSculptor>() && Main.tile[x, y].HasTile;
    }
    public float DamageAnim = 0f;
    public int DamageStage = 0;
    public override void Update()
    {
        DamageAnim *= 0.8f;
    }
}