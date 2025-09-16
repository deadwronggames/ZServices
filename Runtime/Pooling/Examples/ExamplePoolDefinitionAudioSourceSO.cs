using System;
using UnityEngine;

namespace DeadWrongGames.ZServices.Pooling
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Pooling/PoolDefinitionAudioSource", fileName = "PoolDefinitionAudioSource")]
    public class ExamplePoolDefinitionAudioSourceSO : BasePoolDefinitionSO
    {
        // For every different inheritor: Change generic type in GetComponent! 
        // Optional: Override any of the pool creation methods.
        protected override Func<GameObject, Component> ComponentFactory => go => go.GetComponent<AudioSource>();

        protected override void ActionOnRelease(Component poolable)
        {
            base.ActionOnRelease(poolable);
            poolable.gameObject.GetComponent<AudioSource>().Stop();
        }
    }
}