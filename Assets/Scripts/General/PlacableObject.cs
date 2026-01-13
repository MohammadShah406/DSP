using UnityEngine;

public class PlacableObject : MonoBehaviour
{
    private void OnEnable()
    {
        if(TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTaskByRequirement(gameObject.name);
        }
    }
}
