using System.Collections.Generic;

namespace Everware.Content.Base.ParticleSystem;

public abstract class BaseParticleLayer
{
    public List<Particle> AllParticles = [];
    public float Scale = 1f;

    public void Draw()
    {
        for (int i = 0; i < AllParticles.Count; i++)
        {
            AllParticles[i].Draw();
        }
    }

    public void Update()
    {
        for (int i = 0; i < AllParticles.Count; i++)
        {
            AllParticles[i].Update();
        }
    }
}

public class ParticleLayer : BaseParticleLayer
{
    public ParticleLayer() { }
    public ParticleLayer(float sc)
    {
        Scale = sc;
    }
}
