using UnityEngine;
using UnityEngine.UI;

public class StatBars : MonoBehaviour
{
    public Image healthFill;
    public Image staminaFill;

    public void UpdateBars(float currentHealthPct, float currentStaminaPct)
    {
        healthFill.fillAmount = Mathf.Clamp01(currentHealthPct);
        staminaFill.fillAmount = Mathf.Clamp01(currentStaminaPct);
    }
}
