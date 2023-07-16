using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
public class FileManager : EskiNottToolKit.MonoSingleton<FileManager>
{
    [SerializeField] public VideoPlayer videoPlayer;
    [SerializeField] public AudioSource audioSource;
    public bool isAudio = false;


    public void onLoadButtonClick()
    {
        PickMedia();
    }

    private void PickMedia()
    {
        NativeGallery.Permission permission = NativeGallery.GetMixedMediaFromGallery((path) =>
        {
            UnityEngine.Debug.Log("Media path: " + path);
            if (path != null)
            {
                switch (NativeGallery.GetMediaTypeOfFile(path))
                {
                    case NativeGallery.MediaType.Video:
                        isAudio = false;
                        videoPlayer.url = path;
                        UIController.Instance.PlayerPlay();
                        UIController.Instance.MediaPlay();
                        UIController.Instance.fileChoose = true;
                        UIController.Instance.setTitle(path);
                        // Play the selected video
                        //Handheld.PlayFullScreenMovie("file://" + path);
                        break;
                    case NativeGallery.MediaType.Audio:
                        isAudio = true;
                        //UIController.Instance.SetSongTitleAndAblum(path);
                        InitialAudioPlay(path);
                        break;
                    default: UnityEngine.Debug.Log("Probably picked something else"); break;
                }
            }
        }, NativeGallery.MediaType.Audio | NativeGallery.MediaType.Video, "Select Media");
        UnityEngine.Debug.Log("Permission result: " + permission);
    }

    public void InitialAudioPlay(string path)
    {

        UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG);

        // �ֶ�����Web����
        audioRequest.SendWebRequest().completed += (operation) =>
        {
            if (!audioRequest.isNetworkError && !audioRequest.isHttpError)
            {
                // ��ȡ���ص���Ƶ�ļ�
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);

                // ����һ��AudioSource�����������Ƶ
                audioSource.clip = audioClip;
                UIController.Instance.PlayerPlay();
                UIController.Instance.MediaPlay();
                UIController.Instance.fileChoose = true;
                UIController.Instance.setTitle(path);
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to load audio: " + audioRequest.error);
            }

            audioRequest.Dispose();
        };
    }
}
