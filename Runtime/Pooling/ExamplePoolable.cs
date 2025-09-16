using UnityEngine;

namespace DeadWrongGames.ZServices.Pooling
{
    public class ExamplePoolable : MonoBehaviour, IPoolable
    {
        public GameObject GameObject => gameObject;
        
        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public Component Component => GetComponent<AudioSource>();
    }
}