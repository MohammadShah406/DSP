using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public Vector2 MoveInput { get; private set; }
    public Vector2 DragDeltaInput { get; private set; }
    public bool DragInput { get; private set; }
    public bool SelectInput { get; private set; }
    public bool DeselectInput { get; private set; }
    public bool ResetInput { get; private set; }
    public float ZoomInput { get; private set; }   // +1 (E), -1 (Q)
    public bool NextCharacterInput { get; private set; }
    public bool PreviousCharacterInput { get; private set; }
    public bool PauseInput { get; private set; }
    public bool InventoryInput { get; private set; }
    public bool SpeedInput { get; private set; }
    public bool CheatSpeedInput { get; private set; }
    public bool CheatHopeInput { get; private set; }

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1f;
    public float MouseSensitivity
    {
        get { return mouseSensitivity; }
        set { mouseSensitivity = value; }
    }


    private PlayerInput _playerInput;

    private InputAction Movement;
    private InputAction LookDrag;
    private InputAction Drag;
    private InputAction Select;
    private InputAction Deselect;
    private InputAction Reset;
    private InputAction ZoomIn;
    private InputAction ZoomOut;
    private InputAction NextCharacter;
    private InputAction PreviousCharacter;
    private InputAction Pause;
    private InputAction Inventory;
    private InputAction Speed;
    private InputAction CheatSpeed;
    private InputAction CheatHope;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        SetupActions();
    }

    private void Update()
    {
        UpdateActions();
    }

    private void SetupActions()
    {
        Movement = _playerInput.actions["Movement"];
        LookDrag = _playerInput.actions["DragDelta"];
        Drag = _playerInput.actions["Drag"];
        Select = _playerInput.actions["Select"];
        Deselect = _playerInput.actions["Deselect"];
        Reset = _playerInput.actions["Reset"];
        ZoomIn = _playerInput.actions["ZoomIn"];
        ZoomOut = _playerInput.actions["ZoomOut"];
        NextCharacter = _playerInput.actions["NextCharacter"];
        PreviousCharacter = _playerInput.actions["PreviousCharacter"];
        Pause = _playerInput.actions["Pause"];
        Inventory = _playerInput.actions["Inventory"];
        Speed = _playerInput.actions["Speed"];
        CheatSpeed = _playerInput.actions["CheatSpeed"];
        CheatHope = _playerInput.actions["CheatHope"];
    }

    private void UpdateActions()
    {
        MoveInput = Movement.ReadValue<Vector2>();
        DragDeltaInput = LookDrag.ReadValue<Vector2>();

        DragInput = Drag.IsPressed();                // hold for dragging
        SelectInput = Select.triggered;
        DeselectInput = Deselect.triggered;
        ResetInput = Reset.triggered;

        // Zoom: Q = -1, E = +1
        ZoomInput = 0f;
        if (ZoomIn.IsPressed()) ZoomInput -= 1f;
        if (ZoomOut.IsPressed()) ZoomInput += 1f;

        NextCharacterInput = NextCharacter.triggered;
        PreviousCharacterInput = PreviousCharacter.triggered;
        PauseInput = Pause.triggered;
        InventoryInput = Inventory.triggered;
        SpeedInput = Speed.triggered;
        CheatSpeedInput = CheatSpeed.triggered;
        CheatHopeInput = CheatHope.triggered;
    }
}
