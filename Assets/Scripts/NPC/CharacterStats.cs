using UnityEngine;

public class Character : MonoBehaviour
{
    public string characterName;

    [SerializeField] private int health;
    public int Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, maxHealth);
    }
    public int maxHealth = 100;

    [SerializeField] private int stamina;
    public int Stamina
    {
        get => stamina;
        set => stamina = Mathf.Clamp(value, 0, maxStamina);
    }
    public int maxStamina = 100;

    public virtual void DisplayStats()
    {
        Debug.Log($"Name: {characterName}, Health: {health}/{maxHealth}, Stamina: {stamina}/{maxStamina}");
    }

    private void Awake()
    {
        Health = health;
        Stamina = stamina;
    }
}
