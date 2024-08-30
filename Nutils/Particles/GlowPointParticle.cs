using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace Nutils.Particles
{
    public class InitGlowPointElements : IParticleInitModule
    {
        public void ParticleFunction(SimpleParticle particle)
        {
            if (particle.spriteCount < 3)
                throw new ArgumentException("particle must has 3 or more sprites");
            particle.elements[0] = Futile.atlasManager.GetElementWithName("Circle20");
            particle.spriteDatas[0] = new SimpleParticle.SpriteData(0.05f);
            particle.shaders[0] = Custom.rainWorld.Shaders["Nutils.AdditiveDefault"];

            particle.elements[1] = Futile.atlasManager.GetElementWithName("Futile_White");
            particle.shaders[1] = Custom.rainWorld.Shaders["LightSource"];
            particle.spriteDatas[1] = new SimpleParticle.SpriteData(3);
            particle.spriteDatas[1].color = new Color(1, 1, 1, 0.2f);

            particle.elements[2] = Futile.atlasManager.GetElementWithName("Futile_White");
            particle.shaders[2] = Custom.rainWorld.Shaders["FlatLight"];
            particle.spriteDatas[2] = new SimpleParticle.SpriteData(1);
            particle.spriteDatas[2].color = new Color(1, 1, 1, 0.2f);
        }
    }
}
