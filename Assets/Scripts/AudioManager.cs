using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource mainSource;

    // reference to audio source component
    public AudioClip[] pavementSounds;
    public AudioClip[] grassSounds;
    public AudioClip whooshSound;
    public AudioClip punchSound;

    // step variables
    private float stepTimer = 0f;
    private float currentStepInterval = 0.5f;

    // fade out sound length
    private float fadeDuration = 0.3f;

    private void Awake()
    {
        mainSource = GetComponent<AudioSource>();
        if (mainSource == null)
        {
            mainSource = gameObject.AddComponent<AudioSource>();
        }
    }
    // play random footstep sound
    public void PlayFootstep(bool isRunning, bool isOnGrass, Transform playerTransform)
    {
        // adjust interval based on whether the player is running or walking
        currentStepInterval = isRunning ? 0.3f : 0.4f;

        // randomly select footstep sound from the appropriate array
        AudioClip clip;
        if (isOnGrass)
        {
            clip = grassSounds[Random.Range(0, grassSounds.Length)];
        }
        else
        {
            clip = pavementSounds[Random.Range(0, pavementSounds.Length)];
        }

        // create new gameobject with an audio source to play the footsteps overlapping
        GameObject footstepObject = new GameObject("FootstepSound");
        footstepObject.transform.position = playerTransform.position;  

        // add an audio source component and configure it
        AudioSource footstepSource = footstepObject.AddComponent<AudioSource>();
        footstepSource.clip = clip;
        footstepSource.volume = 1f;

        if (footstepSource.clip != null)
        {
            footstepSource.Play();
        }
        else
        {
            Debug.LogError("AudioSource clip is null, cannot play sound.");
        }


        // start fading out the sound and destroy game object when done
        StartCoroutine(FadeOutAndDestroy(footstepSource, fadeDuration, footstepObject));
    }

    public bool CanPlayStep()
    {
        stepTimer += Time.deltaTime;
        if (stepTimer >= currentStepInterval)
        {
            stepTimer = 0f;
            return true;
        }
        return false;
    }

    private IEnumerator FadeOutAndDestroy(AudioSource source, float duration, GameObject footstepObject)
    {
        float startVolume = source.volume;

        // gradually decrease volume
        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        // stop the source and destroy the object
        source.Stop();
        Destroy(footstepObject);
    }

    public void PlayWhooshSound()
    {
        if (mainSource != null && whooshSound != null)
        {
            mainSource.PlayOneShot(whooshSound);
        }
        else
        {
            Debug.LogError("main source or whoosh sound clip missing");
        }
    }

    public void PlayPunchSound()
    {
        if (mainSource != null && punchSound != null)
        {
            mainSource.PlayOneShot(punchSound);
        }
        else
        {
            Debug.LogError("main source or punch sound clip missing");
        }
    }


}
