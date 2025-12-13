using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public static event Action<CharacterStats> OnAnyStatChanged;

    [Header("Identity")]
    public string characterName;

    [TextArea(2,4)]
    public string description;

    [Header("Individual Stats")]
    [Range(0, 100)]public int health = 100;
    [Range(0, 100)]public int stability = 100;
    [Range(0, 100)]public int learning = 0;
    [Range(0, 100)]public int workReadiness = 100;
    [Range(0, 100)]public int trust = 50;

    private void Start()
    {
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
    // Properties that notify when changed
    public int Health
    {
        get => health;
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Stability
    {
        get => stability;
        set
        {
            if (stability != value)
            {
                stability = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Learning
    {
        get => learning;
        set
        {
            if (learning != value)
            {
                learning = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int WorkReadiness
    {
        get => workReadiness;
        set
        {
            if (workReadiness != value)
            {
                workReadiness = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Trust
    {
        get => trust;
        set
        {
            if (trust != value)
            {
                trust = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    // Helpers now use properties
    public void ChangeHealth(int delta) => Health += delta;
    public void ChangeStability(int delta) => Stability += delta;
    public void ChangeLearning(int delta) => Learning += delta;
    public void ChangeWorkReadiness(int delta) => WorkReadiness += delta;
    public void ChangeTrust(int delta) => Trust += delta;

}
