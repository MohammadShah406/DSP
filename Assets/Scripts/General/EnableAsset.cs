using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnableAsset : MonoBehaviour
{
    public static EnableAsset Instance { get; private set; }

    public List<GameObject> assetsToEnable;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void EnableAllAssets()
    {
        foreach (GameObject asset in assetsToEnable)
        {
            asset.SetActive(true);
        }
    }

    public void DisableAllAssets()
    {
        foreach (GameObject asset in assetsToEnable)
        {
            asset.SetActive(false);
        }
    }

    public void EnableCertainAsset(string name)
    {
        GameObject asset = assetsToEnable.Find(a => a.name == name);
        if (asset != null)
        {
            asset.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Asset with name {name} not found in assetsToEnable list.");
        }
    }

    public void DisableCertainAsset(string name)
    {
        GameObject asset = assetsToEnable.Find(a => a.name == name);
        if (asset != null)
        {
            asset.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Asset with name {name} not found in assetsToEnable list.");
        }
    }

    public void EnableCertainAsset(int index)
    {
        if (index >= 0 && index < assetsToEnable.Count)
        {
            assetsToEnable[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Index {index} is out of range for assetsToEnable list.");
        }
    }

    public void DisableCertainAsset(int index)
    {
        if (index >= 0 && index < assetsToEnable.Count)
        {
            assetsToEnable[index].SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Index {index} is out of range for assetsToEnable list.");
        }
    }

}
