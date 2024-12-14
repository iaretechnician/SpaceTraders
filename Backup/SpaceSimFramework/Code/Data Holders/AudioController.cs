using System;
using UnityEngine;

[CreateAssetMenu(menuName = "DataHolders/AudioController")]
public class AudioController : SingletonScriptableObject<AudioController> {

    [Header("UI Audio")]
    public AudioClip ScrollSound;
    public AudioClip SelectSound;

    [Header("Game Audio")]
    public AudioClip GunSound;
    public AudioClip EngineSound;
    public AudioClip SmallImpact;
    public AudioClip HardImpact;

}
