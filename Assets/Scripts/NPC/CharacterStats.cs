using System;
using System.Collections;
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
    [Range(0, 100)]public int learning = 10;
    [Range(0, 100)]public int workReadiness = 100;
    [Range(0, 100)]public int trust = 50;
    [Range(0, 100)]public int hunger = 50;
    [Range(0, 100)]public int thurst = 50;
    [Range(0, 100)]public int sleep = 50;
    public int drainRatePerMinute = 10;
    private float _drainIntervalInSeconds = 60f;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.characters.Contains(this.gameObject))
                GameManager.Instance.characters.Add(this.gameObject);
        }

        StartCoroutine(DrainStatsOverTime());
    }

    private IEnumerator DrainStatsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(_drainIntervalInSeconds);

            ChangeHunger(drainRatePerMinute);
            ChangeSleep(drainRatePerMinute);
            ChangeThurst(drainRatePerMinute);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.characters.Remove(this.gameObject);
        }

        StopAllCoroutines();
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

    public int Hunger
    {
        get => hunger;
        set
        {
            if (hunger != value)
            {
                hunger = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Thurst
    {
        get => thurst;
        set
        {
            if (thurst != value)
            {
                thurst = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Sleep
    {
        get => sleep;
        set
        {
            if (sleep != value)
            {
                sleep = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    // Helpers now use properties
    public void ChangeHealth(int change) => Health += change;
    public void ChangeStability(int change) => Stability += change;
    public void ChangeLearning(int change) => Learning += change;
    public void ChangeWorkReadiness(int change) => WorkReadiness += change;
    public void ChangeTrust(int change) => Trust += change;
    public void ChangeHunger(int change) => Hunger += change;
    public void ChangeThurst(int change) => Thurst += change;
    public void ChangeSleep(int change) => Sleep += change;

}
