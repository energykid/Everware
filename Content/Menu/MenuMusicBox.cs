using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Kiln.Tiles;
using Everware.Content.Quarry.Tiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Everware.Content.Menu;

public class MenuMusicBox : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Menu/MenuMusicBox";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.CanGetPrefixes[Type] = false; // music boxes can't get prefixes in vanilla
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox; // recorded music boxes transform into the basic form in shimmer

        // The following code links the music box's item and tile with a music track:
        //   When music with the given ID is playing, equipped music boxes have a chance to change their id to the given item type.
        //   When an item with the given item type is equipped, it will play the music that has musicSlot as its ID.
        //   When a tile with the given type and Y-frame is nearby, if its X-frame is >= 36, it will play the music that has musicSlot as its ID.
        // When getting the music slot, you should not add the file extensions!
        MusicLoader.AddMusicBox(Mod, Assets.Sounds.Music.SomewhereElse.Slot, ModContent.ItemType<MenuMusicBox>(), ModContent.TileType<MenuMusicBoxPlaced>());
    }

    public override void SetDefaults()
    {
        Item.DefaultToMusicBox(ModContent.TileType<MenuMusicBoxPlaced>(), 0);
    }

    public override void AddRecipes()
    {
        Recipe recipe = Recipe.Create(Type);
        recipe.AddIngredient<KilnMusicBox>();
        recipe.AddIngredient<QuarryMusicBox>();
        recipe.AddTile(TileID.TinkerersWorkbench);
        recipe.Register();
    }
}

public class MenuMusicBoxPlaced : ModTile
{
    public override string Texture => "Everware/Assets/Textures/Menu/MenuMusicBoxPlaced";
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleLineSkip = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.MusicBox"));
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<MenuMusicBox>();
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public static int Timer = 0;

    public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        // This code spawns the music notes when the music box is open.
        Tile tile = Main.tile[i, j];

        if (!visible || tile.TileFrameX != 36 || tile.TileFrameY % 36 != 0 || (int)Main.timeForVisualEffects % 7 != 0 || !Main.rand.NextBool(3))
        {
            return;
        }

        int MusicNote = Main.rand.Next(570, 573);
        Vector2 SpawnPosition = new Vector2(i * 16 + 8, j * 16 - 8);
        Vector2 NoteMovement = new Vector2(Main.WindForVisuals * 2f, -0.5f);
        NoteMovement.X *= Main.rand.NextFloat(0.5f, 1.5f);
        NoteMovement.Y *= Main.rand.NextFloat(0.5f, 1.5f);
        switch (MusicNote)
        {
            case 572:
                SpawnPosition.X -= 8f;
                break;
            case 571:
                SpawnPosition.X -= 4f;
                break;
        }

        Gore.NewGore(new EntitySource_TileUpdate(i, j), SpawnPosition, NoteMovement, MusicNote, 0.8f);
    }
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Tile t = Main.tile[i, j];
        if (t.TileFrameX == 0 || t.TileFrameX == 36)
        {
            if (t.TileFrameY == 0)
            {
                Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
            }
        }
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        return true;
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Color lightColor = Lighting.GetColor(i + 1, j + 1);

        Tile t = Main.tile[i, j];

        int v = (int)(GlobalTimer.Value / 5);

        Rectangle frame = Assets.Textures.Menu.MenuMusicBoxPlaced_Animated.Asset.Frame(8);
        if (t.TileFrameX == 36) frame = Assets.Textures.Menu.MenuMusicBoxPlaced_Animated.Asset.Frame(8, 1, 1 + (v % 6));

        spriteBatch.Draw(Assets.Textures.Menu.MenuMusicBoxPlaced_Animated.Asset.Value, new Vector2((float)i * 16f, (float)j * 16f) - Main.screenPosition + new Vector2(36 / 2, 36 / 2) + new Vector2(-3, 0), frame, lightColor, 0f, frame.Size() / 2, 1f, SpriteEffects.None, 0f);
    }
}