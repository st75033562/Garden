using UnityEngine;


// use of ParticleEmitter
#pragma warning disable 618

namespace Gameboard
{
    public class ObjectActionPlayParticleSystem : ObjectAction
    {
        private ParticleSystem[] m_particleSystems;
        private ParticleEmitter[] m_particleEmitters;

        void Awake()
        {
            m_particleSystems = GetComponentsInChildren<ParticleSystem>();
            m_particleEmitters = GetComponentsInChildren<ParticleEmitter>();

            Stop();
        }

        public override void Execute(object o, params string[] args)
        {
            Stop();

            foreach (var ps in m_particleSystems)
            {
                ps.Play();
            }
            foreach (var pe in m_particleEmitters)
            {
                pe.emit = true;
            }
        }

        public override void Stop()
        {
            foreach (var ps in m_particleSystems)
            {
                ps.Stop();
            }
            foreach (var pe in m_particleEmitters)
            {
                pe.emit = false;
            }
        }
    }
}
