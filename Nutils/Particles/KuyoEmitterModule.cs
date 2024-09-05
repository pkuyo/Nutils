using UnityEngine;

namespace Nutils.Particles
{
    public interface IEmitterModule
    {
        public void UpdateEmitter(KuyoParticleEmitter emitter);
    }


    public class SpawnModule : IEmitterModule
    {
        private IParticleValue<float> spawnRate;
        private float timeCost;
        public SpawnModule(IParticleValue<float> spawnRate) 
        {
            this.spawnRate = spawnRate;
        }

        public void UpdateEmitter(KuyoParticleEmitter emitter)
        {
            timeCost += 1 / 40f;
            if (timeCost > 1 / spawnRate.GetValue(emitter.LifeTime))
            {
                timeCost = 0;
                emitter.SpawnParticle();
            }

        }

    }

    public class BurstModule : IEmitterModule
    {
        private IParticleValue<float> spawnCount;
        private float timeDelay;
        private bool hasBurst = false;
        public BurstModule(IParticleValue<float> spawnCount,float timeDelay)
        {
            this.spawnCount = spawnCount;
            this.timeDelay = timeDelay;
        }

        public void UpdateEmitter(KuyoParticleEmitter emitter)
        {
            if (!hasBurst && emitter.LifeTime > timeDelay)
            {
                var value = spawnCount.GetValue(emitter.LifeTime);
                for (int i = 0; i < value; i++)
                    emitter.SpawnParticle();
                hasBurst = true;
            }
        }
    }

    public class DegEmitterModule : IEmitterModule
    {
        private KuyoParticleEmitter.ModifyEmitterDelegate deg;
        public DegEmitterModule(KuyoParticleEmitter.ModifyEmitterDelegate deg)
        {
            this.deg = deg;
        }
        public void UpdateEmitter(KuyoParticleEmitter emitter)
        {
            deg(emitter);
        }
    }

    public class BindPositionModule : IEmitterModule
    {

        private PhysicalObject obj;
        private int chunkIndex;
        private bool autoPaused;
        public BindPositionModule(PhysicalObject obj,int chunkIndex = 0, bool autoPaused = true)
        {
            this.obj = obj;
            this.chunkIndex = chunkIndex;
            this.autoPaused = autoPaused;
        }
        public void UpdateEmitter(KuyoParticleEmitter emitter)
        {
            if (obj?.room != emitter.room)
            {
                emitter.Die();
                return;
            }

            if (obj is Creature crit && autoPaused)
            {
                emitter.Paused = crit.inShortcut;
            }
            emitter.pos = obj.bodyChunks[chunkIndex].pos;
            emitter.lastPos = obj.bodyChunks[chunkIndex].lastPos;
            emitter.vel = obj.bodyChunks[chunkIndex].vel;
           
        }
    }
}
