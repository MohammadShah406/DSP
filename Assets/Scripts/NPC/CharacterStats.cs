using System;
using System.Collections;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public static event Action<CharacterStats> OnAnyStatChanged;

    public enum PrimaryAttribute
    {
        None,
        Stability,
        Learning,
        WorkReadiness,
        Trust,
        Nutrition,
        Hygiene,
        Energy
    }

    [Header("Identity")]
    public string characterName;
    public Sprite characterIcon;

    [TextArea(2,4)]
    public string description;

    [Header("Growth Settings")]
    public PrimaryAttribute primaryAttribute = PrimaryAttribute.None;
    public float growthRate = 1.0f;

    [Header("Individual Stats")]
    [Range(0, 100)] [SerializeField] private int health = 100;
    [Range(0, 100)] [SerializeField] private int stability = 100;
    [Range(0, 100)] [SerializeField] private int learning = 10;
    [Range(0, 100)] [SerializeField] private int workReadiness = 100;
    [Range(0, 100)] [SerializeField] private int trust = 50;
    [Range(0, 100)] [SerializeField] private int nutrition = 50;
    [Range(0, 100)] [SerializeField] private int hygiene = 50;
    [Range(0, 100)] [SerializeField] private int energy = 50;

    [Header("Hourly Decay Rates")]
    public int nutritionDecayRate = 5;
    public int hygieneDecayRate = 3;
    public int energyDecayRate = 4;

    [Header("Health Formula Settings")]
    public float healthBase = 35f;
    public float healthMultiplier = 0.01f;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.characters.Contains(this.gameObject))
                GameManager.Instance.characters.Add(this.gameObject);
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.HourChanged += ApplyHourlyDecay;
            TimeManager.Instance.HourChanged += UpdateHealth;
        }
    }

    private void ApplyHourlyDecay(int hours, int minutes, int days)
    {
        ChangeNutrition(-nutritionDecayRate);
        ChangeHygiene(-hygieneDecayRate);
        ChangeEnergy(-energyDecayRate);
    }

    private void UpdateHealth(int hours, int minutes, int days)
    {
        // Formula: Health = Health + (Avg - Base) * M
        // Avg = average of h,e,n (hygiene, energy, nutrition)
        float avg = (Nutrition + Hygiene + Energy) / 3f;
        
        Health += Mathf.RoundToInt((avg - healthBase) * healthMultiplier);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.characters.Remove(this.gameObject);
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.HourChanged -= ApplyHourlyDecay;
            TimeManager.Instance.HourChanged -= UpdateHealth;
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

    public int Nutrition
    {
        get => nutrition;
        set
        {
            if (nutrition != value)
            {
                nutrition = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Hygiene
    {
        get => hygiene;
        set
        {
            if (hygiene != value)
            {
                hygiene = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    public int Energy
    {
        get => energy;
        set
        {
            if (energy != value)
            {
                energy = Mathf.Clamp(value, 0, 100);
                OnAnyStatChanged?.Invoke(this);
            }
        }
    }

    // Helpers now use properties
    private int ApplyGrowth(PrimaryAttribute attr, int change)
    {
        if (change > 0 && primaryAttribute == attr)
        {
            return Mathf.RoundToInt(change * growthRate);
        }
        return change;
    }

    public void ChangeHealth(int change) => Health += change;
    public void ChangeStability(int change) => Stability += ApplyGrowth(PrimaryAttribute.Stability, change);
    public void ChangeLearning(int change) => Learning += ApplyGrowth(PrimaryAttribute.Learning, change);
    public void ChangeWorkReadiness(int change) => WorkReadiness += ApplyGrowth(PrimaryAttribute.WorkReadiness, change);
    public void ChangeTrust(int change) => Trust += ApplyGrowth(PrimaryAttribute.Trust, change);
    public void ChangeNutrition(int change) => Nutrition += ApplyGrowth(PrimaryAttribute.Nutrition, change);
    public void ChangeHygiene(int change) => Hygiene += ApplyGrowth(PrimaryAttribute.Hygiene, change);
    public void ChangeEnergy(int change) => Energy += ApplyGrowth(PrimaryAttribute.Energy, change);

}
