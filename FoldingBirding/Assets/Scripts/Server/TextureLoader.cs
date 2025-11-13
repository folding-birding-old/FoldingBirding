using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class TextureLoader : MonoBehaviour
{
    public string serverUrl = "http://localhost:8000"; 
    [SerializeField] private Transform contents;// content parent
    [SerializeField] private GameObject contentPrefab; // nicknameButtonPrefab parent
    [SerializeField] private GameObject nicknameButtonPrefab;
    [SerializeField] private Material targetMaterial;

    private GameObject curContent;
    private int contentAmout = 0;
    private int nicknameAmout = 0;
    private int curContentNum = 0;

    void Start()
    {
        targetMaterial.SetColor("_BaseColor", Color.white);
        targetMaterial.SetTexture("_BaseMap", null);
        StartCoroutine(FetchTextureList());
    }

    public void FetchTextures()
    {
        StartCoroutine(FetchTextureList()); 
    }

    IEnumerator FetchTextureList()
    {
        string url = $"{serverUrl}/textures";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("? Failed to fetch texture list");
            yield break;
        }

        string json = req.downloadHandler.text;
        TextureListWrapper data = JsonUtility.FromJson<TextureListWrapper>(json);
        
        foreach (string name in data.nicknames)
        {
            if(nicknameAmout % 24 == 0)
            {
                curContent = Instantiate(contentPrefab, contents);
                contentAmout++;

                if (contentAmout != 1)
                {
                    curContent.SetActive(false);
                }
            }
            GameObject btn = Instantiate(nicknameButtonPrefab, curContent.transform);
            btn.GetComponentInChildren<TMP_Text>().text = name;
            btn.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(() => StartCoroutine(DownloadTexture(name)));

            nicknameAmout++;
        }
    }

    IEnumerator DownloadTexture(string nickname)
    {
        string url = $"{serverUrl}/textures/{nickname}";
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"? Failed to download texture for {nickname}");
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        targetMaterial.mainTexture = tex;
        Debug.Log($"? {nickname} Texturing");
    }

    [System.Serializable]
    public class TextureListWrapper
    {
        public List<string> nicknames;
    }

    public void showForwawrd()
    {
        if(curContentNum < contentAmout - 1)
        {
            contents.GetChild(curContentNum).gameObject.SetActive(false);
            contents.GetChild(curContentNum + 1).gameObject.SetActive(true);
            curContentNum++;
        }
    }

    public void showBackwawrd()
    {
        if (curContentNum > 0)
        {
            contents.GetChild(curContentNum).gameObject.SetActive(false);
            contents.GetChild(curContentNum - 1).gameObject.SetActive(true);
            curContentNum--;
        }
    }
}