using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnchorCenterPlacer : MonoBehaviour
{
    public GameObject targetObject;
    public OVRSpatialAnchor anchorPrefab;

    private const int AnchorCount = 4;
    private const string NumUuidKey = "numUuids";
    private const string UuidKeyPrefix = "uuid";

    private async void Start()
    {
        var anchorPositions = await LoadAnchorPositionsFromCloud();

        if (anchorPositions.Count == AnchorCount)
        {
            ApplyCenterAndRotation(anchorPositions);
        }
        else
        {
            Debug.LogWarning("[PLACEMENT] Not enough anchors loaded.");
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            SceneManager.LoadScene(4);
        }
    }

    private async Task<List<Vector3>> LoadAnchorPositionsFromCloud()
    {
        List<Guid> guids = new();
        List<Vector3> positions = new();

        int count = PlayerPrefs.GetInt(NumUuidKey, 0);
        for (int i = 0; i < count; i++)
        {
            string key = UuidKeyPrefix + i;
            if (PlayerPrefs.HasKey(key))
            {
                string uuidString = PlayerPrefs.GetString(key);
                if (Guid.TryParse(uuidString, out var guid))
                {
                    guids.Add(guid);
                }
            }
        }

        var loadOptions = new OVRSpatialAnchor.LoadOptions
        {
            StorageLocation = OVRSpace.StorageLocation.Cloud,
            Timeout = 0,
            Uuids = guids
        };

        var unboundAnchors = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(loadOptions);

        foreach (var unbound in unboundAnchors)
        {
            if (!unbound.Localized)
            {
                bool localized = await unbound.LocalizeAsync();
                if (!localized)
                {
                    Debug.LogWarning($"[PLACEMENT] Failed to localize anchor {unbound.Uuid}");
                    continue;
                }
            }

            if (unbound.TryGetPose(out Pose pose))
            {
                positions.Add(pose.position);
                Debug.Log($"[PLACEMENT] Loaded anchor at: {pose.position}");
            }
        }

        return positions;
    }

    private void ApplyCenterAndRotation(List<Vector3> positions)
    {
        Vector3 center = Vector3.zero;
        foreach (var pos in positions)
        {
            center += pos;
        }
        center /= positions.Count;

        Vector3 forward = ((positions[0] - positions[3]) + (positions[1] - positions[2])) * 0.5f;
        forward.y = 0f;
        forward.Normalize();

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        targetObject.transform.position = center;
        targetObject.transform.rotation = rotation;

        Debug.Log("[PLACEMENT] Center: " + center);
        Debug.Log("[PLACEMENT] Forward: " + forward);
    }
}
