using UnityEngine;
using UnityEngine.Video;

public class DynamicVideoPanel : GUIContainer
{
    [SerializeField] private VideoPlayer _videoPlayer;

    public void SetVideo(VideoClip videoClip)
    {
        if (!ContentActive)
            return;
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
