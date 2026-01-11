using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivity : MonoBehaviour
{
    [SerializeField] private Slider mouseSensitivitySlider;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivity = 1f;

    private void Start()
    {
        float saved = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        mouseSensitivitySlider.value = saved;
        SetSensitivity(saved);

        mouseSensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    private void SetSensitivity(float value)
    {
        sensitivity = value;

        // Apply to input pipeline immediately
        if (InputManager.Instance != null)
            InputManager.Instance.MouseSensitivity = sensitivity;

        // Persist
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }
}
