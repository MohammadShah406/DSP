using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public static event Action<CharacterStats, string, int, int> OnStatChanged;

    [Header("Identity")]
    public string characterName;
    [Header("Individual Stats")]
    [Range(0, 100)]public int health = 100;
    [Range(0, 100)]public int stability = 100;
    [Range(0, 100)]public int learning = 0;
    [Range(0, 100)]public int workReadiness = 100;
    [Range(0, 100)]public int trust = 50;

    private void Start()
    {
        // Register GameObject to GameManager (works with teammate's code)
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.characters.Contains(this.gameObject))
                GameManager.Instance.characters.Add(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.characters.Remove(this.gameObject);
        }
    }

    // Helpers to update stats with clamping to 0..100
    public void ChangeHealth(int delta) => health = Mathf.Clamp(health + delta, 0, 100);
    public void ChangeStability(int delta) => stability = Mathf.Clamp(stability + delta, 0, 100);
    public void ChangeLearning(int delta) => learning = Mathf.Clamp(learning + delta, 0, 100);
    public void ChangeWorkReadiness(int delta) => workReadiness = Mathf.Clamp(workReadiness + delta, 0, 100);
    public void ChangeTrust(int delta) => trust = Mathf.Clamp(trust + delta, 0, 100);
}
