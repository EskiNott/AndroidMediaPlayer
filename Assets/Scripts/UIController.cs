using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class UIController : EskiNottToolKit.MonoSingleton<UIController>
{
    public GameObject MainMenu;
    public GameObject Player;
    public TMPro.TextMeshProUGUI PlayPauseText;
    public TMPro.TextMeshProUGUI VideoTitle;
    public bool fileChoose = false;
    public GameObject PlayerController;
    public TMPro.TextMeshProUGUI DebugText;
    public TMPro.TextMeshProUGUI SpeedText;
    public TMPro.TextMeshProUGUI TimeText;
    public Image NowProgress;
    private VideoSituation videoSitu;
    float[] speed = { 0.5f, 1.0f, 1.5f, 1.75f, 2.0f, 2.5f };
    int speedIndex = 1;

    private enum VideoSituation
    {
        playing,
        paused,
        end
    }

    private void Start()
    {
        _endCheck();
    }

    private void Update()
    {
        ProgressUpdate();
        TimeUpdate();
    }

    public void ScaleChangeButtonOnClick()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.aspectRatio = (VideoAspectRatio)(((int)p.aspectRatio + 1) % (int)VideoAspectRatio.Stretch);
    }

    public void SetPlaySpeedButtonOnClick()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        speedIndex = (speedIndex + 1) % speed.Length;
        SpeedText.text = "Multiple:" + speed[speedIndex] + "x";
        p.playbackSpeed = speed[speedIndex];
    }

    private void TimeUpdate()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;

        TimeSpan t1 = TimeSpan.FromSeconds(p.clockTime);
        TimeSpan t2 = TimeSpan.FromSeconds(p.length);
        string time1 = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        t1.Hours,
                        t1.Minutes,
                        t1.Seconds);
        string time2 = string.Format("{0:D2}:{1:D2}:{2:D2}",
                t2.Hours,
                t2.Minutes,
                t2.Seconds);
        TimeText.text = time1 + " | " + time2;
    }

    private void ProgressUpdate()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        if (p.frameCount <= 0) return;
        float realProgress = p.frame * 1.0f / (long)p.frameCount;
        NowProgress.fillAmount = realProgress;
    }

    public void PlayerPlay()
    {
        Player.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void PlayerStop()
    {
        Player.SetActive(false);
        MainMenu.SetActive(true);
        PlayerController.SetActive(false);
        fileChoose = false;
        VideoPause();
    }

    public void VideoReplay()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.frame = -1;
        PlayerPlay();
        VideoPlay();
    }

    public void sentLog(string logs)
    {
        DebugText.text = DebugText.text + "\n" + logs;
    }
    public void setTitle(string path)
    {
        string[] paths = path.Split('/');
        if(paths.Length > 0)
        {
            VideoTitle.text = paths[paths.Length - 1];
        }
    }

    private void _endCheck()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.loopPointReached += EndReach;

        //½áÊø
        void EndReach(VideoPlayer vp)
        {
            if(fileChoose == true)
            {
                videoSitu = VideoSituation.end;
                PlayPauseText.text = "Replay";
            }
        }
    }

    public void OnPlayerScreenClick()
    {
        PlayerController.SetActive(!PlayerController.activeSelf);
    }

    public void PlayPauseButtonOnClick()
    {
        switch (videoSitu)
        {
            case VideoSituation.playing:
                VideoPause();
                break;
            case VideoSituation.paused:
                VideoPlay();
                break;
            case VideoSituation.end:
                VideoPlay();
                break;
        }
    }

    public void VideoPlay()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        PlayPauseText.text = "Pause";
        videoSitu = VideoSituation.playing;
        p.Play();
    }

    public void VideoPause()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.Pause();
        PlayPauseText.text = "Play";
        videoSitu = VideoSituation.paused;
    }
}
