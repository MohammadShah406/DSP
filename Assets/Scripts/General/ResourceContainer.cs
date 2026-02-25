using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple container to hold resources on GameObjects like trashbins, etc.
/// </summary>
public class ResourceContainer : MonoBehaviour
{
    public List<ResourceData> resources = new List<ResourceData>();

    public void AddResource(string name, int amount)
    {
        var existing = resources.Find(r => r.resourceName == name);
        if (existing != null)
        {
            existing.quantity += amount;
        }
        else
        {
            resources.Add(new ResourceData { resourceName = name, quantity = amount });
        }
    }

    public void AddResources(List<ResourceData> newResources)
    {
        foreach (var res in newResources)
        {
            AddResource(res.resourceName, res.quantity);
        }
    }

    public List<ResourceData> TakeAll()
    {
        List<ResourceData> taken = new List<ResourceData>(resources);
        resources.Clear();
        return taken;
    }
}
