using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[Define.Sound.GetNames(typeof(Define.Sound)).Length];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
	        for (int i = 0; i < soundNames.Length; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                _audioSources[i].spatialize = false;
                _audioSources[i].rolloffMode = AudioRolloffMode.Custom;
                go.transform.parent = root.transform;
            }
            _audioSources[(int)Define.Sound.Bgm].loop = true;
            _audioSources[(int)Define.Sound.Heartbeat].loop = true;
        }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
	        if(audioSource != null)
			{
				audioSource.clip = null;
				audioSource.Stop();
			}
        }
        _audioClips.Clear();
    }

    public void ResetVolume()
    {
	    foreach (AudioSource audioSource in _audioSources)
	    {
		    audioSource.volume = 1;
	    }
    }

    public void Play(string path, Define.Sound type = Define.Sound.Effects, AudioSource audioSource = null, float pitch = 1.0f)
    {
	    switch (type)
	    {
		    case Define.Sound.Bgm:
				path = String.Concat("Bgm/", path);
			    break;
		    case Define.Sound.Effects:
			    path = String.Concat("Effects/", path);
			    break;
		    case Define.Sound.Heartbeat:
			    path = String.Concat("Heartbeat/", path);
			    break;
	    }
	    AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, audioSource, pitch);
    }

	public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effects, AudioSource audioSource = null, float pitch = 1.0f)
	{
		if (audioClip == null)	
            return;

		if (audioSource == null)
		{
			switch (type)
			{
				case Define.Sound.Bgm:
				{
					audioSource = _audioSources[(int)Define.Sound.Bgm];
					if (audioSource.isPlaying && audioSource.clip == audioClip)
					{
						return;
					}
					if (audioSource.isPlaying){
						audioSource.Stop();
					}
					audioSource.pitch = pitch;
					audioSource.clip = audioClip;
					audioSource.Play();
					break;
				}
				case Define.Sound.Heartbeat:
				{
					audioSource = _audioSources[(int)Define.Sound.Heartbeat];
					if (audioSource == null)
					{
						return;
					}
					audioSource.pitch = pitch;
					audioSource.clip = audioClip;
					audioSource.Play();
					break;
				}
				case Define.Sound.Effects:
					audioSource = _audioSources[(int)Define.Sound.Effects];
					audioSource.pitch = pitch;
					audioSource.PlayOneShot(audioClip);
					break;
			}
		}
		else
		{
			audioSource.spatialize = true;
			audioSource.spatialBlend = 1.0f;
			audioSource.pitch = pitch;
			audioSource.PlayOneShot(audioClip);
		}
	}

	public void ChangeAudioVolume(Define.Sound type, float volume)
	{
		_audioSources[(int)type].volume = volume;
	}

	AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effects)
    {
		if (path.Contains("Sounds/") == false)
			path = $"Sounds/{path}";

		AudioClip audioClip = null;

		if (type == Define.Sound.Bgm || type == Define.Sound.Heartbeat)
		{
			audioClip = Managers.Resource.Load<AudioClip>(path);
		}
		else
		{
			if (_audioClips.TryGetValue(path, out audioClip) == false)
			{
				audioClip = Managers.Resource.Load<AudioClip>(path);
				_audioClips.Add(path, audioClip);
			}
		}

		if (audioClip == null)
			Debug.Log($"AudioClip Missing ! {path}");

		return audioClip;
    }
	
	public void ChangePitch(Define.Sound type, float pitch)
	{
		_audioSources[(int)type].pitch = pitch;
	}
	
	public void ChangePanStereo(Define.Sound type, float panValue)
	{
		_audioSources[(int)type].panStereo = panValue;
	}

	public IEnumerator FadeIn(Define.Sound type, string path, float fadeTime = 1.0f, float fadeDuration = 0.05f)
	{
		AudioSource audioSource = _audioSources[(int)type];
		float startVolume = 0f;
		float endVolume = 1f;//replace with sound setting
		float time = 0.0f;

		audioSource.volume = startVolume;
		Play(path, type);

		while (time < fadeTime)
		{
			float volume = (endVolume)*(time/fadeTime);
			audioSource.volume = volume;
			time += fadeDuration;
			yield return new WaitForSeconds(fadeDuration);
		}
		audioSource.volume = endVolume;
	}
	
	public IEnumerator FadeOut(Define.Sound type, float fadeTime = 0.5f, float fadeDuration = 0.05f)
	{
		AudioSource audioSource = _audioSources[(int)type];
		float endVolume = 0.0f;
		float startVolume = 1f; //replace with sound setting
		float time = 0.0f;

		while (time < fadeTime)
		{
			float volume = startVolume - (startVolume-endVolume)*(time/fadeTime);
			audioSource.volume = volume;
			time += fadeDuration;
			yield return new WaitForSeconds(fadeDuration);
		}
		audioSource.Stop();
		audioSource.volume = startVolume;
	}
	
	public void SetupKillerAudioSource()
	{
		_audioSources[(int)Define.Sound.Heartbeat] = Managers.Player.GetKillerGameObject().GetComponent<AudioSource>(); //킬러의 AudioSource 설정
	}
	
	public void Stop(Define.Sound type)
	{
		if(_audioSources[(int)type] != null){
			_audioSources[(int)type].Stop();
		}
	}

	/*public void PlayKillerBackground()
	{
		if (Managers.Player.IsMyDediPlayerKiller())
		{
			Managers.Sound.Play(string.Concat(Managers.Killer.GetKillerEnglishName()), Define.Sound.Bgm);
		}
		else
		{
			Managers.Sound.Play("tense-horror-background",Define.Sound.Bgm);
		}
	}*/
}
