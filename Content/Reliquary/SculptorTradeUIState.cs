using Everware.Common.Systems;
using Everware.Content.Base.ParticleSystem;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Everware.Content.Reliquary;

public class SculptorTradeUIState : UIState
{
    public static ParticleLayer Layer;
    public UIPanel BigPanel;
    public UITextBox DialogueText;
    public UIButton<string> ChiselButton;
    public SculptorStatueSlot StatueSlot;
    Item[] Statue = { new Item(ItemID.None) };
    public Vector2 Position;
    public bool CanBeQuit = true;
    public int Timer = 0;

    public override void OnInitialize()
    {
        Layer = new ParticleLayer();

        BigPanel = new UIPanel();
        BigPanel.Width.Set(600, 0f);
        BigPanel.Height.Set(100, 0f);

        DialogueText = new UITextBox("")
        {
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent
        };
        DialogueText.Left.Set(0, 0f);
        DialogueText.Top.Set(0, 0f);
        DialogueText.Width.Set(300, 0f);
        DialogueText.Height.Set(100, 0f);
        DialogueText.SetTextMaxLength(100);
        DialogueText.ShowInputTicker = false;

        List<string> key = ["One", "Two", "Three"];
        SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.TradeGreeting.GetChildText(key[Main.rand.Next(key.Count)]).Value);

        ChiselButton = new UIButton<string>("Chisel!") { ClickSound = SoundID.MenuTick };
        ChiselButton.Left.Set(-65, 1f);
        ChiselButton.Width.Set(70, 0f);
        ChiselButton.Top.Set(-25, 1f);
        ChiselButton.Height.Set(30, 0f);
        ChiselButton.OnLeftClick += Trade;

        StatueSlot = new SculptorStatueSlot(Statue, 0, 0);
        StatueSlot.Left.Set(0f, 0.04f);
        StatueSlot.Top.Set(0f, -0.25f);
        StatueSlot.HAlign = 1f;
        StatueSlot.VAlign = 1f;

        BigPanel.Append(StatueSlot);
        BigPanel.Append(ChiselButton);
        BigPanel.Append(DialogueText);
        Append(BigPanel);
    }

    public void Trade(UIMouseEvent evt, UIElement listeningElement)
    {
        List<string> key = ["One", "Two", "Three"];
        SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.NoStatue.GetChildText(key[Main.rand.Next(key.Count)]).Value);
        if (Statue[0].Name.Contains(" Statue"))
            SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.InvalidStatue.GetChildText(key[Main.rand.Next(key.Count)]).Value);
        /*if (Statue[0].type == ItemID.AngelStatue)
            SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.AngelStatue.GetChildText(key[Main.rand.Next(key.Count)]).Value);
        else*/
        {
            foreach (Chiselable ch in ChiselablesList.AllChiselables)
            {
                if (StatueSlot._itemArray[0].type == ch.BaseStatue)
                {
                    StatueSlot.StartChiseling(ch.UpgradedStatue);
                    SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.SculptAnimation.GetChildText(key[Main.rand.Next(key.Count)]).Value);
                    Timer = 60;
                }
            }
        }
    }

    public override void OnDeactivate()
    {
        Layer.AllParticles.Clear();
    }

    public void SpawnChiselParticle(float rot)
    {
        float a = (Timer - 30) / 30f;
        new SculptorStatueSlot.SculptorHammerChiselParticle(rot, rot > 0 ? false : true, -a).Spawn(Layer);
    }

    public void SetDialogue(string value)
    {
        DialogueText.Left.Set(-5, 0f);
        DialogueText.TextColor = Color.Transparent;
        DialogueText.SetText(value);
    }

    public override void Update(GameTime gameTime)
    {
        Layer.Update();

        ChiselButton.ClickSound = SoundID.MenuTick;

        Timer--;
        if (Timer == 20)
        {
            SoundEngine.PlaySound(Assets.Sounds.NPC.ChiselComplete.Asset);
            List<string> key = ["One", "Two", "Three"];
            SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.SculptAnimationFinished.GetChildText(key[Main.rand.Next(key.Count)]).Value);

            StatueSlot.SetChiselPoints(0f);
            StatueSlot.SetItemFinal();
        }
        if (Timer > 30 && Timer % 8 == 0)
        {
            float vv = (Timer - 30) * 3;
            SpawnChiselParticle(MathHelper.ToRadians(Timer % 16 >= 8 ? vv : -vv));
            StatueSlot.SetChiselPoints(((float)Timer - 30) / 30);
        }

        StatueSlot.Left.Set(0f, 0.01f);
        StatueSlot.Top.Set(0f, -0.45f);
        StatueSlot.HAlign = 1f;
        StatueSlot.VAlign = 1f;

        float v = 200;
        if (Main.screenHeight > 1000) v = -50;

        BigPanel.Left.Set(Position.X - 300 - Main.screenPosition.X, 0f);
        BigPanel.Top.Set(Position.Y + v - Main.screenPosition.Y, 0f);

        DialogueText.Left.Set(MathHelper.Lerp(DialogueText.Left.Pixels, 0f, 0.2f), 0f);
        DialogueText.Width.Set(300, 0f);
        DialogueText.TextHAlign = 0f;
        DialogueText.TextColor = Color.Lerp(DialogueText.TextColor, Color.White, 0.45f);

        if (StatueSlot.IsMouseHovering)
        {
            StatueSlot.Scale = MathHelper.Lerp(StatueSlot.Scale, 0.1f, 0.4f);
        }
        else
        {
            StatueSlot.Scale = MathHelper.Lerp(StatueSlot.Scale, 0f, 0.4f);
        }
    }
}


public class SculptorStatueSlot : UIElement
{
    public class SculptorHammerChiselParticle : Particle
    {
        float Outset = 0f;
        float Pitch = 0f;
        Vector2 targetPos = Vector2.Zero;
        float Timer = 0f;
        bool Flip = false;
        float HammerRot = -45f;
        public SculptorHammerChiselParticle(float rotation, bool flip, float pitch) : base(Vector2.Zero, Vector2.Zero, Vector2.One, null, null)
        {
            Pitch = pitch;
            Flip = flip;
            Vector2 cen = ReliquaryUISystem.TradeState.StatueSlot.GetDimensions().Center();
            Outset = Main.rand.NextFloat(2f, 10f);
            Rotation = rotation;
            position = cen + new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)) + new Vector2(0, -5).RotatedBy(Rotation);
            targetPos = position;
            position += new Vector2(0, -10).RotatedBy(targetPos.AngleFrom(cen));
            position.X += Flip ? -30 : 30;
            targetPos.X += Flip ? -10 : 10;
            Scale = new Vector2(0.2f, 0f);
        }
        public override void Update()
        {
            if (Timer < 15)
            {
                HammerRot = MathHelper.Lerp(HammerRot, -90f, 0.1f);
                if (Timer > 10)
                {
                    HammerRot = MathHelper.Lerp(HammerRot, -30f, 0.1f);
                }

                position = Vector2.Lerp(position, targetPos, 0.3f);
                Scale = Vector2.Lerp(Scale, Vector2.One, 0.4f);
            }
            if (Timer > 23) Opacity *= 0.6f;
            if (Timer == 15)
            {
                position = Vector2.Lerp(position, ReliquaryUISystem.TradeState.StatueSlot.GetDimensions().Center(), 0.1f);
                HammerRot = 0f;
                SoundEngine.PlaySound(SoundID.Tink.WithPitchOffset(-0.5f + Pitch).WithPitchVariance(0.2f));
            }
            if (Timer > 30)
            {
                Kill();
            }
            Timer++;
        }
        public override void Draw()
        {
            Vector2 p1 = position;
            Vector2 p2 = position + new Vector2(16 * (Flip ? -1 : 1), -30).RotatedBy(Rotation);

            float rot = Rotation;
            float k = -90f - HammerRot;
            float rot2 = Rotation + MathHelper.ToRadians(k * (Flip ? -1 : 1));

            Asset<Texture2D> Chisel = Assets.Textures.Gallery.Sculptor.SculptorUIChisel.Asset;
            Asset<Texture2D> Hammer = Assets.Textures.Gallery.Sculptor.SculptorUIHammer.Asset;

            Main.EntitySpriteDraw(Chisel.Value, p1 + new Vector2(0, 4), Chisel.Frame(), Color.Black.MultiplyRGBA(new(Opacity, Opacity, Opacity, Opacity * 0.2f)), rot, new Vector2(3, 24), Scale * 1.2f, SpriteEffects.None);
            Main.EntitySpriteDraw(Hammer.Value, p2 + new Vector2(0, 4), Hammer.Frame(), Color.Black.MultiplyRGBA(new(Opacity, Opacity, Opacity, Opacity * 0.2f)), rot2, new Vector2(10, 20), Scale * 1.2f, Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            Main.EntitySpriteDraw(Chisel.Value, p1, Chisel.Frame(), Color.White.MultiplyRGBA(new(Opacity, Opacity, Opacity, Opacity)), rot, new Vector2(3, 24), Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(Hammer.Value, p2, Hammer.Frame(), Color.White.MultiplyRGBA(new(Opacity, Opacity, Opacity, Opacity)), rot2, new Vector2(10, 20), Scale, Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }

    public float Scale = 0f;

    public Item[] _itemArray;

    public int _itemIndex;

    public int _itemSlotContext;

    public SculptorStatueSlot(Item[] itemArray, int itemIndex, int itemSlotContext)
    {
        _itemArray = itemArray;
        _itemIndex = itemIndex;
        _itemSlotContext = itemSlotContext;
        Width = new StyleDimension(48f, 0f);
        Height = new StyleDimension(48f, 0f);
    }

    public void HandleItemSlotLogic()
    {
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            Item inv = _itemArray[_itemIndex];
            ItemSlot.OverrideHover(ref inv, _itemSlotContext);
            ItemSlot.LeftClick(ref inv, _itemSlotContext);
            ItemSlot.RightClick(ref inv, _itemSlotContext);
            ItemSlot.MouseHover(ref inv, _itemSlotContext);
            _itemArray[_itemIndex] = inv;
        }
    }

    public Item StatueToObtain = new Item(ItemID.AngelStatue);
    public Vector2[] ChiselPoints = { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };
    public bool DrawChiselAnim = false;

    public void SetChiselPoints(float progress)
    {
        DrawChiselAnim = true;
        ChiselPoints[0] = new Vector2(0f, progress + Main.rand.NextFloat(-0.1f, 0.1f));
        ChiselPoints[1] = new Vector2(Main.rand.NextFloat(0.2f, 0.4f), progress + Main.rand.NextFloat(-0.1f, 0.1f));
        ChiselPoints[2] = new Vector2(Main.rand.NextFloat(0.6f, 0.8f), progress + Main.rand.NextFloat(-0.1f, 0.1f));
        ChiselPoints[3] = new Vector2(1f, progress + Main.rand.NextFloat(-0.1f, 0.1f));
        Scale += 0.15f;
        SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(1f).WithVolumeScale(0.5f));
    }

    public void SetItemFinal()
    {
        DrawChiselAnim = false;
        _itemArray[_itemIndex] = StatueToObtain;
    }

    public void StartChiseling(int statueToChiselTo)
    {
        SetChiselPoints(1f);
        DrawChiselAnim = true;
        StatueToObtain = new Item(statueToChiselTo);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        HandleItemSlotLogic();
        Item inv = _itemArray[_itemIndex];
        Item inv2 = StatueToObtain;
        Vector2 position = GetDimensions().Center();
        Asset<Texture2D> tex = Assets.Textures.Gallery.Sculptor.SculptorStatueSlot.Asset;
        Main.EntitySpriteDraw(tex.Value, position, tex.Frame(), Color.White, 0f, tex.Frame().Size() / 2f, 1f + Scale, SpriteEffects.None);
        if (inv != null)
        {

            var chiselTarget = ScreenspaceTargetPool.Shared.Rent(
                Main.instance.GraphicsDevice,
                (width, height) => (200, 200)
            );

            Main.spriteBatch.End(out var sb);

            using (chiselTarget.Scope(clearColor: Color.Transparent))
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null);

                Main.instance.DrawItem_GetBasics(inv, 0, out Texture2D tx, out Rectangle fr, out Rectangle gmFr);

                if (!DrawChiselAnim)
                    Main.EntitySpriteDraw(tx, new Vector2(100, 100), fr, Color.White, 0f, fr.Size() / 2f, 0.5f, SpriteEffects.None);
                else if (inv2 != null)
                {
                    Vector2 cen = new(100, 100);

                    Main.instance.DrawItem_GetBasics(inv2, 0, out Texture2D newItemTx, out Rectangle fr2, out Rectangle gmFr2);

                    List<Vector2> v2s1 = [];
                    List<Color> cols1 = [];
                    List<Vector2> tcs1 = [];

                    for (int i = 0; i < ChiselPoints.Length; i++)
                    {
                        Vector2 point = cen + ((ChiselPoints[i] - new Vector2(0.5f, 0.5f)) * (fr2.Size() / 2f));
                        v2s1.Add(point);
                        v2s1.Add(point with { Y = cen.Y + fr2.Height / 4f });
                        cols1.Add(Color.White);
                        cols1.Add(Color.White);
                        tcs1.Add(ChiselPoints[i]);
                        tcs1.Add(ChiselPoints[i] with { Y = 1f });
                    }

                    PrimitiveDrawing.DrawPrimitiveStrip2(v2s1, cols1, new(200, 200), newItemTx, tcs1);

                    List<Vector2> v2s = [];
                    List<Color> cols = [];
                    List<Vector2> tcs = [];

                    for (int i = 0; i < ChiselPoints.Length; i++)
                    {
                        Vector2 point = cen + ((ChiselPoints[i] - new Vector2(0.5f, 0.5f)) * (fr.Size() / 2f));
                        v2s.Add(point);
                        v2s.Add(point with { Y = cen.Y - fr.Height / 4f });
                        cols.Add(Color.White);
                        cols.Add(Color.White);
                        tcs.Add(ChiselPoints[i]);
                        tcs.Add(ChiselPoints[i] with { Y = 0f });
                    }

                    PrimitiveDrawing.DrawPrimitiveStrip2(v2s, cols, new(200, 200), tx, tcs);
                }
                Main.spriteBatch.End();
            }

            Main.spriteBatch.Begin(sb with { SamplerState = Main.DefaultSamplerState });

            Main.EntitySpriteDraw(chiselTarget.Target, position, chiselTarget.Target.Bounds, Color.White, 0f, chiselTarget.Target.Bounds.Center(), (1f + (Scale * 3f)) * 2f, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(sb);
        }

        SculptorTradeUIState.Layer.Draw();
    }
}