using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public AudioClip titleSong;
    public AudioSource titleAudio;
    public float globalVolume;
    public bool isMuted;

    // Start is called before the first frame update
    void Start()
    {
        titleAudio = GetComponent<AudioSource>();
        titleAudio.clip = titleSong;
        titleAudio.volume = globalVolume;
        titleAudio.Play();
        isMuted = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void showCredits()
    {

    }

    public void exitGame()
    {

    }

    public void lowerVolume()
    {
        if (globalVolume > 0)
        {
            globalVolume -= .05f;
            titleAudio.volume = globalVolume;
        }
    }

    public void raiseVolume()
    {
        if (globalVolume < 1)
        {
            globalVolume += .05f;
            titleAudio.volume = globalVolume;
        }
    }

    public void toggleMute()
    {
        if (isMuted)
        {
            isMuted = false;
            titleAudio.mute = false;
        }
        else
        {
            isMuted = true;
            titleAudio.mute = true;
        }
    }
}
