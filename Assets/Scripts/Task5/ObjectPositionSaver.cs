using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Serializable class to hold save data for a single object.
/// Contains the object's name and its position (Vector3).
/// </summary>
[System.Serializable]
public class SaveData
{
    public string objectName;  
    public Vector3 position;   

    /// <summary>
    /// Constructor to initialize the SaveData with name and position.
    /// </summary>
    public SaveData(string name, Vector3 pos)
    {
        objectName = name;
        position = pos;
    }
}

/// <summary>
/// Main script for saving and loading object positions to/from a JSON file.
/// </summary>
public class ObjectPositionSaver : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] List<Transform> objectsToSave = new List<Transform>();
  
    // The name of the JSON file (including .json extension). Stored in persistentDataPath.
    string fileName = "objectPositions.json";
    string filePath;  

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log("Save/Load path: " + filePath);  // Debug: Shows where the file will be saved/loaded
    }

    /// <summary>
    /// Saves the positions of objects to a JSON file.
    /// If objectsToSave is empty, saves the position of this GameObject (single object fallback).
    /// Otherwise, saves all valid objects in the list.
    /// Uses JsonUtility for serialization and writes to persistentDataPath.
    /// </summary>
    public void SavePositions()
    {
        List<SaveData> saveDataList = new List<SaveData>();  // Temporary list to hold all save data

        if (objectsToSave.Count == 0)
        {
            // Fallback: Save the position of the GameObject this script is attached to
            saveDataList.Add(new SaveData(gameObject.name, transform.position));
            Debug.Log("Saved single object: " + gameObject.name);
        }
        else
        {
            foreach (Transform obj in objectsToSave)
            {
                if (obj != null)
                {
                    saveDataList.Add(new SaveData(obj.name, obj.position));
                }
                else
                {
                    Debug.LogWarning("Null Transform in objectsToSave list!", this);
                }
            }
            Debug.Log("Saved " + saveDataList.Count + " objects");
        }

        // Serialize the list to JSON using a Wrapper (JsonUtility doesn't handle Lists directly)
        // 'true' enables pretty-printing (indented JSON for readability)
        string json = JsonUtility.ToJson(new Wrapper<SaveData>(saveDataList), true);
        File.WriteAllText(filePath, json);  
        Debug.Log("Positions saved to: " + filePath);
    }

    /// <summary>
    /// Loads positions from the JSON file and applies them to the corresponding objects.
    /// Matches objects by name (objectName). If the file doesn't exist, logs a warning and skips.
    /// For multiple objects, only loads for those found in the objectsToSave list.
    /// </summary>
    public void LoadPositions()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Save file not found: " + filePath + ". Nothing to load.");
            return;
        }

        string json = File.ReadAllText(filePath);
        // Deserialize using Wrapper to get the list of SaveData
        Wrapper<SaveData> wrapper = JsonUtility.FromJson<Wrapper<SaveData>>(json);
        List<SaveData> saveDataList = wrapper.list;

        if (saveDataList == null || saveDataList.Count == 0)
        {
            Debug.LogWarning("Empty or invalid save data in: " + filePath);
            return;
        }

        int loadedCount = 0;  

        if (objectsToSave.Count == 0)
        {
            // Fallback: Load for this GameObject only
            SaveData data = saveDataList.Find(d => d.objectName == gameObject.name);  
            if (data != null)
            {
                transform.position = data.position;  
                loadedCount++;
                Debug.Log("Loaded position for: " + gameObject.name);
            }
            else
            {
                Debug.LogWarning("No save data for object: " + gameObject.name);
            }
        }
        else
        {
            foreach (SaveData data in saveDataList)
            {
                Transform targetObj = objectsToSave.Find(t => t != null && t.name == data.objectName);
                if (targetObj != null)
                {
                    targetObj.position = data.position;  
                    loadedCount++;
                }
                else
                {
                    Debug.LogWarning("Object not found for save data: " + data.objectName + ". Skipping.");
                }
            }
            Debug.Log("Loaded " + loadedCount + " out of " + saveDataList.Count + " positions");
        }
    }

    /// <summary>
    /// Helper wrapper class for JSON serialization.
    /// JsonUtility requires a container class to serialize Lists directly.
    /// </summary>
    /// <typeparam name="T">The type of items in the list (e.g., SaveData)</typeparam>
    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> list;  // The actual list to serialize

        /// <summary>
        /// Constructor to initialize the wrapper with a list.
        /// </summary>
        /// <param name="l">The list to wrap</param>
        public Wrapper(List<T> l) { list = l; }
    }
}
