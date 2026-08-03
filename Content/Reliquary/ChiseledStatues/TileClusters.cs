using Daybreak.Common.Features.Hooks;
using Everware.Core.Projectiles;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

public sealed class TileCluster : EverProjectile
{
    // If you somehow still get a stack overflow, so god help me
#region Framing Patches
    public override void Load()
    {
        On_Item.NewItem_Inner += NewItem_Inner_DisableFauxFraming;

        On_WorldGen.KillTile += KillTile_DisableFauxFraming;

        // Jungle plants
        On_NPC.SpawnOnPlayer += SpawnOnPlayer_DisableFauxFraming;

        On_WorldGen.SpawnFallingBlockProjectile += SpawnFallingBlockProjectile_DisableFauxFraming;
        
        On_WorldGen.CheckPot += (orig, i, j, type) => { if (tileFrameCosmeticOnly) { return; } orig(i, j, type); };
        On_WorldGen.CheckJunglePlant += (orig, i, j, type) => { if (tileFrameCosmeticOnly) { return; } orig(i, j, type); };
    }

    private static bool SpawnFallingBlockProjectile_DisableFauxFraming(On_WorldGen.orig_SpawnFallingBlockProjectile orig, int i, int j, Tile tileCache, Tile tileTopCache, Tile tileBottomCache, int type)
    {
        if (tileFrameCosmeticOnly)
        {
            return false;
        }

        return orig(i, j, tileCache, tileTopCache, tileBottomCache, type);
    }

    private static void SpawnOnPlayer_DisableFauxFraming(On_NPC.orig_SpawnOnPlayer orig, int plr, int type)
    {
        if (tileFrameCosmeticOnly)
        {
            return;
        }

        orig(plr, type);
    }

    private static void KillTile_DisableFauxFraming(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
    {
        if (tileFrameCosmeticOnly)
        {
            return;
        }

        orig(i, j, fail, effectOnly, noItem);
    }

    private static int NewItem_Inner_DisableFauxFraming(On_Item.orig_NewItem_Inner orig, IEntitySource source, int x, int y, int width, int height, Item itemToClone, int type, int stack, bool noBroadcast, int prefix, bool noGrabDelay, bool reverseLookup)
    {
        if (tileFrameCosmeticOnly)
        {
            return -1;
        }

        return orig(source, x, y, width, height, itemToClone, type, stack, noBroadcast, prefix, noGrabDelay, reverseLookup);
    }

    private static bool tileFrameCosmeticOnly;

    private static int recursionCount;

    [GlobalTileHooks.TileFrame]
    private static bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
    {
        if (tileFrameCosmeticOnly)
        {
            if (recursionCount > 10)
            {
                noBreak = true;
                return false;
            }

            recursionCount++;
        }

        return true;
    }
#endregion

    private record struct ClusterTileData(
        bool HasTile,
        int CoordinateWidth,
        int CoordinateHeight,
        SlopeType Slope,
        bool HalfTile,
        bool InvisibleCoating,
        bool FullBrightCoating,
        TileDrawInfo DrawData
    );

    // TODO
    public override string Texture => Assets.Textures.Reliquary.ChiseledStatues.CrystalHeartStatue.KEY;

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.scale = 1f;

        Projectile.penetrate = -1;

        Projectile.friendly = true;
        Projectile.hostile = false;

        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;

        Projectile.manualDirectionChange = true;

        Projectile.tileCollide = false;
    }

    public int TileCenterX => (int)Projectile.ai[0];

    public int TileCenterY => (int)Projectile.ai[1];

    public ushort ClusterSize => (ushort)Projectile.ai[2];

    private ClusterTileData[,]? cluster;

    public override void NetOnSpawn()
    {
        if (Main.dedServ
         || TileCenterX <= (ClusterSize / 2) + 1 || TileCenterX >= Main.maxTilesX - (ClusterSize / 2) + 1
         || TileCenterY <= (ClusterSize / 2) + 1 || TileCenterY >= Main.maxTilesY - (ClusterSize / 2) + 1)
        {
            return;
        }

        cluster = new ClusterTileData[ClusterSize, ClusterSize];
        var topLeft = new Point(TileCenterX - (ClusterSize / 2), TileCenterY - (ClusterSize / 2));

        var backupSize = ClusterSize + 2;
        var outerTopLeft = topLeft - new Point(1, 1);

        var holder = new TileDataHolder(backupSize * backupSize);
        BackupCluster();
        {
            // Clear the edges around the cluster to have tiles inside frame nicely
            ClearEdges();

            tileFrameCosmeticOnly = true;
            {
                for (var i = topLeft.X; i < topLeft.X + ClusterSize; i++)
                for (var j = topLeft.Y; j < topLeft.Y + ClusterSize; j++)
                {
                    // TODO: 1.4.5 Move to TileFrameCosmetic
                    recursionCount = 0;

                    var priorGen = WorldGen.gen;
                    WorldGen.gen = true;
                    {
                        WorldGen.TileFrame(i, j, noBreak: true);
                    }
                    WorldGen.gen = priorGen;

                    var tile = Main.tile[i, j];

                    if (tile.HasTile)
                    {
                        WorldGen.KillTile_MakeTileDust(i, j, tile);
                    }
                }
            }
            tileFrameCosmeticOnly = false;

            var tileRenderer = Main.instance.TilesRenderer;

            for (var i = topLeft.X; i < topLeft.X + ClusterSize; i++)
            for (var j = topLeft.Y; j < topLeft.Y + ClusterSize; j++)
            {
                var drawData = new TileDrawInfo();

                drawData.tileCache = Main.tile[i, j];
                drawData.typeCache = drawData.tileCache.type;
                drawData.tileFrameX = drawData.tileCache.frameX;
                drawData.tileFrameY = drawData.tileCache.frameY;
                drawData.tileLight = Color.White;
                drawData.colorTint = Color.White;
                drawData.finalColor = TileDrawing.GetFinalLight(drawData.tileCache, drawData.typeCache, drawData.tileLight, drawData.colorTint);
                tileRenderer.GetTileDrawData(
                    i,
                    j,
                    drawData.tileCache,
                    drawData.typeCache,
                    ref drawData.tileFrameX,
                    ref drawData.tileFrameY,
                    out drawData.tileWidth,
                    out drawData.tileHeight,
                    out drawData.tileTop,
                    out drawData.halfBrickHeight,
                    out drawData.addFrX,
                    out drawData.addFrY,
                    out drawData.tileSpriteEffect,
                    out drawData.glowTexture,
                    out drawData.glowSourceRect,
                    out drawData.glowColor
                );
                drawData.drawTexture = tileRenderer.GetTileDrawTexture(drawData.tileCache, i, j);

                var width = 16;
                var height = 16;

                var _offsetY = 0;
                var _frameX = (short)0;
                var _frameY = (short)0;
                TileLoader.SetDrawPositions(i, j, ref width, ref _offsetY, ref height, ref _frameX, ref _frameY);

                cluster[i - topLeft.X, j - topLeft.Y] = new ClusterTileData(
                    drawData.tileCache.HasTile,
                    width,
                    height,
                    drawData.tileCache.Slope,
                    drawData.tileCache.IsHalfBlock,
                    drawData.tileCache.IsTileInvisible,
                    drawData.tileCache.IsTileFullbright,
                    drawData
                );
            }
        }
        RestoreCluster();

        return;

        void BackupCluster()
        {
            var index = 0;

            for (var i = outerTopLeft.X; i < outerTopLeft.X + backupSize; i++)
            for (var j = outerTopLeft.Y; j < outerTopLeft.Y + backupSize; j++)
            {
                holder.CopyFrom(Main.tile[i, j], index);
                index++;
            }
        }

        void RestoreCluster()
        {
            var index = 0;

            for (var i = outerTopLeft.X; i < outerTopLeft.X + backupSize; i++)
            for (var j = outerTopLeft.Y; j < outerTopLeft.Y + backupSize; j++)
            {
                holder.CopyTo(index, Main.tile[i, j]);
                index++;
            }
        }

        void ClearEdges()
        {
            for (var i = 0; i < backupSize; i++)
            for (var j = 0; j < 2; j++)
            {
                var tile = Main.tile[outerTopLeft.X + i, outerTopLeft.Y + j * (backupSize - 1)];

                // Precaution as some non-solid tiles don't play nicely
                if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                {
                    tile.HasTile = false;
                    tile.WallType = WallID.None;
                }
            }

            for (var j = 0; j < backupSize; j++)
            for (var i = 0; i < 2; i++)
            {
                var tile = Main.tile[outerTopLeft.X + i * (backupSize - 1), outerTopLeft.Y + j];

                if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                {
                    tile.HasTile = false;
                    tile.WallType = WallID.None;
                }
            }
        }
    }

    public override void AI()
    {
        Projectile.velocity.Y = -4f;
        Projectile.timeLeft = 50;
        Projectile.rotation += 0.01f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var sb = Main.spriteBatch;

        if (cluster is null)
        {
            return false;
        }

        var origin = new Vector2((ClusterSize * 16) / 2f);

        var transform =
            Matrix.CreateRotationZ(Projectile.rotation)
          * Matrix.CreateTranslation(new Vector3(Projectile.Center - Main.screenPosition, 0f))
          * Main.GameViewMatrix.TransformationMatrix;

        sb.End(out var ss);
        sb.Begin(ss with { TransformMatrix = transform});
        {
            for (var i = 0; i < ClusterSize; i++)
            for (var j = 0; j < ClusterSize; j++)
            {
                DrawSlopedTile(i, j, false);
                DrawSlopedTile(i, j, true);
            }
        }
        sb.Restart(in ss);

        return false;

        void DrawSlopedTile(int i, int j, bool useGlowMask)
        {
            var (hasTile, frameWidth, frameHeight, slope, halfTile, invisible, fullBright, drawData) = cluster![i, j];

            if (!hasTile || invisible)
            {
                return;
            }

            var position = -origin + new Vector2(i * 16f, j * 16f);

            var source = new Rectangle(drawData.tileFrameX + drawData.addFrX, drawData.tileFrameY + drawData.addFrY, frameWidth, frameHeight);

            drawData.tileLight = fullBright ? Color.White : Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            drawData.colorTint = Color.White;
            drawData.finalColor = TileDrawing.GetFinalLight(drawData.tileCache, drawData.typeCache, drawData.tileLight, drawData.colorTint);

            var color = useGlowMask ? drawData.glowColor : drawData.finalColor;

            var texture = useGlowMask ? drawData.glowTexture : drawData.drawTexture;

            if (texture is null)
            {
                return;
            }

            if (slope == SlopeType.Solid && !halfTile)
            {
                sb.Draw(texture, position, source, color);
            }
            else if (halfTile)
            {
                sb.Draw(texture, new Vector2(position.X, position.Y + 8), new Rectangle(source.X, source.Y, 16, 8), color);
            }
            else
            {
                if (slope is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight)
                {
                    for (var a = 0; a < 16; a += 2)
                    {
                        int length;
                        int height;

                        if (slope == SlopeType.SlopeDownRight)
                        {
                            length = 16 - a - 2;
                            height = 16 - a;
                        }
                        else
                        {
                            length = a;
                            height = 16 - length;
                        }

                        sb.Draw(texture, position + new Vector2(length, a), new Rectangle(source.X + length, source.Y, 2, height), color);
                    }
                }
                else
                {
                    for (var a = 0; a < 16; a += 2)
                    {
                        int length;
                        int height;

                        if (slope == SlopeType.SlopeUpLeft)
                        {
                            length = a;
                            height = 16 - length;
                        }
                        else
                        {
                            length = 16 - a - 2;
                            height = 16 - a;
                        }

                        sb.Draw(texture, position + new Vector2(length, 0), new Rectangle(source.X + length, source.Y + 16 - height, 2, height), color);
                    }
                }
            }
        }
    }

    [ModSystemHooks.PostUpdateWorld]
    private static void PostUpdateWorld()
    {
        if (Main.mouseRight && Main.mouseRightRelease)
        {
            var (i, j) = Main.MouseWorld.ToTileCoordinates();

            var spawnPosition = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates();

            Projectile.NewProjectile(new EntitySource_Misc(""), spawnPosition, Vector2.Zero, ModContent.ProjectileType<TileCluster>(), 1, 1, Main.myPlayer, i, j, 5);
        }
    }
}

public sealed class TileDataHolder
{
    private static readonly Dictionary<Type, int> type_sizes = [];
    private static readonly MethodInfo sizeOfMethod = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf))!;
    private static readonly FieldInfo onCopyFromField = typeof(TileDataHolder).GetField(nameof(OnCopyFrom), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo onCopyToField = typeof(TileDataHolder).GetField(nameof(OnCopyTo), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo copyFromGenericMethod = typeof(TileDataHolder).GetMethod(nameof(CopyFromGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo copyToGenericMethod = typeof(TileDataHolder).GetMethod(nameof(CopyToGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    public int Length { get; }
    private readonly Dictionary<Type, int> dataOffsets = [];

    private readonly byte[] data;

    private Action<TileDataHolder, Tile, int>? OnCopyFrom;
    private Action<TileDataHolder, int, Tile>? OnCopyTo;

    public TileDataHolder(int length)
    {
        Length = length;

        // TODO: Find a proper method to move this to load time?
        var types = TileData.OnSetLength.GetInvocationList().Select(x => x.Method.DeclaringType!.GenericTypeArguments[0]!).ToArray()!;
        foreach (var type in types)
        {
            if (!type_sizes.ContainsKey(type))
            {
                type_sizes[type] = SizeOf(type);
            }
        }

        var totalBytes = 0;
        foreach (var type in types)
        {
            dataOffsets[type] = totalBytes;
            totalBytes += type_sizes[type] * length;
        }

        data = new byte[totalBytes];

        var fromDelegates = new Delegate[types.Length];
        var toDelegates = new Delegate[types.Length];
        for (var i = 0; i < types.Length; i++)
        {
            fromDelegates[i] = Delegate.CreateDelegate(onCopyFromField.FieldType, copyFromGenericMethod.MakeGenericMethod(types[i]));
            toDelegates[i] = Delegate.CreateDelegate(onCopyToField.FieldType, copyToGenericMethod.MakeGenericMethod(types[i]));
        }

        OnCopyFrom = (Action<TileDataHolder, Tile, int>?)Delegate.Combine(fromDelegates);
        OnCopyTo = (Action<TileDataHolder, int, Tile>?)Delegate.Combine(toDelegates);
    }

    public void CopyFrom(Tile tile, int index)
    {
        OnCopyFrom?.Invoke(this, tile, index);
    }

    public void CopyTo(int index, Tile tile)
    {
        OnCopyTo?.Invoke(this, index, tile);
    }

    private static int SizeOf(Type t)
    {
        return (int)sizeOfMethod.MakeGenericMethod(t).Invoke(null, null)!;
    }

    private static unsafe void CopyFromGeneric<T>(TileDataHolder holder, Tile tile, int index) where T : unmanaged, ITileData
    {
        fixed (byte* data = &holder.data[holder.dataOffsets[typeof(T)]])
        {
            T* arr = (T*)data;
            arr[index] = TileData<T>.ptr[tile.TileId];
        }
    }

    private static unsafe void CopyToGeneric<T>(TileDataHolder holder, int index, Tile tile) where T : unmanaged, ITileData
    {
        fixed (byte* data = &holder.data[holder.dataOffsets[typeof(T)]])
        {
            T* arr = (T*)data;
            TileData<T>.ptr[tile.TileId] = arr[index];
        }
    }
}
