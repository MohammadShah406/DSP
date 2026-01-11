using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayAudio : MonoBehaviour, IPointerEnterHandler
{
    public string audioName;
    public AudioClip audioClick;

    public string audioHoverName;
    public AudioClip audioHover;

    private void Start()
    {
        if(audioClick != null) return;
        else
        {
            if(string.IsNullOrEmpty(audioName)) 
            {
                Debug.LogWarning("Audio name is not set for PlayAudio component on " + gameObject.name);
                return;
            }
            // Get the audio clip from the AudioLibrary
            audioClick = AudioLibrary.Instance.GetSfx(audioName);
        }

        if(audioHover != null) return;
        else
        {
            if(string.IsNullOrEmpty(audioHoverName)) 
            {
                Debug.LogWarning("Audio hover name is not set for PlayAudio component on " + gameObject.name);
                return;
            }
            // Get the audio clip from the AudioLibrary
            audioHover = AudioLibrary.Instance.GetSfx(audioHoverName);
        }

    }   

    public void PlayClickSound()
    {
        if (AudioPlayer.Instance == null || audioClick == null) return;
        // Play the specified audio clip
        AudioPlayer.Instance.Play(audioClick);
    }

    public void PlayHoverSound()
    {
        if (AudioPlayer.Instance == null || audioHover == null) return;
        // Play the specified audio clip
        AudioPlayer.Instance.Play(audioHover);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
        Debug.Log("Played sound on pointer enter: " + audioHoverName);
    }
}
