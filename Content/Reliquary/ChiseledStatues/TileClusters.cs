using Everware.Content.Base;
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

        On_Main.DrawPlayers_AfterProjectiles += DrawAllBlocks;
    }

    public override void Unload()
    {
        On_Main.DrawPlayers_AfterProjectiles -= DrawAllBlocks;
    }

    private void DrawAllBlocks(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
    {
        List<Projectile> projs = [];

        Color c = Color.White;

        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.ModProjectile is TileCluster cluster)
            {
                projs.Add(projectile);
            }

        projs.Sort((a, b) => { return a.scale > b.scale ? 1 : -1; });

        Main.spriteBatch.Begin(SpriteSortMode.BackToFront, Main._multiplyBlendState, Main.DefaultSamplerState, DepthStencilState.DepthRead, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

        foreach (Projectile projectile in projs)
            if (projectile.ModProjectile is TileCluster cluster)
                cluster.DrawSelf(ref c, 0f, 0.85f);

        Main.spriteBatch.End(out var sb);

        orig(self);

        Main.spriteBatch.Begin(sb);

        foreach (Projectile projectile in projs)
            if (projectile.ModProjectile is TileCluster cluster)
                cluster.DrawSelf(ref c, 0.85f, 2f);

        Main.spriteBatch.End();
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
        Projectile.width = 100;
        Projectile.height = 100;
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
                    var drawData = new TileDrawInfo
                    {
                        tileCache = Main.tile[i, j]
                    };
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

                    if (Main.tileSolid[drawData.typeCache])
                    {
                        cluster[i - topLeft.X, j - topLeft.Y] = new ClusterTileData(
                        drawData.tileCache.HasTile,
                        drawData.tileCache.Slope,
                        drawData.tileCache.IsHalfBlock,
                        drawData.tileCache.IsTileInvisible,
                        drawData.tileCache.IsTileFullbright,
                        drawData
                    );
                    }

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

    public int ClusterIndex = 0;
    public int MaxClusterIndex = 6;
    float TimerOffset = Main.rand.NextFloat(10f);
    float Timer = 0f;
    bool Sent = false;
    float VelocityMod = 0f;
    float VelocityMod2 = Main.rand.NextFloat(0.8f, 1.2f);
    public bool Fall = false;
    public Vector2 VelocityTarget = Vector2.Zero;
    public override void AI()
    {
        float t = ((float)((float)ClusterIndex / (float)MaxClusterIndex)) * MathHelper.TwoPi;

        Timer++;
        if (!Fall)
        {
            if (!Sent)
            {
                if (Timer < 15)
                {
                    Projectile.velocity *= 0.9f;
                }

                VelocityMod = MathHelper.Lerp(VelocityMod, 0.5f, 0.1f);
                Vector2 off = new Vector2((float)Math.Sin((GlobalTimer.Value / 30f) + t) * 60f, ((float)Math.Sin((GlobalTimer.Value / 30f) + t + MathHelper.PiOver2) * 20f));
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.8f + (off.Y / 70f * 0.6f), 0.2f);
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, (float)Math.Sin(Timer / 45f) * 0.3f, 0.1f);

                off.Y += (float)Math.Sin(Timer / 15f) * 6f;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Lerp(Projectile.Center, Owner.Center + off + new Vector2(0, 12), 0.3f * VelocityMod) - Projectile.Center, 0.3f * VelocityMod * VelocityMod2);
                if (NetworkOwner.MouseDown)
                {
                    Projectile.velocity -= Owner.DirectionTo(NetworkOwner.MousePosition) * 5f;
                    Sent = true;
                    VelocityTarget = Owner.DirectionTo(NetworkOwner.MousePosition) * 10f;
                    Projectile.owner = Main.player.Length - 1;
                    Timer = -TimerOffset;
                }
                Projectile.timeLeft = 140;
            }
            else
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.8f, 0.2f);

                Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.X * 0.5f);

                if (Timer > 0)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, VelocityTarget, 0.1f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }

                if (Timer > 20) KillIfColliding();

                Projectile.velocity *= 1.1f;
            }
        }
        else
        {
            Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.Y * 0.5f);
            Projectile.velocity.Y += 1f;
            KillIfColliding();
        }
    }

    public void KillIfColliding()
    {
        Tile tle = Main.tile[(Projectile.Center / 16 + new Vector2(0.5f, 0.5f)).ToPoint()];

        if (tle.HasTile && Main.tileSolid[tle.TileType])
        {
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }

    public void DrawSelf(ref Color lightColor, float scalerange1, float scalerange2)
    {
        if (Projectile.scale > scalerange2 || Projectile.scale < scalerange1) return;

        var sb = Main.spriteBatch;

        if (cluster is null)
        {
            return;
        }

        var origin = new Vector2((ClusterSize * 16) / 2f);

        var transform =
            Matrix.CreateTranslation(new Vector3(new Vector2(200), 0f))
          * Main.GameViewMatrix.TransformationMatrix;

        var rt = RenderTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, 400, 400);

        sb.End(out var ss);
        using (rt.Scope(clearColor: Color.Transparent))
        {
            sb.Begin(ss with { TransformMatrix = transform });
            for (var i = 0; i < ClusterSize; i++)
                for (var j = 0; j < ClusterSize; j++)
                {
                    DrawSlopedTile(i, j, false);
                    DrawSlopedTile(i, j, true);
                }
            sb.End();
        }
        sb.Begin(ss with { });

        Main.EntitySpriteDraw(rt.Target, Projectile.Center - Main.screenPosition, rt.Target.Bounds, Color.Lerp(Color.DarkGray, Color.White, Projectile.scale), Projectile.rotation, new Vector2(200), Projectile.scale, SpriteEffects.None, Projectile.scale);

        sb.Restart(in ss);

        rt.Dispose();

        return;

        void DrawSlopedTile(int i, int j, bool useGlowMask)
        {
            var (hasTile, slope, halfTile, invisible, fullBright, drawData) = cluster![i, j];

            if (!hasTile || invisible)
            {
                return;
            }

            var position = -origin + new Vector2(i * 16f, j * 16f + drawData.tileTop);

            var source = new Rectangle(drawData.tileFrameX + drawData.addFrX, drawData.tileFrameY + drawData.addFrY, drawData.tileWidth, drawData.tileHeight);

            drawData.tileLight = fullBright ? Color.White : Lighting.GetColor((Projectile.Center + position).ToTileCoordinates());
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
                sb.Draw(texture, position, source, color, 0f, Vector2.Zero, 1f, drawData.tileSpriteEffect, 0f);
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
        /*
        if (Main.mouseRight && Main.mouseRightRelease)
        {
            var (i, j) = Main.MouseWorld.ToTileCoordinates();

            var spawnPosition = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates();

            Projectile.NewProjectile(new EntitySource_Misc(""), spawnPosition, Vector2.Zero, ModContent.ProjectileType<TileCluster>(), 1, 1, Main.myPlayer, i, j, 3);
        }
        */
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
