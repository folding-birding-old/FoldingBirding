using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnchorManager : MonoBehaviour
{
    public static AnchorManager Instance;
    [SerializeField] private int _numMaxAnchors; // use 4

    [SerializeField] private GameObject _saveableAnchorPrefab;
    [SerializeField] private GameObject _saveablePreview;
    [SerializeField] private Transform _saveableTransform;

    private GameObject _anchorPreview;
    private List<OVRSpatialAnchor> _anchorInstances = new();
    private HashSet<Guid> _anchorUuids = new(); // List of UUIDs for anchors saved to device storage

    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;

    // PlayerPrefs Key definition
    private const string SavedAnchorUuidsKey = "SavedAnchorUuids";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _onLocalized = OnLocalized;
            
            // Load UUID list from PlayerPrefs when the app starts
            LoadUuidsFromPlayerPrefs();
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _anchorPreview = Instantiate(_saveablePreview);
        // Automatically load anchors based on UUID list from PlayerPrefs when the app starts
        LoadAllAnchors(); 
    }

    // Called when the application quits
    private void OnApplicationQuit()
    {
        // Save current UUID list to PlayerPrefs before the app closes
        SaveUuidsToPlayerPrefs();
    }

    void Update()
    {
        // Update anchor preview
        _anchorPreview.transform.rotation = _saveableTransform.rotation;
        _anchorPreview.transform.position = _saveableTransform.position + _saveableTransform.forward * 0.1f;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) // Create a green box
        {
            if (_anchorInstances.Count >= _numMaxAnchors) return;
            var go = Instantiate(_saveableAnchorPrefab, _anchorPreview.transform.position, _anchorPreview.transform.rotation);
            SetupAnchorAsync(go.AddComponent<OVRSpatialAnchor>(), saveAnchor: true);
        }
        else if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            foreach (var anchor in _anchorInstances)
            {
                Destroy(anchor.gameObject);
            }
            _anchorInstances.Clear();  
            EraseAllAnchors();     
        }
        else if (OVRInput.GetDown(OVRInput.Button.One)) // a button
        {
            // // Destroy anchor GameObjects in scene (can be moved inside EraseAllAnchors if preferred)
            // foreach (var anchor in _anchorInstances)
            // {
            //     Destroy(anchor.gameObject);
            // }
            // _anchorInstances.Clear();
        }
        else if (OVRInput.GetDown(OVRInput.Button.Two)) // b button
        {
            // // Erase all anchors from device storage and destroy scene objects
            // EraseAllAnchors();
        }
    }

    private async void SetupAnchorAsync(OVRSpatialAnchor anchor, bool saveAnchor)
    {
        if (!await anchor.WhenLocalizedAsync())
        {
            Debug.LogError($"Unable to create anchor.");
            Destroy(anchor.gameObject);
            return;
        }

        _anchorInstances.Add(anchor);

        if (saveAnchor && (await anchor.SaveAnchorAsync()).Success)
        {
            _anchorUuids.Add(anchor.Uuid);
            Debug.Log($"Anchor created and saved. UUID: {anchor.Uuid}. Total saved UUIDs in list: {_anchorUuids.Count}");
            // Save UUID list to PlayerPrefs immediately (optional)
            SaveUuidsToPlayerPrefs(); 
        }
        else if (saveAnchor) // If saveAnchor is true but saving failed
        {
            Debug.LogError($"Failed to save anchor: {anchor.Uuid}");
            Destroy(anchor.gameObject); 
        }
    }


    public async void LoadAllAnchors()
    {
        // Destroy currently loaded anchor GameObjects in the scene (to prevent duplicates)
        foreach (var anchor in _anchorInstances)
        {
            Destroy(anchor.gameObject);
        }
        _anchorInstances.Clear();

        if (_anchorUuids.Count == 0)
        {
            Debug.LogWarning("No anchor UUIDs found in PlayerPrefs. No anchors to load.");
            return;
        }

        Debug.Log($"Requesting to load {_anchorUuids.Count} anchors using UUIDs from PlayerPrefs...");
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(_anchorUuids, unboundAnchors);

        if (result.Success)
        {
            Debug.Log($"Successfully loaded {unboundAnchors.Count} anchors from device storage.");
            foreach (var anchor in unboundAnchors)
            {
                anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
            }
        }
        else
        {
            Debug.LogError($"Load anchors failed with {result.Status}.");
        }
    }

    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        if (success)
        {
            var pose = unboundAnchor.Pose;
            var go = Instantiate(_saveableAnchorPrefab, pose.position, pose.rotation);
            var anchor = go.AddComponent<OVRSpatialAnchor>();

            unboundAnchor.BindTo(anchor);
            _anchorInstances.Add(anchor);
            Debug.Log($"Anchor localized and bound: {anchor.Uuid}");
        }
        else
        {
            Debug.LogError($"Failed to localize unbound anchor: {unboundAnchor.Uuid}");
        }
    }

    public async void EraseAllAnchors()
    {
        var result = await OVRSpatialAnchor.EraseAnchorsAsync(anchors: null, uuids: _anchorUuids);
        if (result.Success)
        {
            // Erase from device storage, then clear app's UUID list and update PlayerPrefs
            _anchorUuids.Clear();
            SaveUuidsToPlayerPrefs(); 
            
            Debug.Log($"Anchors erased from device storage and UUID list cleared in PlayerPrefs.");

            // Also destroy anchor GameObjects in the scene
            foreach (var anchor in _anchorInstances)
            {
                Destroy(anchor.gameObject);
            }
            _anchorInstances.Clear();
        }
        else
        {
            Debug.LogError($"Anchors NOT erased {result.Status}");
        }
    }


    // Method to load UUID list from PlayerPrefs
    private void LoadUuidsFromPlayerPrefs()
    {
        _anchorUuids.Clear(); // Clear existing list
        if (PlayerPrefs.HasKey(SavedAnchorUuidsKey))
        {
            string uuidString = PlayerPrefs.GetString(SavedAnchorUuidsKey);
            string[] uuidArray = uuidString.Split(';', StringSplitOptions.RemoveEmptyEntries); // Remove empty entries

            foreach (string uuid in uuidArray)
            {
                if (Guid.TryParse(uuid, out Guid parsedGuid))
                {
                    _anchorUuids.Add(parsedGuid);
                }
            }
            Debug.Log($"Loaded {_anchorUuids.Count} UUIDs from PlayerPrefs.");
        }
        else
        {
            Debug.Log("No anchor UUIDs saved in PlayerPrefs.");
        }
    }

    // Method to save UUID list to PlayerPrefs
    private void SaveUuidsToPlayerPrefs()
    {
        // Convert all UUIDs in HashSet to a single semicolon-separated string
        string uuidString = string.Join(";", _anchorUuids.Select(uuid => uuid.ToString()));
        PlayerPrefs.SetString(SavedAnchorUuidsKey, uuidString);
        PlayerPrefs.Save(); // Immediately save changes to disk
        Debug.Log($"Saved UUID list ({_anchorUuids.Count} items) to PlayerPrefs.");
    }
}