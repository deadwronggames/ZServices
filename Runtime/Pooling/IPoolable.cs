using UnityEngine;

namespace DeadWrongGames.ZServices.Pooling
{
    public interface IPoolable
    {
        // things to pool (AudioSource, ParticleSystem, ...) always live on GOs. Reference is needed for instantiating, setting-active and destroying.
        GameObject GameObject { get; }
        void Release();
    }
}