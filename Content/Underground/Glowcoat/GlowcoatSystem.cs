using Everware.Utils;
using System.Collections.Generic;
using System.Linq;
using Everware.Common.Systems;
using Everware.Content.Base;
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
            if (t.HasTile && inRange)
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
    public static List<Color> AllColors = new();
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
            for (int ki = 0; ki < AllColors.Count; ki++)
            {
                Main.spriteBatch.End(out var sb);
                
                var target = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice,
                    Main.instance.tileTarget.Width, Main.instance.tileTarget.Height);

                Color color = AllColors[ki];
                using (target.Scope(clearColor: Color.Transparent))
                {
                    Color cc = color;
                    cc.R = (byte)((float)cc.R * 0.4f);
                    cc.G = (byte)((float)cc.G * 0.7f);
                    var glowEffect = Assets.Effects.Underground.GlowcoatColoration.CreateEffect();
                    glowEffect.Parameters.Color = cc.ToVector4();
                    if (color == new Color(255, 255, 255))
                        glowEffect.Parameters.Color = new Vector4(Main.DiscoR * 0.4f, Main.DiscoG * 0.7f, Main.DiscoB, 255) / 255f;
                    glowEffect.Apply();
                
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, null, glowEffect.Shader);
                
                    for (int i = -10; i < Main.screenWidth / 16 + 10; i++)
                    {
                        for (int j = -10; j < Main.screenHeight / 16 + 10; j++)
                        {
                            Point a = (Main.screenPosition / 16).ToPoint();
                            a.X += i;
                            a.Y += j;
                            Tile t = Main.tile[a];

                            if (t.HasTile)
                            {
                                Color c = t.Get<GlowcoatTileData>().color;
                                if (c.PackedValue == color.PackedValue)
                                {
                                    for (int ii = 0; ii < 4; ii++)
                                    {
                                        Main.instance.TilesRenderer.DrawSingleTile(new(), true, 0, Main.screenPosition,
                                            DrawingUtils.TileOffset() + new Vector2(1, 0).RotatedBy(MathHelper.PiOver2 * ii), a.X, a.Y);
                                    }
                                }
                            }
                        }
                    }
                    Main.spriteBatch.End();
                }

                var blurEffect = Assets.Effects.Misc.Blur.CreateEffect();
                blurEffect.Parameters.Radius = 0.01f + (float)(Math.Sin(GlobalTimer.Value / 40f) * 0.002f);
                blurEffect.Apply();
                
                Main.spriteBatch.Begin(sb with { CustomEffect = blurEffect.Shader, BlendState = BlendState.Additive });
                
                Main.spriteBatch.Draw(target.Target, target.Target.Bounds, new Color(1f, 1f,  1f, 0f));
                
                var glowEffect2 = Assets.Effects.Underground.GlowcoatColoration.CreateEffect();
                glowEffect2.Parameters.Color = color.ToVector4();
                if (color == new Color(255, 255, 255))
                    glowEffect2.Parameters.Color = new Vector4(Main.DiscoR, Main.DiscoG, Main.DiscoB, 255) / 255f;
                glowEffect2.Apply();
                
                Main.spriteBatch.Restart(sb with {CustomEffect = glowEffect2.Shader});
                
                
                for (int i = -10; i < Main.screenWidth / 16 + 10; i++)
                {
                    for (int j = -10; j < Main.screenHeight / 16 + 10; j++)
                    {
                        Point a = (Main.screenPosition / 16).ToPoint();
                        a.X += i;
                        a.Y += j;
                        Tile t = Main.tile[a];

                        if (t.HasTile)
                        {
                            Color c = t.Get<GlowcoatTileData>().color;
                            if (c.PackedValue == color.PackedValue)
                            {
                                for (int ii = 0; ii < 4; ii++)
                                {
                                    Main.instance.TilesRenderer.DrawSingleTile(new(), true, 0, Main.screenPosition,
                                        DrawingUtils.TileOffset() + new Vector2(2, 0).RotatedBy(MathHelper.PiOver2 * ii), a.X, a.Y);
                                }
                            }
                        }
                    }
                }
                
                Main.spriteBatch.Restart(sb);
                
                target.Dispose();
            }
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