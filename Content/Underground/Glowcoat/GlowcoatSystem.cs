using Everware.Utils;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Underground.Glowcoat;

public class GlowcoatGlobalTile : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail && !effectOnly)
        {
            GlowcoatSystem.Unglowcoat(i, j);
        }
    }
}
public class GlowcoatPaintScraper : GlobalItem
{
    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.type == ItemID.PaintScraper || item.type == ItemID.SpectrePaintScraper && player.ItemAnimationJustStarted)
        {
            bool inRange = Math.Abs(Player.tileTargetX - (player.Center.X / 16)) < Player.tileRangeX && Math.Abs(Player.tileTargetY - (player.Center.Y / 16)) < Player.tileRangeY;
            Tile t = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (t.HasTile && Main.tileSolid[t.TileType] && inRange)
            {
                if (t.Get<GlowcoatTileData>().color != Color.Transparent)
                {
                    GlowcoatSystem.Unglowcoat(Player.tileTargetX, Player.tileTargetY);
                }
            }
        }
    }
}

public class GlowcoatSystem : ModSystem
{
    public static void Unglowcoat(int i, int j)
    {
        Main.tile[i, j].Get<GlowcoatTileData>().color = Color.Transparent;
        Main.tile[i, j].Get<GlowcoatTileData>().chromatic = false;

        if (GlowcoatedTiles.Contains(new Point(i, j)))
            GlowcoatedTiles.Remove(new Point(i, j));
    }
    public static void Glowcoat(int i, int j, Color color, bool chromatic = false)
    {
        Unglowcoat(i, j);

        Main.tile[i, j].Get<GlowcoatTileData>().color = color;
        Main.tile[i, j].Get<GlowcoatTileData>().chromatic = chromatic;

        GlowcoatedTiles.Add(new Point(i, j));
    }
    public override void Load()
    {
        On_Main.DrawTiles += On_Main_DrawTiles;
    }

    private void On_Main_DrawTiles(On_Main.orig_DrawTiles orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
    {
        if (!solidLayer)
        {
            var GlowEffect = Assets.Effects.Underground.GlowcoatColoration.CreateEffect();
            GlowEffect.Parameters.Color = Color.Blue.ToVector4();
            GlowEffect.Apply();

            for (int i = -10; i < (Main.screenWidth / 16) + 10; i++)
            {
                for (int j = -10; j < (Main.screenHeight / 16) + 10; j++)
                {
                    Point a = (Main.screenPosition / 16).ToPoint();
                    a.X += i; a.Y += j;
                    Tile t = Main.tile[a];

                    if (t.HasTile)
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, null, null, GlowEffect.Shader, Main.GameViewMatrix.EffectMatrix);

                        Color c = t.Get<GlowcoatTileData>().color;
                        if (c != Color.Transparent)
                        {
                            GlowEffect.Parameters.Color = c.ToVector4();
                            if (t.Get<GlowcoatTileData>().chromatic)
                                GlowEffect.Parameters.Color = new Vector4(Main.DiscoR, Main.DiscoG, Main.DiscoB, 255) / 255f;
                            GlowEffect.Apply();

                            Lighting.AddLight(new Vector2(a.X * 16, a.Y * 16), c.ToVector3() * 0.25f);

                            for (float k = 0; k < 360; k += 90)
                                Main.instance.TilesRenderer.DrawSingleTile(new(), true, 0, Main.screenPosition, DrawingUtils.TileOffset() + new Vector2(2, 0).RotatedBy(MathHelper.ToRadians(k)), a.X, a.Y);
                        }
                    }
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null);
        }

        orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
    }

    public override void Unload()
    {
        On_Main.DrawTiles -= On_Main_DrawTiles;
    }

    public static string PointString(Point p)
    {
        return p.X.ToString() + "," + p.Y.ToString();
    }
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Set("Count", GlowcoatedTiles.Count);
        for (int i = 0; i < GlowcoatedTiles.Count; i++)
        {
            Tile t = Main.tile[GlowcoatedTiles[i]];
            tag.Set(i.ToString(), GlowcoatedTiles[i].ToVector2());
            tag.Set("Color_" + PointString(GlowcoatedTiles[i]), t.Get<GlowcoatTileData>().color);
            tag.Set("Chroma_" + PointString(GlowcoatedTiles[i]), t.Get<GlowcoatTileData>().chromatic);
        }
        GlowcoatedTiles.Clear();
    }
    public override void LoadWorldData(TagCompound tag)
    {
        GlowcoatedTiles.Clear();
        for (int i = 0; i < tag.Get<int>("Count"); i++)
        {
            Point p = tag.Get<Vector2>(i.ToString()).ToPoint();
            Tile t = Main.tile[p];
            GlowcoatedTiles.Add(p);
            t.Get<GlowcoatTileData>().color = tag.Get<Color>("Color_" + PointString(p));
            t.Get<GlowcoatTileData>().chromatic = tag.Get<bool>("Chroma_" + PointString(p));
        }
    }

    public static List<Point> GlowcoatedTiles = [];
}

public struct GlowcoatTileData : ITileData
{
    public Color color = Color.Transparent;
    public bool chromatic = false;

    public GlowcoatTileData(Color c)
    {
        color = c;
        chromatic = false;
    }
}