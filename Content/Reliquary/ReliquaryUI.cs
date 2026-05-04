using Everware.Content.Gallery.Sculptor;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Everware.Content.Reliquary;

public class ReliquaryUISystem : ModSystem
{
    internal static UserInterface Interface;
    internal static ReliquaryUIState State;
    internal static SculptorTradeUIState TradeState;
    internal static NPC? Sculptor = null;

    public static bool ReliquaryOpenedFromInventory = false;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            Interface = new UserInterface();
            State = new ReliquaryUIState();
            TradeState = new SculptorTradeUIState();
            State.Activate();
            TradeState.Activate();
        }
    }
    public override void Unload()
    {
        State = null;
        TradeState = null;
    }
    private GameTime _lastUpdateUiGameTime;
    public override void PostUpdateInput()
    {
        if (Interface?.CurrentState != null)
        {
            PlayerInput.LockVanillaMouseScroll("Everware: Sculptor and Reliquary");
        }
    }
    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (Interface?.CurrentState != null)
        {
            Interface.Update(gameTime);

            if (!SculptorNearby(Main.LocalPlayer, out NPC guide, 100) && !ReliquaryOpenedFromInventory)
            {
                CloseUI();
            }
            else if (!Main.LocalPlayer.releaseInventory)
            {
                CloseUI();
            }
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Everware: Sculptor and Reliquary",
                delegate
                {
                    if (_lastUpdateUiGameTime != null && Interface?.CurrentState != null)
                    {
                        Interface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                    }
                    return true;
                },
                InterfaceScaleType.UI));
        }
    }


    public static void OpenUI()
    {
        Interface.SetState(State);
    }
    public static void OpenTrade(NPC? sculptor = null)
    {
        if (sculptor == null)
        {
            State.Position = Main.ScreenSize.ToVector2() / 2f;
        }
        Sculptor = sculptor;
        Interface.SetState(TradeState);
    }
    public static void CloseUI()
    {
        Interface.SetState(null);
        ReliquaryOpenedFromInventory = false;
    }

    public override void PostUpdateNPCs()
    {
        if (Sculptor != null)
        {
            TradeState.Position = Sculptor.Center + new Vector2(0, -120);
        }
    }

    public bool SculptorNearby(Player player, out NPC sculptor, float dist)
    {
        sculptor = null;

        bool b = false;
        float distance = dist;
        foreach (NPC npc in Main.npc)
        {
            if (npc.type == ModContent.NPCType<SculptorNPC>() && npc.Distance(player.Center) < distance)
            {
                sculptor = npc;
                distance = npc.Distance(player.Center);
                b = true;
            }
        }

        return b;
    }
}
public class ReliquaryUIState : UIState
{
    public UIPanel BigPanel;
    public Vector2 Position;

    public override void OnInitialize()
    {
        BigPanel = new UIPanel();
        BigPanel.Width.Set(1920 * 0.5f, 0f);
        BigPanel.Height.Set(1080 * 0.5f, 0f);
        BigPanel.HAlign = 0.5f;
        BigPanel.VAlign = 0.5f;
        Append(BigPanel);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }
}
public class SculptorTradeUIState : UIState
{
    public UIPanel BigPanel;
    public Vector2 Position;

    public override void OnInitialize()
    {
        BigPanel = new UIPanel();
        BigPanel.Width.Set(300, 0f);
        BigPanel.Height.Set(100, 0f);
        Append(BigPanel);
    }
    public override void Update(GameTime gameTime)
    {
        BigPanel.Left.Set(Position.X - 150 - Main.screenPosition.X, 0f);
        BigPanel.Top.Set(Position.Y - 50 - Main.screenPosition.Y, 0f);
    }
}
