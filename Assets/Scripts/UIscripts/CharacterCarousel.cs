using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CharacterCarousel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float transitionSpeed = 10f;
    public float sideScale = 0.8f;
    public float centerScale = 2.0f;
    public float sideAlpha = 0.5f;
    public float horizontalSpacing = 200f;

    [Header("References")]
    public RectTransform container; // The parent transform for character items
    public GameObject characterItemPrefab;
    public GameObject nullCharacterPrefab;

    private List<CharacterStats> _characters;
    private readonly List<RectTransform> _spawnedItems = new List<RectTransform>();
    private RectTransform _nullItem;
    private int _currentIndex = -1;
    private bool _isTransitioning = false;
    private bool _isMouseOver = false;
    private bool _suppressCameraUpdate = false;

    public void OnPointerEnter(PointerEventData eventData) => _isMouseOver = true;
    public void OnPointerExit(PointerEventData eventData) => _isMouseOver = false;

    void Start()
    {
        // Get characters from GameManager
        if (GameManager.Instance != null)
        {
            _characters = GameManager.Instance.GetCharacterComponents();
            SetupCarousel();
            
            if (UIManager.Instance != null && UIManager.Instance.CurrentState != UIManager.UIState.CharacterStats)
            {
                _currentIndex = 0;
            }
            
            UpdateLayout(true); // Immediate snap on start
        }
        else
        {
            Debug.LogError("[CharacterCarousel] GameManager.Instance is null!");
        }
    }

    void Update()
    {
        if (!_isMouseOver) return;

        // Detect Mouse Scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool nextPressed = (InputManager.Instance != null && InputManager.Instance.NextCharacterInput);
        bool prevPressed = (InputManager.Instance != null && InputManager.Instance.PreviousCharacterInput);

        if (!_isTransitioning)
        {
            if (scroll > 0 || prevPressed) MovePrev();
            else if (scroll < 0 || nextPressed) MoveNext();
        }
    }

    void SetupCarousel()
    {
        if (_characters == null || _characters.Count == 0) return;
        
        if (nullCharacterPrefab != null)
        {
            GameObject nullGo = Instantiate(nullCharacterPrefab, container);
            if (nullGo.GetComponent<CanvasGroup>() == null)
            {
                nullGo.AddComponent<CanvasGroup>();
            }
            _nullItem = nullGo.GetComponent<RectTransform>();
        }
        
        foreach (var character in _characters)
        {
            GameObject go = Instantiate(characterItemPrefab, container);
            
            // Try to set the icon if characterIcon exists and there is an Image component
            Image img = go.GetComponentInChildren<Image>();
            if (img != null && character.characterIcon != null)
            {
                img.sprite = character.characterIcon;
            }
            
            // Add CanvasGroup if missing to support alpha fading
            if (go.GetComponent<CanvasGroup>() == null)
            {
                go.AddComponent<CanvasGroup>();
            }

            _spawnedItems.Add(go.GetComponent<RectTransform>());
        }
    }

    public void MoveNext()
    {
        if (_characters == null) return;
        
        // Cycle: -1 (null) -> 0 -> 1 -> ... -> count-1 -> -1
        _currentIndex++;
        if (_currentIndex >= _characters.Count)
        {
            _currentIndex = 0;
        }

        StartCoroutine(TransitionLayout());
        UpdateCameraFocus();
    }

    public void MovePrev()
    {
        if (_characters == null) return;
        
        // Cycle backwards: -1 (null) -> count-1 -> ... -> 1 -> 0 -> -1
        _currentIndex--;
        if (_currentIndex < 0)
        {
            _currentIndex = _characters.Count - 1;
        }

        StartCoroutine(TransitionLayout());
        UpdateCameraFocus();
    }

    private void UpdateCameraFocus()
    {
        if (_suppressCameraUpdate) return;

        if (CameraBehaviour.Instance == null) return;

        //if (_currentIndex == -1)
        //{
        //    // Deselect character in camera
        //    CameraBehaviour.Instance.DeselectCharacter();
        //}
        if (_currentIndex >= 0 && _currentIndex < _characters.Count)
        {
            // Focus on the selected character
            CharacterStats selectedChar = _characters[_currentIndex];
            if (selectedChar != null)
            {
                CameraBehaviour.Instance.SetFocussed(selectedChar.gameObject);
                
                // Update UI Manager stats display
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateCharacterStatsDisplay(selectedChar);
                }
            }
        }
    }
    
    IEnumerator TransitionLayout()
    {
        _isTransitioning = true;
        float elapsed = 0;
        float duration = 1f / transitionSpeed;

        // Store starting states
        int totalItems = _spawnedItems.Count + (_nullItem != null ? 1 : 0);
        Vector3[] startPos = new Vector3[totalItems];
        Vector3[] startScale = new Vector3[totalItems];
        float[] startAlpha = new float[totalItems];

        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            startPos[i] = _spawnedItems[i].anchoredPosition;
            startScale[i] = _spawnedItems[i].localScale;
            CanvasGroup cg = _spawnedItems[i].GetComponent<CanvasGroup>();
            startAlpha[i] = cg != null ? cg.alpha : 1f;
        }

        if (_nullItem != null)
        {
            int nullIndex = _spawnedItems.Count;
            startPos[nullIndex] = _nullItem.anchoredPosition;
            startScale[nullIndex] = _nullItem.localScale;
            CanvasGroup cg = _nullItem.GetComponent<CanvasGroup>();
            startAlpha[nullIndex] = cg != null ? cg.alpha : 1f;
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            // Lerp character items
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                TargetState target = GetTargetStateForCharacter(i);
                _spawnedItems[i].anchoredPosition = Vector3.Lerp(startPos[i], target.Pos, t);
                _spawnedItems[i].localScale = Vector3.Lerp(startScale[i], target.Scale, t);
                
                CanvasGroup cg = _spawnedItems[i].GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = Mathf.Lerp(startAlpha[i], target.Alpha, t);
            }

            // Lerp null item
            if (_nullItem != null)
            {
                int nullIndex = _spawnedItems.Count;
                TargetState target = GetTargetStateForNull();
                _nullItem.anchoredPosition = Vector3.Lerp(startPos[nullIndex], target.Pos, t);
                _nullItem.localScale = Vector3.Lerp(startScale[nullIndex], target.Scale, t);
                
                CanvasGroup cg = _nullItem.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = Mathf.Lerp(startAlpha[nullIndex], target.Alpha, t);
            }

            yield return null;
        }

        UpdateLayout(true); // Final snap
        _isTransitioning = false;

        // Inform UIManager about the change
        if (UIManager.Instance != null && _characters != null)
        {
            if (_currentIndex >= 0 && _currentIndex < _characters.Count)
            {
                UIManager.Instance.UpdateCharacterStatsDisplay(_characters[_currentIndex]);
            }
            else if (_currentIndex == -1)
            {
                // Optionally call a clear method if UIManager needs to know
                UIManager.Instance.UpdateCharacterStatsDisplay(null);
            }
        }
    }

    void UpdateLayout(bool immediate)
    {
        if (_spawnedItems.Count == 0) return;

        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            TargetState target = GetTargetStateForCharacter(i);
            _spawnedItems[i].anchoredPosition = target.Pos;
            _spawnedItems[i].localScale = target.Scale;
            
            CanvasGroup cg = _spawnedItems[i].GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = target.Alpha;

            // Handle sorting order (Center item on top)
            if (i == _currentIndex) _spawnedItems[i].SetAsLastSibling();
        }
        
        if (_nullItem != null)
        {
            TargetState target = GetTargetStateForNull();
            _nullItem.anchoredPosition = target.Pos;
            _nullItem.localScale = target.Scale;
            
            CanvasGroup cg = _nullItem.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = target.Alpha;

            if (_currentIndex == -1) _nullItem.SetAsLastSibling();
        }
        
    }

    private TargetState GetTargetStateForCharacter(int characterIndex)
    {
        TargetState state = new TargetState();
        
        if (_currentIndex == -1)
        {
            // Null is selected, characters are positioned normally but offset
            // Treat as if we're at position -1, so character 0 is at offset +1
            int offset = characterIndex + 1;
            state.Pos = new Vector2(offset * horizontalSpacing, 0);
            state.Scale = Vector3.one * sideScale;
            state.Alpha = sideAlpha;
        }
        else
        {
            // A character is selected
            int offset = characterIndex - _currentIndex;
            int count = _characters.Count + 1; // Include null slot in wrapping

            // Circular wrapping
            if (offset > count / 2) offset -= count;
            if (offset < -count / 2) offset += count;

            state.Pos = new Vector2(offset * horizontalSpacing, 0);
            state.Scale = (offset == 0) ? Vector3.one * centerScale : Vector3.one * sideScale;
            state.Alpha = (offset == 0) ? 1.0f : sideAlpha;
        }
        
        return state;
    }

    private TargetState GetTargetStateForNull()
    {
        TargetState state = new TargetState();
        
        if (_currentIndex == -1)
        {
            // Null is centered
            state.Pos = Vector2.zero;
            state.Scale = Vector3.one * centerScale;
            state.Alpha = 1.0f;
        }
        else
        {
            // Null is to the left of character 0
            int offset = -1 - _currentIndex;
            int count = _characters.Count + 1;

            if (offset > count / 2) offset -= count;
            if (offset < -count / 2) offset += count;

            state.Pos = new Vector2(offset * horizontalSpacing, 0);
            state.Scale = Vector3.one * sideScale;
            state.Alpha = sideAlpha;
        }
        
        return state;
    }

    struct TargetState
    {
        public Vector2 Pos;
        public Vector3 Scale;
        public float Alpha;
    }

    public CharacterStats GetCurrentCharacter()
    {
        if (_characters != null && _currentIndex >= 0 && _currentIndex < _characters.Count)
            return _characters[_currentIndex];
        return null;
    }

    public void SetCurrentCharacter(CharacterStats character)
    {
        if (_characters == null) return;

        _suppressCameraUpdate = true; // Prevent circular updates

        int newIndex = (character == null) ? -1 : _characters.IndexOf(character);
        
        if (newIndex != _currentIndex)
        {
            _currentIndex = newIndex;
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(TransitionLayout());
            }
            else
            {
                UpdateLayout(true);
            }
        }

        _suppressCameraUpdate = false;
    }
}
