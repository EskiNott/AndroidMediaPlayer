using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class UIController : EskiNottToolKit.MonoSingleton<UIController>
{
    public GameObject MainMenu;
    public GameObject Player;
    public TMPro.TextMeshProUGUI PlayPauseText;
    public TMPro.TextMeshProUGUI VideoTitle;
    public bool fileChoose = false;
    public GameObject PlayerController;
    public TMPro.TextMeshProUGUI DebugText;
    public Transform pos2;
    public Transform pos1;
    public Transform NowProgress;

    private void LateUpdate()
    {
        _videoCheck();
        ProgressUpdate();
    }

    private void ProgressUpdate()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        if (p.frameCount <= 0) return;
        float realProgress = p.frame * 1.0f / (long)p.frameCount;
        NowProgress.position = new Vector3(pos1.position.x + realProgress * (pos2.position.x - pos1.position.x), pos2.position.y, pos2.position.z);
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

    private void _videoCheck()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        sentLog(p.frame.ToString());
        sentLog(p.isPlaying.ToString());
        //½áÊø
        if(p.frame > (long)p.frameCount - 3 
            && fileChoose == true
            && p.frame != -1
            && p.frameCount != 0)
        {
            PlayerStop();
        }
    }

    public void OnPlayerScreenClick()
    {
        PlayerController.SetActive(!PlayerController.activeSelf);
    }

    public void PlayPauseButtonOnClick()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        if (p.isPlaying)
        {
            VideoPause();
        }
        else if (p.isPaused)
        {
            VideoPlay();
        }
    }

    public void VideoPlay()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.Play();
        PlayPauseText.text = "Pause";
    }

    public void VideoPause()
    {
        VideoPlayer p = FileManager.Instance.videoPlayer;
        p.Pause();
        PlayPauseText.text = "Play";
    }
}
