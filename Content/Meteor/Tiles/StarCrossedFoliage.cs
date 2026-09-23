using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class StarCrossedFoliage : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/StarCrossedFoliage";

    public override void SetStaticDefaults()
    {
        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;

        DustType = DustID.YellowStarDust;

        HitSound = SoundID.Grass;

        AddMapEntry(new Color(135, 150, 174));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        return Main.tile[i, j + 1].TileType == ModContent.TileType<StarCrossedGrassTile>() ? false : base.PreDraw(i, j, spriteBatch);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        height = 20;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 0)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }
}

public class LargeStarCrossedFoliage : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/LargeStarCrossedFoliage";

    public override void SetStaticDefaults()
    {
        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;

        DustType = DustID.YellowStarDust;

        HitSound = SoundID.Grass;

        AddMapEntry(new Color(135, 150, 174));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        return Main.tile[i, j + 1].TileType == ModContent.TileType<StarCrossedGrassTile>() ? false : base.PreDraw(i, j, spriteBatch);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        height = 20;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 0)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }
}
