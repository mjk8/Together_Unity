using System;
using System.Collections;
using UnityEngine;

public class LobbyDayCycleEffect : MonoBehaviour
{
    public MeshRenderer _waterMeshRenderer;
    public Material _daySkybox;
    public Material _nightSkybox;
    public Light _directionalLight;
    
    
    //해 쨍쨍 값
    private float _dayWaterMetallic = 0;
    private Color _dayWaterDepthColor = new Color(0.01568627f, 0.4196078f, 0.6313725f); // 046BA1
    private Color _dayFogColor = new Color(0.8f, 0.9254902f, 1f); // CCECFF
    private float _dayFogIntensity = 0;
    private Color _dayDirectionalLightColor = new Color(0.9921569f, 0.8784314f, 0.7529412f); // FDE0C0
    
    //일몰 값
    private float _sunsetWaterMetallic = 1;
    private Color _sunsetFogColor = new Color(1f, 0.4313725f, 0.2431373f); // FF6E3E
    private float _sunsetFogIntensity = 0.4f;
    private Color _sunsetDirectionalLightColor = new Color(1f, 0.5254902f, 0f); // FF8600
    
    //한밤중 값
    private float _nightWaterMetallic = 1;
    private Color _nightWaterDepthColor = Color.black; // 000000
    private Color _nightFogColor = new Color(0.7843137f, 0.2117647f, 0.5882353f); // C83696
    private float _nightFogIntensity = 1f;
    private Color _nightDirectionalLightColor = new Color(0.2627451f, 0.4f, 0.9803922f); // 4366FA

    private void Start()
    {
        Init();
        StartCoroutine(SimulateDayCycle());
    }

    /// <summary>
    /// 초기 밝은 낮 설정
    /// </summary>
    public void Init()
    {
        //_waterMeshRenderer의 metallic값을 0으로 설정 (밝은 바다 효과)
        _waterMeshRenderer.material.SetFloat("_Metallic", _dayWaterMetallic);
        
        //water depth color를 046BA1로 설정해서 밝은 바다 설정
        _waterMeshRenderer.material.SetColor("_DepthColor", _dayWaterDepthColor);
        
        //fog color를 CCECFF로 설정해서 밝은 낮 설정
        RenderSettings.fogColor = _dayFogColor;
        
        //day스카이박스 머터리얼의 fog intensity를 0으로 설정해서 노을 없애기 설정
        _daySkybox.SetFloat("_FogIntensity", _dayFogIntensity);
        
        //directionalLight의 color를 FDE0C0으로 설정해서 한낮 태양 색 설정
        _directionalLight.color = _dayDirectionalLightColor;
    }

    /// <summary>
    /// 낮->일몰->밤->낮 사이클 시뮬레이션
    /// </summary>
    /// <returns></returns>
    public IEnumerator SimulateDayCycle()
    {
        while (true)
        {
            //8초동안 낮 유지
            yield return new WaitForSeconds(8);

            //5초동안 천천히 일몰효과
            StartCoroutine(StartSunsetEffect(5));
            yield return new WaitForSeconds(5);

            //8초동안 일몰상태 유지
            yield return new WaitForSeconds(8);
        
            //5 천천히 밤으로 변화
            StartCoroutine(StartNightEffectWithDelay(5));
            yield return new WaitForSeconds(5);
            //RenderSettings.skybox = _nightSkybox;
        
            //8초동안 밤 유지
            yield return new WaitForSeconds(8);
        
            //5초동안 천천히 낮으로 변화
            StartCoroutine(StartDayEffectWithDelay(5));
            yield return new WaitForSeconds(5);
            //RenderSettings.skybox = _daySkybox;
        }
        
    }
    
    #region 낮 ->일몰 관련

     /// <summary>
    /// 낮->일몰 효과 적용
    /// </summary>
    /// <param name="sunsetSeconds">낮->노을로 변화하는데 걸리는 시간</param>
    /// <returns></returns>
    IEnumerator StartSunsetEffect(float sunsetSeconds)
    {
        StartCoroutine(ChangeMetallicToSunsetOverTime(sunsetSeconds));
        StartCoroutine(ChangeFogColorToSunsetOverTime(sunsetSeconds));
        StartCoroutine(ChangeFogIntensityToSunsetOverTime(sunsetSeconds));
        StartCoroutine(ChangeDirectionalLightColorToSunsetOverTime(sunsetSeconds));
        yield return null;
    }
     
    /// <summary>
    /// 시간에 따라 물 재질의 메탈릭 값을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화하는지</param>
    /// <returns></returns>
    IEnumerator ChangeMetallicToSunsetOverTime(float duration)
    {
        float time = 0;
        float startValue = _dayWaterMetallic;
        float targetValue = _sunsetWaterMetallic;
        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, time / duration);
            _waterMeshRenderer.material.SetFloat("_Metallic", newValue);
            yield return null;
        }
        _waterMeshRenderer.material.SetFloat("_Metallic", _sunsetWaterMetallic); 
    }
    
    /// <summary>
    /// 시간에 따라 안개 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeFogColorToSunsetOverTime(float duration)
    {
        float time = 0;
        Color startColor = _dayFogColor;
        Color targetColor = _sunsetFogColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }
        RenderSettings.fogColor = targetColor; // Ensure it ends at the target color
    }
    
    /// <summary>
    /// 시간에 따라 안개의 강도를 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화하는지</param>
    /// <returns></returns>
    IEnumerator ChangeFogIntensityToSunsetOverTime(float duration)
    {
        float time = 0;
        float startValue = _dayFogIntensity;
        float targetValue = _sunsetFogIntensity;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, time / duration);
            _daySkybox.SetFloat("_FogIntensity", newValue);
            yield return null;
        }
        _daySkybox.SetFloat("_FogIntensity", _sunsetFogIntensity); 
    }
    
    /// <summary>
    /// 시간에 따라 Directional Light의 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화하는지</param>
    /// <returns></returns>
    IEnumerator ChangeDirectionalLightColorToSunsetOverTime(float duration)
    {
        float time = 0;
        Color startColor = _dayDirectionalLightColor;
        Color targetColor = _sunsetDirectionalLightColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            _directionalLight.color = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }
        _directionalLight.color = targetColor; 
    }
    

    #endregion

    #region 일몰->한밤중 관련
    
    /// <summary>
    /// 일몰->밤 효과 적용
    /// </summary>
    /// <param name="nightSeconds">노을->밤 걸리는 시간</param>
    /// <returns></returns>
    IEnumerator StartNightEffectWithDelay(float nightSeconds)
    {
        StartCoroutine(ChangeMetallicToNightOverTime(nightSeconds));
        StartCoroutine(ChangeWaterDepthColorToNightOverTime(nightSeconds));
        StartCoroutine(ChangeFogColorToNightOverTime(nightSeconds));
        StartCoroutine(ChangeFogIntensityToNightOverTime(nightSeconds));
        StartCoroutine(ChangeDirectionalLightColorToNightOverTime(nightSeconds));
        
        StartCoroutine(ChangeSkyboxMaterialOverTime(_daySkybox, _nightSkybox, nightSeconds));
        yield return null;
    }
    
    /// <summary>
    /// 시간에 따라 물 재질의 메탈릭 값을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeMetallicToNightOverTime(float duration)
    {
        float time = 0;
        float startValue = _sunsetWaterMetallic;
        float targetValue = _nightWaterMetallic;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, time / duration);
            _waterMeshRenderer.material.SetFloat("_Metallic", newValue);
            yield return null;
        }
        _waterMeshRenderer.material.SetFloat("_Metallic", _nightWaterMetallic); 
    }
    
    /// <summary>
    /// 시간에 따라 물 깊이의 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할지</param>
    /// <returns></returns>
    IEnumerator ChangeWaterDepthColorToNightOverTime(float duration)
    {
        float time = 0;
        Color startColor = _dayWaterDepthColor;
        Color targetColor = _nightWaterDepthColor;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            Color newValue = Color.Lerp(startColor, targetColor, time / duration);
            _waterMeshRenderer.material.SetColor("_ColorDepth", newValue);
            yield return null;
        }
        _waterMeshRenderer.material.SetColor("_ColorDepth", _nightWaterDepthColor); 
    }
    
    /// <summary>
    /// 시간에 따라 안개 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeFogColorToNightOverTime(float duration)
    {
        float time = 0;
        Color startColor = _sunsetFogColor;
        Color targetColor = _nightFogColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }
        RenderSettings.fogColor = targetColor; // Ensure it ends at the target color
    }
    
    /// <summary>
    /// 시간에 따라 안개의 강도를 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeFogIntensityToNightOverTime(float duration)
    {
        float time = 0;
        float startValue = _sunsetFogIntensity;
        float targetValue = _nightFogIntensity;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, time / duration);
            _daySkybox.SetFloat("_FogIntensity", newValue);
            yield return null;
        }
        _daySkybox.SetFloat("_FogIntensity", _nightFogIntensity); 
    }
    
    /// <summary>
    /// 시간에 따라 Directional Light의 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeDirectionalLightColorToNightOverTime(float duration)
    {
        float time = 0;
        Color startColor = _sunsetDirectionalLightColor;
        Color targetColor = _nightDirectionalLightColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            _directionalLight.color = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }
        _directionalLight.color = targetColor; 
    }

    #endregion

    #region 밤->낮 관련
    
    /// <summary>
    /// 밤->낮 효과 적용
    /// </summary>
    /// <param name="daySeconds">밤->낮 걸리는 시간</param>
    /// <returns></returns>
    IEnumerator StartDayEffectWithDelay(float daySeconds)
    {
        StartCoroutine(ChangeWaterMetallicToDayOverTime(daySeconds));
        StartCoroutine(ChangeWaterDepthColorToDayOverTime(daySeconds));
        StartCoroutine(ChangeFogColorToDayOverTime(daySeconds));
        StartCoroutine(ChangeFogIntensityToDayOverTime(daySeconds));
        StartCoroutine(ChangeDirectionalLightColorToDayOverTime(daySeconds));
        
        StartCoroutine(ChangeSkyboxMaterialOverTime(_nightSkybox, _daySkybox, daySeconds));
        yield return null;
    }
    
    /// <summary>
    /// 시간에 따라 물 재질의 메탈릭 값을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeWaterMetallicToDayOverTime(float duration)
    {
        float time = 0;
        float startValue = _nightWaterMetallic;
        float targetValue = _dayWaterMetallic;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, time / duration);
            _waterMeshRenderer.material.SetFloat("_Metallic", newValue);
            yield return null;
        }
        _waterMeshRenderer.material.SetFloat("_Metallic", _dayWaterMetallic); 
    }
    
    /// <summary>
    /// 시간에 따라 물 깊이의 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할지</param>
    /// <returns></returns>
    IEnumerator ChangeWaterDepthColorToDayOverTime(float duration)
    {
        float time = 0;
        Color startColor = _nightWaterDepthColor;
        Color targetColor = _dayWaterDepthColor;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            Color newValue = Color.Lerp(startColor, targetColor, time / duration);
            _waterMeshRenderer.material.SetColor("_ColorDepth", newValue);
            yield return null;
        }
        _waterMeshRenderer.material.SetColor("_ColorDepth", _dayWaterDepthColor); 
    }
    
    /// <summary>
    /// 시간에 따라 안개 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeFogColorToDayOverTime(float duration)
    {
        float time = 0;
        Color startColor = _nightFogColor;
        Color targetColor = _dayFogColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }
        RenderSettings.fogColor = targetColor; // Ensure it ends at the target color
    }
    
    /// <summary>
    /// 시간에 따라 안개의 강도를 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeFogIntensityToDayOverTime(float duration)
    {
        float time = 0;
        float startValue = _nightFogIntensity;
        float targetValue = _dayFogIntensity;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, time / duration);
            _daySkybox.SetFloat("_FogIntensity", newValue);
            yield return null;
        }
        _daySkybox.SetFloat("_FogIntensity", _dayFogIntensity); 
    }
    
    /// <summary>
    /// 시간에 따라 Directional Light의 색을 변경
    /// </summary>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeDirectionalLightColorToDayOverTime(float duration)
    {
        float time = 0;
        Color startColor = _nightDirectionalLightColor;
        Color targetColor = _dayDirectionalLightColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            _directionalLight.color = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }
        _directionalLight.color = targetColor; 
    }

    #endregion
    
    /// <summary>
    /// 시간에 따라 스카이박스의 머터리얼을 변경
    /// </summary>
    /// <param name="finalMaterial">최종 목적지 스카이박스 머터리얼</param>
    /// <param name="duration">몇초에 걸쳐서 변화할건지</param>
    /// <returns></returns>
    IEnumerator ChangeSkyboxMaterialOverTime(Material startMaterial, Material finalMaterial, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            /*RenderSettings.skybox.Lerp(startMaterial, finalMaterial, time / duration);
            DynamicGI.UpdateEnvironment();*/
            yield return null;
        }
        
        RenderSettings.skybox = finalMaterial;
    }
}