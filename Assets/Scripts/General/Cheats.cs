using UnityEngine;
using UnityEngine.InputSystem;

public class Cheats : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(InputManager.Instance.CheatSpeedInput)
        {
            Debug.Log("Toggling Cheat Speed");
            TimeManager.Instance.ToggleCheatSpeed();
        }

        if(InputManager.Instance.CheatHopeInput)
        {
            Debug.Log("Toggling Cheat Hope");
            GameManager.Instance.ToggleCheatHope();
        }
    }
}
