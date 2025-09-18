using UnityEngine;
using UnityEngine.Audio;

namespace DeadWrongGames.ZServices.Audio
{
    [CreateAssetMenu(fileName = "SoundDataSO", menuName = "Scriptable Objects/SoundDataSO")]
    public class SoundDataSO : ScriptableObject
    {
        public AudioClip[] Clips;
        public AudioMixerGroup MixerGroup;
        public float PitchMin = 1f;
        public float PitchMax = 1f;
    }
}
