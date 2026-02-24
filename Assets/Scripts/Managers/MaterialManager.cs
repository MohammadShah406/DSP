using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager instance;
    [Header("House Materials")]
    public List<MaterialSettings> OldMaterialSettings;
    public List<MaterialSettings> upgradedMaterialSettings;

    [Header("Bashir Materials")]
    public List<MaterialSettings> oldBashirMaterialsSettings;
    public List<MaterialSettings> upgradedBashirMaterialsSettings;

    [Header("Sahil Materials")]
    public List<MaterialSettings> oldSahilMaterialSettings;
    public List<MaterialSettings> upgradedSahilMaterialSettings;

    [Header("Aisha Materials")]
    public List<MaterialSettings> oldAishaMaterialSettings;
    public List<MaterialSettings> upgradedAishaMaterialSettings;


    [Header("House Objects")]
    public List<GameObject> objectsToUpgrade;

    [Header("Bashir Objects")]
    public List<GameObject> bashirObjectsToUpgrade;
    [Header("Sahil Objects")]
    public List<GameObject> sahilObjectsToUpgrade;
    [Header("Aisha Objects")]
    public List<GameObject> aishaObjectsToUpgrade;


    public bool upgradedAll = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool upgradeAll = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (objectsToUpgrade.Count != upgradedMaterialSettings.Count)
        {
            Debug.LogError("MaterialManager: The number of objects to upgrade does not match the number of material settings provided.");
        }
        else
        {
            for (int i = 0; i < objectsToUpgrade.Count; i++)
            {
                OldMaterialSettings.Add(new MaterialSettings
                {
                    name = objectsToUpgrade[i].name,
                    material = new List<Material>(objectsToUpgrade[i].GetComponent<Renderer>().materials)
                });
            }


        }

        if (bashirObjectsToUpgrade.Count != upgradedBashirMaterialsSettings.Count)
        {
            Debug.LogError("MaterialManager: The number of Bashir objects to upgrade does not match the number of Bashir material settings provided.");
        }
        else
        {
            for (int i = 0; i < bashirObjectsToUpgrade.Count; i++)
            {
                oldBashirMaterialsSettings.Add(new MaterialSettings
                {
                    name = bashirObjectsToUpgrade[i].name,
                    material = new List<Material>(bashirObjectsToUpgrade[i].GetComponent<Renderer>().materials)
                });
            }
        }

        if (sahilObjectsToUpgrade.Count != upgradedSahilMaterialSettings.Count)
        {
            Debug.LogError("MaterialManager: The number of Sahil objects to upgrade does not match the number of Sahil material settings provided.");
        }
        else
        {
            for (int i = 0; i < sahilObjectsToUpgrade.Count; i++)
            {
                oldSahilMaterialSettings.Add(new MaterialSettings
                {
                    name = sahilObjectsToUpgrade[i].name,
                    material = new List<Material>(sahilObjectsToUpgrade[i].GetComponent<Renderer>().materials)
                });
            }
        }

        if (aishaObjectsToUpgrade.Count != upgradedAishaMaterialSettings.Count)
        {
            Debug.LogError("MaterialManager: The number of Aisha objects to upgrade does not match the number of Aisha material settings provided.");
        }
        else
        {
            for (int i = 0; i < aishaObjectsToUpgrade.Count; i++)
            {
                oldAishaMaterialSettings.Add(new MaterialSettings
                {
                    name = aishaObjectsToUpgrade[i].name,
                    material = new List<Material>(aishaObjectsToUpgrade[i].GetComponent<Renderer>().materials)
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
        if(GameManager.Instance.Hope > 80)
        {
            if(!upgradedAll)
            {
                upgradedAll = true;
                UpgradeAllMaterials();
            }
        }
    }

    public void UpgradeAllBashirMaterials()
    {
        for (int i = 0; i < bashirObjectsToUpgrade.Count; i++)
        {
            UpgradeBashirMaterials(i);
        }
    }

    public void UpgradeAllSahilMaterials()
    {
        for (int i = 0; i < sahilObjectsToUpgrade.Count; i++)
        {
            UpgradeSahilMaterials(i);
        }
    }

    public void UpgradeAllAishaMaterials()
    {
        for (int i = 0; i < aishaObjectsToUpgrade.Count; i++)
        {
            UpgradeAishaMaterials(i);
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

    public void UpgradeBashirMaterials(int i)
    {
        bashirObjectsToUpgrade[i].GetComponent<Renderer>().materials = upgradedBashirMaterialsSettings[i].material.ToArray();
    }

    public void UpgradeSahilMaterials(int i)
    {
        sahilObjectsToUpgrade[i].GetComponent<Renderer>().materials = upgradedSahilMaterialSettings[i].material.ToArray();
    }

    public void UpgradeAishaMaterials(int i)
    {
        aishaObjectsToUpgrade[i].GetComponent<Renderer>().materials = upgradedAishaMaterialSettings[i].material.ToArray();
    }
}

[System.Serializable]
public class MaterialSettings
{
    public string name;
    public List<Material> material;
}
