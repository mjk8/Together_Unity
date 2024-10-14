using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EffectsManager : MonoBehaviour
{
    Dictionary <string, PostProcessVolume> _postProcessVolumes = new Dictionary<string, PostProcessVolume>();

    /// <summary>
    /// Effects Manager의 Init. InGameScene으로 전환 후 호출할 것.
    /// </summary>
    public void Start()
    {
        _eyesClosed = false;
        Managers.Effects = this;
        PostProcessVolume[] postProcessVolumes = GameObject.Find("PPVolumes").GetComponentsInChildren<PostProcessVolume>();
        foreach (PostProcessVolume postProcessVolume in postProcessVolumes)
        {
            _postProcessVolumes.Add(postProcessVolume.gameObject.name, postProcessVolume);
            _postProcessVolumes[postProcessVolume.gameObject.name].weight = 0;
        }
    }

    /// <summary>
    /// Detector킬러가 걸렸을 때 활성화
    /// </summary>
    public void DetectorPPEnable()
    {
        _postProcessVolumes["DetectorPP"].weight = 1;
    }
    
    /// <summary>
    /// Detector킬러가 해제되었을 때 비활성화
    /// </summary>
    public void DetectorPPDisable()
    {
        _postProcessVolumes["DetectorPP"].weight = 0;
    }

    /// <summary>
    /// Detector에게 감지 당한 생존자의 효과 재생.
    /// </summary>
    public void DetectedPPPlay()
    {
        
        PostProcessVolume cur = _postProcessVolumes["DetectedPP"];
        DepthOfField depthOfField;
        if (!cur.profile.TryGetSettings(out depthOfField))
        {
            Debug.LogError("DepthOfField settings not found in the PostProcessVolume.");
            return;
        }
        cur.weight = 1;
        StartCoroutine(DetectedPPCoroutine(depthOfField));
    }

    /// <summary>
    /// 손전등에 당했을 때 효과. 푸는건 따로 있음.
    /// </summary>

    //만약 손전등 효과가 이미 실행중이면 다시 실행하면 안됨.
    bool _eyesClosed;
    public void FlashlightPPPlay()
    {
        //check if coroutine is already running
        if (_eyesClosed)
        {
            return;
        }

        PostProcessVolume cur = _postProcessVolumes["FlashlightPP"];
        Vignette vignette;
        //Bloom bloom;
        
        
        if (!cur.profile.TryGetSettings(out vignette))
        {
            Debug.LogError("Vignette settings not found in the PostProcessVolume.");
            return;
        }
        /*if (!cur.profile.TryGetSettings(out bloom))
        {
            Debug.LogError("Bloom settings not found in the PostProcessVolume.");
            return;
        }*/
        
        cur.weight = 1;
        _eyesClosed = true;
        //StartCoroutine(FlashlightStartCoroutine(vignette, bloom));
        StartCoroutine(FlashlightStartCoroutine(vignette));
    }
    
    /// <summary>
    /// 손전등 효과가 끝나면 이 함수를 호출.
    /// </summary>
    public void FlashlightPPStop()
    {
        _eyesClosed = false;
        PostProcessVolume cur = _postProcessVolumes["FlashlightPP"];
        Vignette vignette;
        
        if (!cur.profile.TryGetSettings(out vignette))
        {
            Debug.LogError("Vignette settings not found in the PostProcessVolume.");
            return;
        }
        StartCoroutine(FlashlightStopCoroutine(vignette));
    }
    
    /*
    /// <summary>
    /// Dash 아이템 효과 재생
    /// </summary>
    public void DashPPPlay(float duration)
    {
        
        PostProcessVolume cur = _postProcessVolumes["DashPP"];
        LensDistortion lensDistortion;
        if (!cur.profile.TryGetSettings(out lensDistortion))
        {
            Debug.LogError("DepthOfField settings not found in the PostProcessVolume.");
            return;
        }
        cur.weight = 1;
        StartCoroutine(DashPPCoroutine(lensDistortion,duration));
    }*/

    public void Clear()
    {
        _postProcessVolumes.Clear();
    }
    
    #region Specific Effect functions
    
    ///////// DetectedPP's Coroutine /////////
    float DetectedEffectDuration = 2.5f;
    float DetectedMaxDepthOfField = 5f;

    IEnumerator DetectedPPCoroutine(DepthOfField depth)
    {
        float currentTime=0;
        while (currentTime <= DetectedEffectDuration)
        {
            depth.focusDistance.value = DetectedMaxDepthOfField-(DetectedMaxDepthOfField * Mathf.Sin(Mathf.PI * (currentTime / DetectedEffectDuration)));
            currentTime+=Time.deltaTime;
            yield return null;
        }
        _postProcessVolumes["DetectedPP"].weight = 0;
    }

    ///////// FlashlightPPPlay's Coroutine /////////

    private float vignetteCloseIntensity = 0.31f; //also used in FlashlightPPStop
    private float vignetteCloseDuration = 0.8f;

    IEnumerator FlashlightStartCoroutine(Vignette vignette)
    {
        float currentTime=0;
        while (currentTime < vignetteCloseDuration)
        {
            vignette.intensity.value =  vignetteCloseIntensity * Mathf.Sin(Mathf.PI/2 * (currentTime / vignetteCloseDuration));
            currentTime+=Time.deltaTime;
            yield return null;
        }
    }
    
    ///////// FlashlightPPStop's Coroutine /////////
    
    private float flashlightStopDuration = 1.5f;
    
    IEnumerator FlashlightStopCoroutine (Vignette vignette)
    {
        float currentTime=0;
        while (currentTime < vignetteCloseDuration)
        {
            if (_eyesClosed)
            {
                yield break;
            }
            vignette.intensity.value =  (vignetteCloseIntensity* Mathf.Sin(Mathf.PI/2+(Mathf.PI/2 * (currentTime / vignetteCloseDuration))));
            currentTime+=Time.deltaTime;
            yield return null;
        }
        _postProcessVolumes["FlashlightPP"].weight = 0;
    }
    
    /*
    ///////// DashPP's Coroutine /////////
    
    IEnumerator DashPPCoroutine (LensDistortion lensDistortion, float duration)
    {
        float currentTime=0;
        float maxDistortion = 0.5f;
        float duration = 0.5f;
        while (currentTime < duration)
        {
            lensDistortion.intensity.value = maxDistortion * Mathf.Sin(Mathf.PI/2 * (currentTime / duration));
            currentTime+=Time.deltaTime;
            yield return null;
        }
        _postProcessVolumes["DashPP"].weight = 0;
    }*/
    #endregion
}
