using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using TagLib;

public class UIController : EskiNottToolKit.MonoSingleton<UIController> {
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
    public AudioSource audioSource;
    public GameObject AudioController;
    public TMPro.TextMeshProUGUI SongTitle;
    private VideoSituation videoSitu;
    float[] speed = { 0.5f, 1.0f, 1.5f, 1.75f, 2.0f, 2.5f };
    int speedIndex = 1;
    private bool isDraggingProgress = false;

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

        //结束
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

    private void HandleMouseClick()
    {
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(NowProgress.rectTransform, Input.mousePosition, null, out localMousePosition);

        // 获取点击位置相对于进度条的比例
        float normalizedPosition = Mathf.InverseLerp(NowProgress.rectTransform.rect.xMin, NowProgress.rectTransform.rect.xMax, localMousePosition.x);

        // 计算点击位置对应的视频播放时间
        VideoPlayer p = FileManager.Instance.videoPlayer;
        double targetTime = p.length * normalizedPosition;

        // 调整视频播放进度到目标时间
        p.time = targetTime;
    }
    
    public void SetSongTitleAndAblum(string path)
   {
       using (var file = TagLib.File.Create(path))
       {
           // 获取歌曲名
           string title = file.Tag.Title;
           Debug.Log("Song Title: " + title);
           SongTitle.text = title;
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
  
    public void InitialAudioPlay(string path)
    {
        MainMenu.SetActive(false);

        UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG);

        // 手动发送Web请求
        audioRequest.SendWebRequest().completed += (operation) =>
        {
            if (!audioRequest.isNetworkError && !audioRequest.isHttpError)
            {
                // 获取加载的音频文件
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);

                // 创建一个AudioSource组件并播放音频
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio: " + audioRequest.error);
            }

            audioRequest.Dispose();
        };
    }
}
