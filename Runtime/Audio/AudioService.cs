using UnityEngine;

namespace DeadWrongGames.ZServices.Audio
{
    public class AudioService : MonoBehaviour, IService
    {
        public abstract class SoundTypeMarker { }

        private void Awake()
        {
            ServiceLocator.Register(this);
        }
    }
}
