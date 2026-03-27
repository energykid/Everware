namespace Everware.Content.Base.World;

public abstract class GenComponent
{
    public Point origin;
    public virtual void RunGen(Point p) { }
}