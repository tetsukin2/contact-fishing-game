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
        // formatted this way as directly assigning value may cause panel to show up unintended
        if (_videoPlayer.clip == null)
        {
            Show(false);
        }
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
