namespace Everware.Content.Underground.Glowcoat;

public class MagmaticGlowcoat : BaseGlowcoatItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/MagmaticGlowcoat";

    public override void SetColor()
    {
        Color = new(255, 90, 0);
    }
}
