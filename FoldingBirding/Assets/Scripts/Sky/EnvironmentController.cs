using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public enum TimeOfDay { Day, Sunset, Night }

    [Header("하늘 Plane들 (5개)")]
    public GameObject[] skyPlanes;

    [Header("Directional Light")]
    public Light directionalLight;

    [Header("Night Light Objects")]
    public GameObject[] nightLights;

    [Header("Play Time (초)")]
    public float totalPlayTime = 300f;

    private float timer = 0f;
    private TimeOfDay currentTime;

    private Color targetTopColor;
    private Color targetMiddleColor;
    private Color targetFogColor;
    private Vector3 targetLightRotation;
    private bool nightActive;

    private Color currentTopColor;
    private Color currentMiddleColor;
    private Color currentFogColor;
    private Vector3 currentLightRotation;

    private readonly Color horizonColorFixed = Color.white;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
        SetEnvironment(TimeOfDay.Day, immediate: true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. 반복 가능한 시간으로 변환
        float cycleTime = timer;

        // 2. 시간 상태 결정 (Day → Sunset → Night)
        TimeOfDay newTime;
        if (cycleTime < totalPlayTime * 0.4f)
            newTime = TimeOfDay.Day;
        else if (cycleTime < totalPlayTime * 0.8f)
            newTime = TimeOfDay.Sunset;
        else
            newTime = TimeOfDay.Night;

        // 3. 상태 변화 시 환경 설정
        if (newTime != currentTime)
        {
            SetEnvironment(newTime);
            Debug.Log("환경 전환: " + newTime);
        }

        // 4. 사이클이 끝나면 타이머 리셋 (한 바퀴 끝나면 0초로)
        if (timer >= totalPlayTime)
            timer = 0f;


        float speed = 0.5f * Time.deltaTime;
        currentTopColor = Color.Lerp(currentTopColor, targetTopColor, speed);
        currentMiddleColor = Color.Lerp(currentMiddleColor, targetMiddleColor, speed);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, speed);
        currentLightRotation = Vector3.Lerp(currentLightRotation, targetLightRotation, speed);

        RenderSettings.fogColor = currentFogColor;

        if (directionalLight != null)
            directionalLight.transform.eulerAngles = currentLightRotation;

        ApplySkyColorsToPlanes();
    }

    void ApplySkyColorsToPlanes()
    {
        propBlock.SetColor("_TopSkyColor", currentTopColor);
        propBlock.SetColor("_MiddleSkyColor", currentMiddleColor);
        propBlock.SetColor("_HorizonColor", horizonColorFixed);

        foreach (var plane in skyPlanes)
        {
            if (plane != null)
            {
                var renderer = plane.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.SetPropertyBlock(propBlock);
            }
        }
    }

    void SetEnvironment(TimeOfDay time, bool immediate = false)
    {
        currentTime = time;

        switch (time)
        {
            case TimeOfDay.Day:
                SetTarget("#688DFD", "#BBDADD", "#B3D7DB", new Vector3(80, -500, -50), false, immediate);
                break;
            case TimeOfDay.Sunset:
                SetTarget("#95ACE9", "#FFB25A", "#BEAA99", new Vector3(17, -452, -8), false, immediate);
                break;
            case TimeOfDay.Night:
                SetTarget("#3D4565", "#000535", "#324C59", new Vector3(0, -520, -20), true, immediate);
                break;
        }
    }

    void SetTarget(string topHex, string middleHex, string fogHex, Vector3 rotation, bool isNight, bool immediate)
    {
        targetTopColor = HexToColor(topHex);
        targetMiddleColor = HexToColor(middleHex);
        targetFogColor = HexToColor(fogHex);
        targetLightRotation = rotation;
        nightActive = isNight;

        if (immediate)
        {
            currentTopColor = targetTopColor;
            currentMiddleColor = targetMiddleColor;
            currentFogColor = targetFogColor;
            currentLightRotation = targetLightRotation;
        }

        foreach (GameObject obj in nightLights)
        {
            if (obj != null)
                obj.SetActive(isNight);
        }
    }

    private static Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}
