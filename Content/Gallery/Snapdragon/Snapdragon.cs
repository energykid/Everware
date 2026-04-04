using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Utils;
using System;

namespace Everware.Content.Gallery.Snapdragon;

[AutoloadBossHead]
public class Snapdragon : ModNPC
{
    public Player Target => Main.player[NPC.target];
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/Snapdragon_Head";
    public override string BossHeadTexture => "Everware/Assets/Textures/Gallery/Snapdragon/Snapdragon_Head_Boss";
    public static int DefaultContactDamage => 45;
    public Vector2 BasePosition = Vector2.Zero;
    public Vector2[] SpinePositions = Array.Empty<Vector2>();
    public Vector2[] SpineCurve = Array.Empty<Vector2>();
    public bool start = false;
    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.life = NPC.lifeMax = 25000;
        NPC.defense = 35;
        NPC.knockBackResist = 0f;
        NPC.damage = 0;
        NPC.width = 120; NPC.height = 190;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
        BasePosition = (GallerySystem.GalleryPosition.ToVector2() * 16).Grounded();
        SpinePositions = new Vector2[15];
        for (float i = 0; i < 15; i++)
        {
            SpinePositions[(int)i] = Vector2.Lerp(NPC.Center, BasePosition, i / 15f);
        }
        SpineCurve = new Vector2[2];
        for (float i = 0; i < 2; i++)
        {
            SpineCurve[(int)i] = Vector2.Lerp(NPC.Center, BasePosition, i / 2f);
        }
        Music = Assets.Sounds.Music.Snapdragon.Slot;
    }
    public override void AI()
    {
        NPC.rotation = MathHelper.Clamp(NPC.rotation, -0.5f, 0.5f);

        if (!start)
        {
            for (float i = 0; i < 15; i++)
            {
                SpinePositions[(int)i] = Vector2.Lerp(NPC.Center, BasePosition, i / 15f);
            }
            for (float i = 0; i < 2; i++)
            {
                SpineCurve[(int)i] = Vector2.Lerp(NPC.Center, BasePosition, i / 15f);
            }
            start = true;
        }

        CurveAnimation_Idle();

        NPC.TargetClosest();

        NPC.rotation = MathHelper.Lerp(NPC.rotation, (NPC.Center.X - Target.Center.X) * 0.001f, 0.4f);

        NPC.ai[0] += 1f;

        float maxDist = NPC.Center.Distance(BasePosition);
        maxDist = MathHelper.Clamp(maxDist, 200, 250);

        NPC.VelocityMoveTowardsPosition(Target.Center + new Vector2(-20f + (float)(Math.Sin(NPC.ai[0] / 60) * 100f), -220 + (float)(Math.Sin(NPC.ai[0] / 63) * 20f)), 0.1f, 0.05f, 20);

        Vector2 outPos = new Vector2(maxDist, 0).RotatedBy(BasePosition.AngleTo(NPC.Center));

        NPC.Center = Vector2.Lerp(NPC.Center, BasePosition + (outPos * new Vector2(0.7f, 1.0f)), 0.3f);

        for (float i = 0; i < 15; i++)
        {
            float skewX = (NPC.Center.X - BasePosition.X) * MathHelper.Lerp(0.2f, 0.1f, (i / 15f));
            float skew = Math.Abs(NPC.Center.X - BasePosition.X) * 0.2f;
            skew -= 10;
            if (skew < 0) skew = 0;
            float v = Easing.KeyFloat(i, 0, 7, 1f, 0.2f, Easing.OutExpo, 0f);
            v = Easing.KeyFloat(i, 7, 15, 0.2f, 1f, Easing.InExpo, v);

            Vector2 off = (MathUtils.CubicBezier2(NPC.Center + new Vector2(0, -10).RotatedBy(NPC.rotation), SpineCurve[1], SpineCurve[0], BasePosition, i / 15f));

            SpinePositions[(int)i] = Vector2.Lerp(SpinePositions[(int)i], off, v);
        }
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Asset<Texture2D> HeadTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Head.Asset;
        Asset<Texture2D> CollarTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Collar.Asset;

        DrawSpine();
        DrawSpine(true);

        for (int i = -4; i <= 4; i++)
        {
            for (int j = 0; j <= 4; j++)
            {
                Point p = ((BasePosition / 16) + new Vector2(i, j)).ToPoint();
                if (p.X > 100 && p.X < Main.maxTilesX - 100)
                    if (p.Y > 100 && p.Y < Main.maxTilesY - 100 && Main.tile[p].HasUnactuatedTile)
                        Main.instance.TilesRenderer.DrawSingleTile(new TileDrawInfo(), true, 0, screenPos, Vector2.Zero, p.X, p.Y);
            }
        }

        Main.EntitySpriteDraw(CollarTex.Value, NPC.Center + new Vector2(0, 10) - screenPos + (NPC.DirectionTo(BasePosition) * 10), CollarTex.Frame(), drawColor, 0f, CollarTex.Size() / 2f, 1f, SpriteEffects.None);
        Main.EntitySpriteDraw(HeadTex.Value, NPC.Center - screenPos + new Vector2(6, 25), HeadTex.Frame(), drawColor, NPC.rotation, HeadTex.Size() / 2f, 1f, SpriteEffects.None);

        return false;
    }

    public void DrawSpine(bool topLayer = false)
    {
        Asset<Texture2D> NeckTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Neck.Asset;
        Asset<Texture2D> SpineTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Spine.Asset;
        Asset<Texture2D> CollarSpineTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_SpineCollared.Asset;
        Asset<Texture2D> CollarSpineTex_Top = Assets.Textures.Gallery.Snapdragon.Snapdragon_SpineCollared_Top.Asset;

        for (int i = 0; i < SpinePositions.Length; i++)
        {
            var t = SpineTex;
            if ((i + 1) % 4 == 3 && i < SpinePositions.Length - 2) t = topLayer ? CollarSpineTex_Top : CollarSpineTex;
            if (i == 1) t = NeckTex;

            Rectangle f = t.Frame();
            if (t == SpineTex) f = t.Frame(2, frameX: i % 2);

            Vector2 lastPos = NPC.Center;
            if (i > 0) lastPos = SpinePositions[i - 1];
            Color c = Lighting.GetColor((SpinePositions[i] / 16).ToPoint());
            if (!topLayer || t == CollarSpineTex_Top)
                Main.EntitySpriteDraw(t.Value, SpinePositions[i] + new Vector2(0, 20) - Main.screenPosition + (NPC.DirectionTo(BasePosition) * 10), f, c, SpinePositions[i].AngleTo(lastPos) + MathHelper.ToRadians(90f), f.Size() / 2f, 1f, SpriteEffects.None);
        }
    }

    public void CurveAnimation_Idle()
    {
        float rot = -NPC.rotation * 3f;

        rot = MathHelper.Clamp(rot, -0.5f, 0.5f);

        SpineCurve[0] = Vector2.Lerp(SpineCurve[0], Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + (NPC.DirectionTo(BasePosition) * (-50 * Math.Abs(NPC.rotation) * 2f)).RotatedBy(-rot) + new Vector2(0, 40), 0.2f);
        SpineCurve[1] = Vector2.Lerp(SpineCurve[1], NPC.Center + (NPC.DirectionTo(BasePosition) * 200).RotatedBy(rot) + new Vector2(MathHelper.ToDegrees(NPC.rotation * 3), -100) + new Vector2(MathHelper.ToDegrees(NPC.rotation), 0), 0.35f);

        SpineCurve[0] += new Vector2((float)Math.Sin(GlobalTimer.Value / 25f) * 2f, (float)Math.Sin(GlobalTimer.Value / 24f) * 2f);
        SpineCurve[1] -= new Vector2((float)Math.Sin(GlobalTimer.Value / 15f) * 2f, (float)Math.Sin(GlobalTimer.Value / 14f) * 2f);
    }
}
