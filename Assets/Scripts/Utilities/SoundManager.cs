using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	public AudioSource musicSource;
	public static SoundManager instance = null;

	public float lowPitchRange = 0.95f, highPitchRange = 1.05f;
    bool mute;

	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
        mute = true;
	}

	public void PlaySoundEffect(AudioSource efxSource, params AudioClip[] clips){
        if (mute)
            return;
		int randomIndex = Random.Range (0, clips.Length);

		efxSource.volume = 1f;
		efxSource.PlayOneShot (clips [randomIndex]);
	}

	public void PlaySoundEffect(int distance, AudioSource efxSource, params AudioClip[] clips){
        if (mute)
            return;
        int randomIndex = Random.Range (0, clips.Length);

		if (distance > 12)
			efxSource.volume = 0f;
		else
			efxSource.volume = 1f / distance;

		efxSource.PlayOneShot (clips [randomIndex]);
	}

	public void PlaySoundEffectWithRandomPitch(AudioSource efxSource, params AudioClip[] clips){
        if (mute)
            return;
        float randomPitch = Random.Range (lowPitchRange, highPitchRange);
		efxSource.pitch = randomPitch;
		PlaySoundEffect (efxSource, clips);
	}

	public void PlaySoundEffectWithRandomPitch(int distance, AudioSource efxSource, params AudioClip[] clips){
        if (mute)
            return;
        float randomPitch = Random.Range (lowPitchRange, highPitchRange);
		efxSource.pitch = randomPitch;
		PlaySoundEffect (distance, efxSource, clips);
	}

	public void PlayClip(AudioClip clip){
        if (mute)
            return;
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 1f);
	}

	public void PlayClip(int distance, AudioClip clip){
        if (mute)
            return;
        if (distance > 12)
			return;
		else
			AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 1f / distance);
	}

	public void PlayClipFromList(params AudioClip[] clips){
        if (mute)
            return;
        int randomIndex = Random.Range (0, clips.Length);
		PlayClip(clips [randomIndex]);
	}

	public void PlayClipFromList(int distance, params AudioClip[] clips){
        if (mute)
            return;
        int randomIndex = Random.Range (0, clips.Length);
		if (distance > 12)
			return;
		else
			PlayClip(distance, clips [randomIndex]);
	}
}
