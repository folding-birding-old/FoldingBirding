using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMatManager : MonoBehaviour
{
    [SerializeField] private GameObject selectMatCanvas;
    private void Start()
    {
        StartCoroutine(SetCanvas());
    }
    private IEnumerator SetCanvas()
    {
        yield return new WaitForSeconds(0.3f);

        Camera cam = Camera.main;
        if (cam == null) yield break;

        Transform camTf = cam.transform;

        Vector3 pos = camTf.position + camTf.forward * 0.1f;
        pos.y = camTf.position.y;

        selectMatCanvas.transform.position = pos;

        selectMatCanvas.transform.LookAt(camTf);
        selectMatCanvas.transform.Rotate(0, 180f, 0f);
    }

    public void OnClickSelectBtn()
    {
        SceneLoader.Instance.LoadScene(3);
        selectMatCanvas.SetActive(false);


    }
}
