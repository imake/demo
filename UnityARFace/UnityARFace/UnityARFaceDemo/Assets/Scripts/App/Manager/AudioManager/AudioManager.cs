using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMono<AudioManager>
{
    #region Base
    protected override string ParentRootName
    {
        get
        {
            return AppObjConst.EngineSingletonGoName;
        }
    }

    protected override void New()
    {
        base.New();
    }
    #endregion

    /// <summary>
    /// 声音资源目录
    /// </summary>
    public const string AudioResourceDir = "Audio/";

    /// <summary>
    /// 音效缓存池
    /// </summary>
    private Dictionary<string, AudioClip> audioClipCacheDict = new Dictionary<string, AudioClip>();

    /// <summary>
    /// 背景声音播放组件
    /// </summary>
    private AudioSource bgmSource;

    /// <summary>
    /// 音效播放组件
    /// </summary>
    private AudioSource effectSource;

    /// <summary>
    /// 动态音效 播放组件
    /// </summary>
    private List<AudioSource> dynamicEffectSources = new List<AudioSource>();

    /// <summary>
    /// 动态音效 播放组件 游戏对象
    /// </summary>
    private GameObject dynamicEffectSourcesGo;

    /// <summary>
    /// 背景音量大小
    /// </summary>
    public float BGMVolume
    {
        get
        {
            return bgmSource.volume;
        }
        set
        {
            bgmSource.volume = value;
        }
    }

    /// <summary>
    /// 音效音量大小
    /// </summary>
    public float EffectVolume
    {
        get
        {
            return effectSource.volume;
        }
        set
        {
            effectSource.volume = value;
        }
    }

    /// <summary>
    /// 是否开启背景音乐
    /// </summary>
    private bool isOpenBGM;
    public bool IsOpenBGM
    {
        get
        {
            return isOpenBGM;
        }
        set
        {
            isOpenBGM = value;
            //PrefsUtil.WriteBool(PrefsKeyConst.AudioMgr_isOpenBGM, isOpenBGM);

            if (!bgmSource) return;

            bgmSource.enabled = isOpenBGM;
            if (value)
                bgmSource.Play();
            else
                bgmSource.Pause();
        }
    }

    /// <summary>
    /// 是否开启音效
    /// </summary>
    private bool isOpenEffect;
    public bool IsOpenEffect
    {
        get
        {
            return isOpenEffect;
        }
        set
        {
            isOpenEffect = value;
            //PrefsUtil.WriteBool(PrefsKeyConst.AudioMgr_isOpenEffect, isOpenEffect);
        }
    }

    public void Init()
    {
        //UnityEngine.Object.DontDestroyOnLoad(gameObject);

        gameObject.AddComponent<AudioListener>();

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.playOnAwake = false;
        effectSource.loop = false;

        dynamicEffectSourcesGo = new GameObject("DynamicAudioSource");
        dynamicEffectSourcesGo.transform.SetParent(gameObject.transform, false);

        InitAudioMode();
    }

    private void InitAudioMode()
    {
        isOpenBGM = PrefsUtil.ReadBool(PrefsKeyConst.AudioMgr_isOpenBGM, true);
        isOpenEffect = PrefsUtil.ReadBool(PrefsKeyConst.AudioMgr_isOpenEffect, true);
    }

    /// <summary>
    /// 加载音效缓存
    /// </summary>
    /// <param name="path"></param>
    public void AddAudioClipCache(string path)
    {
        ResourceBase resourceBase = Singleton<ResourcesManager>.Instance.GetResource(path, typeof(AudioClip), enResourceType.Sound, false, false);
        if (resourceBase == null)
        {
            Debug.LogWarning("加载音效缓存错误: " + path);
            return;
        }

        string name = resourceBase.Name;
        if (!audioClipCacheDict.ContainsKey(name))
        {
            audioClipCacheDict.Add(name, resourceBase.content as AudioClip);
        }
    }

    /// <summary>
    /// 获取声音时长
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetLength(string name)
    {
        AudioClip clip = null;
        if (audioClipCacheDict.TryGetValue(name, out clip))
        {
            return clip.length;
        }
        else
        {
            string path = AudioResourceDir + name;
            ResourceBase resourceBase = Singleton<ResourcesManager>.Instance.GetResource(path, typeof(AudioClip), enResourceType.Sound, false, false);
            if (resourceBase == null || !(resourceBase.content is AudioClip))
            {
                Debug.Log("获取音效时长错误: " + name);
                return 0.5f;
            }
            audioClipCacheDict.Add(name, resourceBase.content as AudioClip);
            return (resourceBase.content as AudioClip).length;
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayBGM(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        string curName;
        if (bgmSource.clip == null)
        {
            curName = null;
        }
        else
        {
            curName = bgmSource.clip.name;
        }

        if (curName != name)
        {
            AudioClip audioClip = null;
            if (audioClipCacheDict.ContainsKey(name))
            {
                audioClip = audioClipCacheDict[name];
            }
            else
            {
                string path = AudioResourceDir + name;
                audioClip = Singleton<ResourcesManager>.Instance.GetResource(path, typeof(AudioClip), enResourceType.Sound, false, false).content as AudioClip;
                audioClipCacheDict.Add(name, audioClip);
            }

            if (audioClip != null)
            {
                bgmSource.clip = audioClip;
                if (isOpenBGM)
                {
                    bgmSource.Play();
                }
            }
        }
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopBgm()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isLoop"></param>
    public void PlayEffect(string name,bool isLoop = false)
    {
        if (!isOpenEffect || string.IsNullOrEmpty(name))
        {
            return;
        }

        string curName;
        AudioClip audioClip = null;
        if (effectSource.clip == null)
        {
            curName = null;
        }
        else
        {
            curName = effectSource.clip.name;
        }

        if (curName != name)
        {
            if (audioClipCacheDict.ContainsKey(name))
            {
                audioClip = audioClipCacheDict[name];
            }
            else
            {
                string path = AudioResourceDir + name;
                audioClip = Singleton<ResourcesManager>.Instance.GetResource(path, typeof(AudioClip), enResourceType.Sound, false, false).content as AudioClip;
                audioClipCacheDict.Add(name, audioClip);
            }
        }

        if (audioClip)
        {
            effectSource.loop = isLoop;
            effectSource.clip = audioClip;
            effectSource.Play();
        }
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    public void StopEffect()
    {
        effectSource.Stop();
        effectSource.clip = null;
    }

    /// <summary>
    /// 播放动态音效
    /// </summary>
    public void PlayDynamicEffect(string audioName, float delay = 0)
    {
        if (!isOpenEffect || string.IsNullOrEmpty(audioName))
        {
            return;
        }

        AudioSource effectSourceCom = null;
        for (int i = 0; i < dynamicEffectSources.Count; i++)
        {
            AudioSource sourceItem = dynamicEffectSources[i];
            if (!sourceItem.isPlaying)
            {
                effectSourceCom = sourceItem;
                break;
            }
        }
        if (effectSourceCom == null)
        {
            effectSourceCom = dynamicEffectSourcesGo.AddComponent<AudioSource>();
            effectSourceCom.playOnAwake = false;
            effectSourceCom.loop = false;
            dynamicEffectSources.Add(effectSourceCom);
        }

        //当前音乐
        string curName;
        AudioClip currClip = null;
        if (effectSourceCom.clip == null)
        {
            curName = null;
        }
        else
        {
            currClip = effectSourceCom.clip;
            curName = currClip.name;
        }

        if (curName != audioName)
        {
            string path = AudioResourceDir + audioName;
            if (audioClipCacheDict.ContainsKey(audioName))
            {
                currClip = audioClipCacheDict[audioName];
            }
            else
            {
                currClip = Singleton<ResourcesManager>.Instance.GetResource(path, typeof(AudioClip), enResourceType.Sound, false, false).content as AudioClip;
                audioClipCacheDict.Add(audioName, currClip);
            }
        }

        //播放
        if (currClip != null)
        {
            effectSourceCom.clip = currClip;
            if (delay == 0)
            {
                effectSourceCom.Play();
            }
            else
            {
                effectSourceCom.PlayDelayed(delay);
            }
        }
    }
}
