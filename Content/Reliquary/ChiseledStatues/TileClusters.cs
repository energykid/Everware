using Everware.Core.Projectiles;
using Terraria.GameContent.Drawing;

namespace Everware.Content.Reliquary.ChiseledStatues;

public sealed class TileCluster : EverProjectile
{
    // TODO
    public override string Texture => Assets.Textures.Reliquary.ChiseledStatues.CrystalHeartStatue.KEY;

    public int TileCenterX => (int)Projectile.ai[0];

    public int TileCenterY => (int)Projectile.ai[1];

    public ushort ClusterSize => (ushort)Projectile.ai[2];

    public Tilemap Cluster;

    public override void NetOnSpawn()
    {
        Cluster = new Tilemap(ClusterSize, ClusterSize);
        var topLeft = new Point(TileCenterX - (ClusterSize / 2), TileCenterY - (ClusterSize / 2));

        var backup = new Tilemap((ushort)(ClusterSize + 2), (ushort)(ClusterSize + 2));
        var outerTopLeft = topLeft - new Point(1, 1);

        BackupCluster();
        {
            // Clear the edges around the cluster to have tiles inside frame nicely
            ClearEdges();

            for (var i = topLeft.X; i < topLeft.X + Cluster.Width; i++)
            for (var j = topLeft.Y; j < topLeft.Y + Cluster.Height; j++)
            {
                WorldGen.TileFrame(i, j, noBreak: true);
            }

            CloneClusterToTilemap(topLeft, Cluster);
        }
        RestoreCluster();

        return;

        void BackupCluster()
        {
            CloneClusterToTilemap(outerTopLeft, backup);
        }

        void RestoreCluster()
        {
            for (var i = 0; i < backup.Width; i++)
            for (var j = 0; j < backup.Height; j++)
            {
                var tile = backup[i, j];
                Main.tile[outerTopLeft.X + i, outerTopLeft.Y + j] = tile;
            }
        }

        void ClearEdges()
        {
            for (var i = 0; i < backup.Width; i++)
            for (var j = 0; j < 2; j++)
            {
                Main.tile[outerTopLeft.X + i, outerTopLeft.Y + j * backup.Height].HasTile = false;
            }

            for (var j = 0; j < backup.Height; j++)
            for (var i = 0; i < 2; i++)
            {
                Main.tile[outerTopLeft.X + i * backup.Height, outerTopLeft.Y + j].HasTile = false;
            }
        }

        static void CloneClusterToTilemap(Point topLeft, Tilemap map)
        {
            for (var i = 0; i < map.Width; i++)
            for (var j = 0; j < map.Height; j++)
            {
                var tile = Framing.GetTileSafely(topLeft.X + i, topLeft.Y + j);
                map[i, j] = tile;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var origin = (ClusterSize * 16) / 2f;

        var transform = Matrix.CreateTranslation();

        Main.tileBatch.Begin();

        var prior = Main.tile;
        var priorMenu = Main.gameMenu;
        Main.tile = Cluster;
        Main.gameMenu = true;
        {
            for (var i = 0; i < Cluster.Width; i++)
            for (var j = 0; j < Cluster.Height; j++)
            {
                Main.instance.TilesRenderer.DrawSingleTile(new TileDrawInfo(), true, Main.waterStyle, Vector2.Zero, -origin, i, j);
            }
        }
        Main.gameMenu = priorMenu;
        Main.tile = prior;

        return false;
    }
}
