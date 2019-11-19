using Gameboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayNetSound : BlockBehaviour
{
    private const string request = "http://tsn.baidu.com/text2audio?tex={0}&lan=zh&ctp=1&cuid=abcdxxx&tok={1}&aue=6&per=4";
    private AudioSource audios;
    private bool playingPause;
    private bool pauseState;
    // Use this for initialization
    protected override void Start () {
        base.Start();
        audios = gameObject.GetComponent<AudioSource>();
        if (audios == null)
        {
            audios = gameObject.AddComponent<AudioSource>();
        }
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        yield break;
    }

    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        pauseState = false;
        playingPause = false;
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        
        yield return GetBaiduToken(slotValues[0], float.Parse(slotValues[1]) / 100.0f, (str) =>
        {
            retValue.value = str;
        });
        yield break;
    }

    IEnumerator GetBaiduToken(string content, float volume, Action<string> result)
    {
        string requestUrl = string.Format(request, content, UserManager.Instance.baiduToken);
        UnityWebRequest webRequest = UnityWebRequest.GetAudioClip(requestUrl, AudioType.WAV);
        webRequest.timeout = 3;
        yield return webRequest.Send();
        if (webRequest.isError)
        {
            result("false");
        }
        else
        {

            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
            audios.clip = audioClip;
            audios.volume = volume;
            if (pauseState)
            {
                playingPause = true;
            }
            else
            {
                audios.Play();
            }
            
            if (audios.isPlaying)
            {
                result("true");
            }
            else {
                result("false");
            }
            
            if (!UIGameboard.instance.netAudioSources.Contains(this)) {
                UIGameboard.instance.netAudioSources.Add(this);
            }
        }
        yield break;
    }

    protected override void OnDestroy()
    {
        StopSound();
    }

    public void StopSound() {
        if (audios != null)
        {
            audios.Stop();
        }
    }

    public void PauseSound(bool paused) {
        pauseState = true;
        if (paused && audios.isPlaying)
        {
            playingPause = true;
            audios.Pause();
        }
        else if(playingPause)
        {
            playingPause = false;
            audios.Play();
        }

        
    }
}
