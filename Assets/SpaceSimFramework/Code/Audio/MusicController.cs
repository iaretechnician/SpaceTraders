using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace SpaceSimFramework
{
public class MusicController : Singleton<MusicController> {

    private enum Track
    {
        Station, Ambient, Battle
    }

    // TWo sources are needed for crossfading
    public AudioSource Source1, Source2, Effects;
    public AudioClip Ambient, Battle, Station;
    [Tooltip("How often the Music Controller will check the distance between the player ship and" +
        "the closest enemy ship to change music")]
    public float CheckInterval = 2f;
    [Tooltip("Distance below which the Battle music replaces Ambient music")]
    public int BattleDistanceThreshold = 1000;
    public float CrossfadeDuration = 3f;

    private float _checkTimer = 0f;
    private Track _currentTrack = Track.Ambient;
    private bool _isSwitching = false;

    private void Start()
    {
        _checkTimer = CheckInterval;
        // All clear, chill out mode
        StartCoroutine(SwitchTrack(Ambient, Track.Ambient));
    }

    private void Update()
    {
        _checkTimer -= Time.deltaTime;
        if(_checkTimer < 0)
        {
            _checkTimer = CheckInterval;          
            if (Ship.PlayerShip == null)
                return;

            if (Ship.PlayerShip.StationDocked != "none" && _currentTrack != Track.Station)
            {
                StartCoroutine(SwitchTrack(Station, Track.Station));
            }
            if (Ship.PlayerShip.StationDocked == "none")
            {
                // Check distance to closest enemy ship
                Vector3 playerPosition = Ship.PlayerShip.transform.position;
                var enemies = SectorNavigation.Instance.GetClosestEnemyShip(Ship.PlayerShip.transform, 5000);
                if (enemies.Count == 0 && _currentTrack != Track.Ambient)
                {
                    // No enemy in range.
                    StartCoroutine(SwitchTrack(Ambient, Track.Ambient));
                }
                if (enemies.Count > 0){
                    var closestEnemyDist = Vector3.Distance(playerPosition, enemies[0].transform.position);

                    if (_currentTrack != Track.Ambient && closestEnemyDist > BattleDistanceThreshold)
                        // All clear, chill out mode
                        StartCoroutine(SwitchTrack(Ambient, Track.Ambient));
                    if (_currentTrack != Track.Battle && closestEnemyDist < BattleDistanceThreshold)
                        // Danger close, start war drums
                        StartCoroutine(SwitchTrack(Battle, Track.Battle));
                }
            }
        }
    }

    private IEnumerator SwitchTrack(AudioClip clipToPlay, Track track)
    {
        if (_isSwitching)
            yield return null;

        _isSwitching = true;
        AudioSource newSource, oldSource;
        _currentTrack = track;

        if(Source1.volume == 1.0)
        {
            newSource = Source2;
            oldSource = Source1;
        }
        else
        {
            newSource = Source1;
            oldSource = Source2;
        }

        newSource.clip = clipToPlay;
        newSource.Play();
        newSource.volume = 0;

        while(newSource.volume < 1.0)
        {
            newSource.volume += Time.deltaTime / CrossfadeDuration;
            if (oldSource.isPlaying && oldSource.volume >= 0)
                oldSource.volume -= Time.deltaTime / CrossfadeDuration;

            yield return null;
        }

        oldSource.Stop();
        newSource.volume = 1.0f;
        _isSwitching = false;

        // Crossfade
        yield return null;
    }

    public void PlaySound(AudioClip clip)
    {
        Effects.PlayOneShot(clip);
    }

}
}