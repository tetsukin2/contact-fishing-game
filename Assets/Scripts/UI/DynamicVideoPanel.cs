using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DynamicVideoPanel : GUIPanel
{
    [SerializeField] private VideoPlayer _videoPlayer;

    public void Awake()
    {
        // Hide the panel at the start if nothing's assigned to it
        Show(_videoPlayer.clip != null);
    }

    public void SetVideo(VideoClip videoClip)
    {
        _videoPlayer.Stop();
        if (videoClip == null)
        {
            _content.SetActive(false);
            return;
        }
        _content.SetActive(true);
        _videoPlayer.clip = videoClip;
        _videoPlayer.Play();
    }
}
