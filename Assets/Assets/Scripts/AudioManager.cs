using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pusoy
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        [SerializeField] private AudioSource _musicAudioSource;
        [SerializeField] private AudioSource _sfxAudioSource;
        private void Awake()
        {
            _instance = this;
        }

        public void PlaySfx(AudioClip audioClip, float volume = 1)
        {
            _sfxAudioSource.PlayOneShot(audioClip, volume);
        }
    }
}
