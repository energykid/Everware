using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Everware.Content.Reliquary;

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