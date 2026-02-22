using System.Collections.Generic;
using UnityEngine;

public class PlacableObject : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> lights;

    private void OnEnable()
    {
        TaskManager.Instance.CompleteTaskByRequirement(gameObject.name);
        foreach (GameObject light in lights)
        {
            if (light != null)
            {
                light.SetActive(true);
            }
        }
    }
}
