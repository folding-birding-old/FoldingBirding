using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FourAnchor : MonoBehaviour
{
    public OVRSpatialAnchor anchorPrefab;
    public Material markerMaterial;

    private List<OVRSpatialAnchor> anchors = new();
    private List<GameObject> markers = new();
    private List<GameObject> labels = new();

    private int anchorCount = 0;
    private const int MaxAnchors = 4;
    private const string UuidKey = "uuid";
    private const string NumUuidKey = "numUuids";

    private GameObject previewMarker;

    void Start()
    {
        previewMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        previewMarker.transform.localScale = Vector3.one * 0.05f;
        Renderer previewRenderer = previewMarker.GetComponent<Renderer>();
        previewRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        previewRenderer.material.color = Color.yellow;
        previewMarker.name = "PreviewMarker";
    }

    void Update()
    {
        Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        Vector3 forwardDir = controllerRot * Vector3.forward;
        Vector3 targetPos = controllerPos + forwardDir * 0.1f;
        targetPos.y = 0f;

        if (previewMarker != null)
            previewMarker.transform.position = targetPos;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (anchorCount < MaxAnchors)
                CreateAnchorWithMarker(targetPos, controllerRot);
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            DeleteLastAnchor();

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            SceneManager.LoadScene(3);

        if (OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.LTouch))
            Application.Quit();
    }

    private void CreateAnchorWithMarker(Vector3 targetPos, Quaternion controllerRot)
    {
        OVRSpatialAnchor anchor = Instantiate(anchorPrefab, targetPos, Quaternion.Euler(0f, controllerRot.eulerAngles.y, 0f));
        anchors.Add(anchor);

        CreateVisualMarker(targetPos, anchorCount + 1);
        SaveAnchorToCloudAsync(anchor, anchorCount);
        anchorCount++;
    }

    private async void SaveAnchorToCloudAsync(OVRSpatialAnchor anchor, int index)
    {
        while (!anchor.Created)
            await Task.Yield();

        bool success = await anchor.SaveAnchorAsync();
        if (success)
        {
            PlayerPrefs.SetString(UuidKey + index, anchor.Uuid.ToString());
            PlayerPrefs.SetInt(NumUuidKey, anchorCount);
            PlayerPrefs.Save();
            Debug.Log($"[CLOUD] Saved UUID: {anchor.Uuid}");
        }
        else
        {
            Debug.LogWarning("[CLOUD] Failed to save anchor");
        }
    }

    private async void DeleteLastAnchor()
    {
        if (anchorCount == 0) return;

        int lastIndex = anchorCount - 1;
        OVRSpatialAnchor anchorToDelete = anchors[lastIndex];

        if (anchorToDelete != null)
        {
            bool success = await anchorToDelete.EraseAsync();
            if (success)
            {
                Destroy(anchorToDelete.gameObject);
                Destroy(markers[lastIndex]);
                Destroy(labels[lastIndex]);

                anchors.RemoveAt(lastIndex);
                markers.RemoveAt(lastIndex);
                labels.RemoveAt(lastIndex);

                PlayerPrefs.DeleteKey(UuidKey + lastIndex);
                anchorCount--;
                PlayerPrefs.SetInt(NumUuidKey, anchorCount);
                PlayerPrefs.Save();
            }
        }
    }

    private void CreateVisualMarker(Vector3 position, int labelNumber)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.05f;

        Material mat = markerMaterial != null ? markerMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.green;
        marker.GetComponent<Renderer>().material = mat;
        marker.name = "AnchorMarker";
        markers.Add(marker);

        GameObject labelObj = new GameObject("AnchorLabel" + labelNumber);
        TextMeshPro label = labelObj.AddComponent<TextMeshPro>();
        label.text = labelNumber.ToString();
        label.fontSize = 10f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.transform.position = position + Vector3.up * 0.08f;
        label.transform.localScale = Vector3.one * 0.05f;
        labelObj.transform.SetParent(marker.transform);

        labels.Add(labelObj);
    }
}
