using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class UIController : EskiNottToolKit.MonoSingleton<UIController> {
    public GameObject MainMenu;
    public GameObject VideoPlayer;
    public GameObject AudioPlayer;
    public GameObject PlayerBottom;
    public TMPro.TextMeshProUGUI PlayPauseText;
    public TMPro.TextMeshProUGUI MediaTitle;
    public bool fileChoose = false;
    public bool isChosenAudio = false;
    public GameObject PlayerController;
    public TMPro.TextMeshProUGUI DebugText;
    public TMPro.TextMeshProUGUI SpeedText;
    public TMPro.TextMeshProUGUI TimeText;
    public Image NowProgress;
    private MediaSituation mediaSitu;
    float[] speed = { 0.5f, 1.0f, 1.5f, 1.75f, 2.0f, 2.5f };
    int speedIndex = 1;
    private bool isDraggingProgress = false;

    private enum MediaSituation
    {
        playing,
        paused,
        end
    }

    private void Start()
    {
        _videoEndCheck();
    }

    private void Update()
    {
        ProgressUpdate();
        TimeUpdate();
        if (Input.GetMouseButtonDown(0))
        {
            // 检查鼠标点击的位置是否在进度条上
            Vector2 mousePosition = Input.mousePosition;
            RectTransform progressBarRect = NowProgress.rectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(progressBarRect, mousePosition))
            {
                // 点击进度条时开始拖动
                isDraggingProgress = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 停止拖动
            isDraggingProgress = false;
        }

        // 当拖动进度条时才更新进度
        if (isDraggingProgress)
        {
            HandleMouseClick();
        }
    }

    public void ScaleChangeButtonOnClick()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.aspectRatio = (VideoAspectRatio)(((int)p.aspectRatio + 1) % (int)VideoAspectRatio.Stretch);
    }

    public void SetPlaySpeedButtonOnClick()
    {
        speedIndex = (speedIndex + 1) % speed.Length;
        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            p.pitch = speed[speedIndex];
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            p.playbackSpeed = speed[speedIndex];
        }
        SpeedText.text = "Multiple:" + speed[speedIndex] + "x";
        
    }

    private void TimeUpdate()
    {
        TimeSpan t1;
        TimeSpan t2;
        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            if (p.clip == null) return;
            t1 = TimeSpan.FromSeconds(p.time);
            t2 = TimeSpan.FromSeconds(p.clip.length);
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            t1 = TimeSpan.FromSeconds(p.clockTime);
            t2 = TimeSpan.FromSeconds(p.length);
        }
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
        float realProgress;
        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            if (p.clip == null) return;
            realProgress = p.time * 1.0f / p.clip.length;
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            if (p.frameCount <= 0) return;
            realProgress = p.frame * 1.0f / (long)p.frameCount;
        }
        NowProgress.fillAmount = realProgress;
    }

    public void PlayerPlay()
    {
        if (FileManager.Instance.isAudio)
        {
            AudioPlayer.SetActive(true);
            PlayerController.SetActive(true);
        }
        else
        {
            VideoPlayer.SetActive(true);
        }
        PlayerBottom.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void PlayerStop()
    {
        VideoPlayer.SetActive(false);
        AudioPlayer.SetActive(false);
        MainMenu.SetActive(true);
        PlayerBottom.SetActive(false);
        PlayerController.SetActive(false);
        fileChoose = false;
        MediaPause();
    }

    public void MediaReplay()
    {
        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            p.time = 0;
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            p.frame = -1;
        }

        PlayerPlay();
        MediaPlay();
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
            MediaTitle.text = paths[paths.Length - 1];
        }
    }

    private void _videoEndCheck()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.loopPointReached += EndReach;

        //结束
        void EndReach(VideoPlayer vp)
        {
            if(fileChoose == true)
            {
                mediaSitu = MediaSituation.end;
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
        switch (mediaSitu)
        {
            case MediaSituation.playing:
                MediaPause();
                break;
            case MediaSituation.paused:
                MediaPlay();
                break;
            case MediaSituation.end:
                MediaPlay();
                break;
        }
    }

    public void MediaPlay()
    {
        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            p.Play();
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            p.Play();
        }
        PlayPauseText.text = "Pause";
        mediaSitu = MediaSituation.playing;
    }

    public void MediaPause()
    {
        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            p.Pause();
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            p.Pause();
        }
        PlayPauseText.text = "Play";
        mediaSitu = MediaSituation.paused;
    }

    private void HandleMouseClick()
    {
        Vector2 localMousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(NowProgress.rectTransform, Input.mousePosition, null, out localMousePosition);

        // 获取点击位置相对于进度条的比例
        float normalizedPosition = Mathf.InverseLerp(NowProgress.rectTransform.rect.xMin, NowProgress.rectTransform.rect.xMax, localMousePosition.x);

        if (FileManager.Instance.isAudio)
        {
            AudioSource p = FileManager.Instance.audioSource;
            double targetTime = p.clip.length * normalizedPosition;
            p.time = (float)targetTime;
        }
        else
        {
            VideoPlayer p = FileManager.Instance.videoPlayer;
            double targetTime = p.length * normalizedPosition;
            p.time = targetTime;
        }
    }
    
    public void SetSongTitleAndAblum(string path)
   {
        var file = TagLib.File.Create(path);
        // 获取歌曲名
        string title = file.Tag.Title;
        UnityEngine.Debug.Log("Song Title: " + title);
        MediaTitle.text = title;
        // 获取专辑图片
        /*IPicture[] pictures = file.Tag.Pictures;
        if (pictures.Length > 0)
        {
            byte[] pictureData = pictures[0].Data.Data;
            Texture2D albumArt = new Texture2D(2, 2);
            albumArt.LoadImage(pictureData);
            // 在此处使用专辑图片（Texture2D）
        }*/
   }
}
