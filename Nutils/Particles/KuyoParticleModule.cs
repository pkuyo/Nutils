using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nutils;
using RWCustom;
using UnityEngine;
using static Nutils.Particles.SimpleParticle;
using Random = UnityEngine.Random;

namespace Nutils.Particles
{
    public interface IParticleModule
    {
        public void ParticleFunction(SimpleParticle particle);
    }
    public interface IParticleInitModule : IParticleModule { }

    public interface IParticleUpdateModule : IParticleModule { }

    public interface IParticleDieModule : IParticleModule { }

    public interface IParticleNeedInitUpdateModule : IParticleUpdateModule { }


    public abstract class SimpleParModule
    {
        private readonly bool onlyRandAtSpawn;
        public int? NeedRandom(SimpleParticle particle) => !(onlyRandAtSpawn && particle.counter > 1) ? particle.RandomSeed : null;
        protected SimpleParModule(bool onlyRandAtSpawn) => this.onlyRandAtSpawn = onlyRandAtSpawn;

    }

    public class DegParticleModule : IParticleModule
    {
        private KuyoParticleEmitter.ModifyParticleDelegate deg;
        public DegParticleModule(KuyoParticleEmitter.ModifyParticleDelegate deg)
        {
            this.deg = deg;
        }
        public void ParticleFunction(SimpleParticle emitter)
        {
            deg(emitter);
        }
    }

    public class ChunksInitPosModule : IParticleInitModule
    {
        private readonly BodyChunk[] chunks;
        private readonly bool autoRad = false;
        private readonly bool onSurface;

        private readonly IParticleValue<float> radValue;
        private readonly IParticleValue<float> vel;

        public ChunksInitPosModule(IParticleValue<float> rad, IParticleValue<float> vel, bool autoRad = true, bool onSurface = false, params BodyChunk[] chunks)
        {
            this.autoRad = autoRad;
            this.radValue = rad;
            this.chunks = chunks;
            this.onSurface = onSurface;
            this.vel = vel;

        }

        public void ParticleFunction(SimpleParticle particle)
        {
            var index = Random.Range(0, chunks.Length);
            particle.pos = chunks[index].pos - particle.emitter.pos;
            var dir = Custom.RNV();
            particle.vel = vel.GetValue(particle.emitter.LifeTime) * dir;
            if (autoRad)
            {
                particle.pos += chunks[index].rad * (onSurface ? 1 : Random.value) *
                                radValue.GetValue(particle.emitter.LifeTime) * dir;
            }
            else
            {
                particle.pos += radValue.GetValue(particle.emitter.LifeTime) * (onSurface ? 1 : Random.value) * dir;
            }
            particle.lastPos = particle.pos;

        }
    }
    public class ChunkConnectionsInitPosModule : IParticleInitModule
    {
        private readonly PhysicalObject.BodyChunkConnection[] connections;
        private readonly bool autoRad = false;
        private readonly bool onSurface;
        private readonly IParticleValue<float> radValue;
        private readonly IParticleValue<float> vel;

        public ChunkConnectionsInitPosModule(IParticleValue<float> rad, IParticleValue<float> vel, bool autoRad = true, bool onSurface = false, params PhysicalObject.BodyChunkConnection[] connections)
        {
            this.autoRad = autoRad;
            this.radValue = rad;
            this.connections = connections;
            this.onSurface = onSurface;
            this.vel = vel;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            var index = Random.Range(0, connections.Length);
            var lerp = Random.value; 
            particle.pos = Vector2.Lerp(connections[index].chunk1.pos, connections[index].chunk2.pos, lerp) - particle.emitter.pos;
            var dir = Custom.RNV();
            particle.vel = vel.GetValue(particle.emitter.LifeTime) * dir;
            if (autoRad)
            {
                particle.pos += Mathf.Lerp(connections[index].chunk1.rad , connections[index].chunk2.rad, lerp) * (onSurface ? 1 : Random.value) *
                                radValue.GetValue(particle.emitter.LifeTime) * dir;
            }
            else
            {
                particle.pos += radValue.GetValue(particle.emitter.LifeTime) * (onSurface ? 1 : Random.value)  * dir;
            }
            particle.lastPos = particle.pos;
        }
    }

    public class CircleInitPosModule : IParticleInitModule
    {
        private readonly IParticleValue<float> rad;
        private readonly IParticleValue<float> vel;

        private readonly bool onSurface;

        public CircleInitPosModule(IParticleValue<float> rad, bool onSurface, IParticleValue<float> vel)
        {
            this.rad = rad;
            this.vel = vel;
            this.onSurface = onSurface;
        }
        public void ParticleFunction(SimpleParticle particle)
        {
            var r = rad.GetValue(particle.emitter.LifeTime);
            particle.pos = particle.lastPos = (onSurface ? r : Random.Range(0, r)) * Custom.RNV();
            particle.vel = particle.pos.normalized * vel.GetValue(particle.emitter.LifeTime);
        }
    }
    public class InitVelocityModule : IParticleInitModule
    {
        private readonly IParticleValue<Vector2> velocity;

        public InitVelocityModule(IParticleValue<Vector2> velocity)
        {
            this.velocity = velocity;
        }
        public void ParticleFunction(SimpleParticle particle)
        {
            particle.vel = velocity.GetValue(particle.emitter.LifeTime);
        }
    }

    public class InitScaleModule : IParticleInitModule
    {
        private readonly IParticleValue<Vector2> scale;

        private readonly bool quad;

        public InitScaleModule(IParticleValue<Vector2> scale,bool quad)
        {
            this.scale = scale;
            this.quad = quad;
        }
        public void ParticleFunction(SimpleParticle particle)
        {
            var scale = this.scale.GetValue(particle.emitter.LifeTime);
            particle.lastScale.y = particle.scale.x = scale.x;
            particle.lastScale.x = particle.scale.y = quad ? scale.x : scale.y;
        }
    }

    public class InitElementModule : IParticleInitModule
    {
        private readonly FAtlasElement element;
        private readonly FShader shader;
        private readonly int index;
        private readonly SimpleParticle.SpriteData data;

        public InitElementModule(string elementName,string shaderName = null,SpriteData spriteData = null, int index = 0)
        {
            element = Futile.atlasManager.GetElementWithName(elementName);
            this.index = index;
            data = spriteData;
            if (shaderName != null && !Custom.rainWorld.Shaders.ContainsKey(shaderName))
            {
                Plugin.LogError($"Shader Not found : {shaderName}");
                shader = FShader.defaultShader;
                return;
            }
            shader = shaderName == null ? FShader.defaultShader : Custom.rainWorld.Shaders[shaderName];
        }
        public InitElementModule(string elementName, int index)
        {
            element = Futile.atlasManager.GetElementWithName(elementName);
            shader = FShader.defaultShader;
            this.index = index;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.shaders[index] = shader;
            particle.elements[index] = element;
            particle.spriteDatas[index] = data ?? particle.spriteDatas[index];
        }
    }



    public class InitRotationModule : IParticleInitModule
    {
        public InitRotationModule(IParticleValue<float> rotation)
        {
            this.rotation = rotation;
        }
        private readonly IParticleValue<float> rotation;

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.lastRotation = particle.rotation = this.rotation.GetValue(particle.emitter.LifeTime);
        }
    }
    public class InitColorModule : IParticleInitModule
    {
        public InitColorModule(IParticleValue<Color> color)
        {
            this.color = color;
        }
        private readonly IParticleValue<Color> color;

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.lastColor = particle.color = color.GetValue(particle.emitter.LifeTime);
        }
    }

    public class LifeModule : IParticleInitModule
    {
        private readonly IParticleValue<float> life;
        public LifeModule(IParticleValue<float> life) 
        {
            this.life = life;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.maxLife = life.GetValue(particle.emitter.LifeTime);

        }
    }
    public class ScaleRatioOverLifeModule : SimpleParModule, IParticleNeedInitUpdateModule
    {
        private readonly IParticleValue<Vector2> scale;

        private readonly bool quad;
        public ScaleRatioOverLifeModule(IParticleValue<Vector2> scale, bool quad, bool onlyRandAtSpawn = false) : base(onlyRandAtSpawn)
        {
            this.scale = scale;
            this.quad = quad;
        }
        public void ParticleFunction(SimpleParticle particle)
        {
            var scale = this.scale.GetValue(particle.life, NeedRandom(particle));
            particle.scale.x = scale.x * particle.initScale.x;
            particle.scale.y = quad ? scale.x * particle.initScale.x : scale.y * particle.initScale.y;
        }
    }

    public class ColorRatioOverLifeModule : SimpleParModule, IParticleNeedInitUpdateModule
    {
        private readonly IParticleValue<Color> color;

        public ColorRatioOverLifeModule(IParticleValue<Color> color, bool onlyRandAtSpawn = false) : base(onlyRandAtSpawn)
        {
            this.color = color;
        }
        public void ParticleFunction(SimpleParticle particle)
        {
            particle.color = color.GetValue(particle.life, NeedRandom(particle)) * particle.initColor;
        }
    }

    public class AccelerationModule : SimpleParModule, IParticleUpdateModule
    {
        private readonly IParticleValue<Vector2> force;
        public AccelerationModule(IParticleValue<Vector2> force, bool onlyRandAtSpawn = false) : base(onlyRandAtSpawn)
        {
            this.force = force;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.vel += force.GetValue(particle.life, NeedRandom(particle)) / 40;

        }
    }
    public class ResistanceModule : SimpleParModule, IParticleUpdateModule
    {
        private readonly IParticleValue<float> resistance;
        public ResistanceModule(IParticleValue<float> resistance, bool onlyRandAtSpawn = false) : base(onlyRandAtSpawn)
        {
            this.resistance = resistance;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.vel *= resistance.GetValue(particle.life, NeedRandom(particle));

        }
    }

    public class LockRotationWithVel : IParticleUpdateModule
    {
        public void ParticleFunction(SimpleParticle particle)
        {
            particle.rotation = Custom.VecToDeg(particle.vel);
        }
    }

    public class ScaleRatioOverSpeed : IParticleUpdateModule
    {
        private readonly Vector2 maxScale;
        private readonly Vector2 scaleMulti;

        public ScaleRatioOverSpeed(Vector2 maxScale, Vector2 scaleMulti)
        {
            this.maxScale = maxScale;
            this.scaleMulti = scaleMulti;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.scale = new Vector2(1, 1) + scaleMulti * particle.vel.magnitude;
            particle.scale.x = Mathf.Min(particle.scale.x, maxScale.x);
            particle.scale.y = Mathf.Min(particle.scale.y, maxScale.y);

        }
    }
    public class RotationSpeedModule : SimpleParModule, IParticleUpdateModule
    {
        private readonly IParticleValue<float> speed;
        public RotationSpeedModule(IParticleValue<float> speed, bool onlyRandAtSpawn = true) : base(onlyRandAtSpawn)
        {
            this.speed = speed;
        }

        public void ParticleFunction(SimpleParticle particle)
        {
            particle.rotation += speed.GetValue(particle.life, NeedRandom(particle));
        }
    }
}
