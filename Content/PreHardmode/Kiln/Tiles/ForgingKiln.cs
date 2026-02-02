using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Content.PreHardmode.Kiln.Visual;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class ForgingKiln : EverMultitile
{
    public static readonly string POIName = "Forging Kiln";

    public override int Width => 5;
    public override int Height => 5;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        AdjTiles = [TileID.Furnaces];

        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(151, 62, 59));
        style.PitchRange = (-0.3f, -0.1f);
        HitSound = style;
        Main.tileNoAttach[Type] = true;
        Main.tileFrameImportant[Type] = true;
    }

    public static readonly Asset<Texture2D> FireGlow = ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Kiln/Tiles/ForgingKilnGlow");

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0)
        {
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
        }
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Lighting.AddLight(i, j, 0.7f, 0.5f, 0.2f);

        float ReferenceValue = GlobalTimer.Value * 0.05f;

        float alph1 = 1f;
        alph1 = Easing.KeyFloatPersistent(ReferenceValue % 2, 0f, 1f, 0f, 1f, Easing.Linear).GetValueOrDefault(alph1);
        alph1 = Easing.KeyFloatPersistent(ReferenceValue % 2, 1f, 2f, 1f, 0f, Easing.Linear).GetValueOrDefault(alph1);

        float alph2 = 1f;
        alph2 = Easing.KeyFloatPersistent(ReferenceValue % 2, 0f, 1f, 1f, 0f, Easing.Linear).GetValueOrDefault(alph2);
        alph2 = Easing.KeyFloatPersistent(ReferenceValue % 2, 1f, 2f, 0f, 1f, Easing.Linear).GetValueOrDefault(alph2);

        Vector2 pos = new Vector2(i * 16, j * 16);
        spriteBatch.Draw(FireGlow.Value, pos - Main.screenPosition, FireGlow.Frame(2, 1, 0), Color.White.MultiplyRGBA(new(alph1, alph1, alph1, alph1 * 0.5f)), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        spriteBatch.Draw(FireGlow.Value, pos - Main.screenPosition, FireGlow.Frame(2, 1, 1), Color.White.MultiplyRGBA(new(alph2, alph2, alph2, alph2 * 0.5f)), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        Lighting.AddLight(i, j, 2, 2, 1);
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        SoundEngine.PlaySound(new SoundStyle("Everware/Sounds/Tile/ForgingKilnDestroy").WithPitchOffset(0.2f), new Vector2((i * 16) + 32, (j * 16) + 32));
        base.KillMultiTile(i, j, frameX, frameY);
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        new ForgingKilnSmokePillar(new Vector2(i * 16, j * 16)).Spawn();
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        return [new Item(ModContent.ItemType<ForgingKilnItem>())];
    }
}