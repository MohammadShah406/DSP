using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MaterialManager : MonoBehaviour
{
    public List<MaterialSettings> OldMaterialSettings;
    public List<MaterialSettings> upgradedMaterialSettings;

    public List<GameObject> objectsToUpgrade;

    public bool upgradeAll = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(objectsToUpgrade.Count != OldMaterialSettings.Count || objectsToUpgrade.Count != upgradedMaterialSettings.Count)
        {
            Debug.LogError("MaterialManager: The number of objects to upgrade does not match the number of material settings provided.");
        }
        else
        {
            for(int i = 0; i < objectsToUpgrade.Count; i++)
            {
                OldMaterialSettings.Add(new MaterialSettings
                {
                    name = objectsToUpgrade[i].name,
                    material = new List<Material>(objectsToUpgrade[i].GetComponent<Renderer>().materials)
                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(upgradeAll)
        {
            upgradeAll = false;
            for(int i = 0; i < objectsToUpgrade.Count; i++)
            {
                UpgradeMaterials(i);
            }
        }
    }

    public void UpgradeMaterials(int i)
    {
        objectsToUpgrade[i].GetComponent<Renderer>().materials = upgradedMaterialSettings[i].material.ToArray();    
    }

    public void UpgradeAllMaterials()
    {
        for(int i = 0; i < objectsToUpgrade.Count; i++)
        {
            UpgradeMaterials(i);
        }
    }
}

[System.Serializable]
public class MaterialSettings
{
    public string name;
    public List<Material> material;
}
