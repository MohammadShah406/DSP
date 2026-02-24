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
    public RectTransform container;
    public GameObject characterItemPrefab;

    private List<CharacterStats> _characters;
    private readonly List<RectTransform> _spawnedItems = new List<RectTransform>();
    private int _currentIndex = -1;
    private bool _isTransitioning = false;
    private bool _isMouseOver = false;

    public void OnPointerEnter(PointerEventData eventData) => _isMouseOver = true;
    public void OnPointerExit(PointerEventData eventData) => _isMouseOver = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            _characters = GameManager.Instance.GetCharacterComponents();
            SetupCarousel();

            if (UIManager.Instance != null && UIManager.Instance.CurrentState != UIManager.UIState.CharacterStats)
            {
                _currentIndex = 0;
            }

            UpdateLayout(true);
            FocusCameraOnCurrent();
        }
        else
        {
            Debug.LogError("[CharacterCarousel] GameManager.Instance is null!");
        }
    }

    void Update()
    {
        if (!_isMouseOver) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool nextPressed = (InputManager.Instance != null && InputManager.Instance.NextCharacterInput);
        bool prevPressed = (InputManager.Instance != null && InputManager.Instance.PreviousCharacterInput);

        if (!_isTransitioning)
        {
            if (scroll > 0 || prevPressed)
            {
                MovePrev();
            }
            else if (scroll < 0 || nextPressed)
            {
                MoveNext();
            }
        }
    }

    void SetupCarousel()
    {
        if (_characters == null || _characters.Count == 0) return;

        foreach (var character in _characters)
        {
            GameObject go = Instantiate(characterItemPrefab, container);

            Image img = go.GetComponentInChildren<Image>();
            if (img != null && character.characterIcon != null)
            {
                img.sprite = character.characterIcon;
            }

            if (go.GetComponent<CanvasGroup>() == null)
            {
                go.AddComponent<CanvasGroup>();
            }

            _spawnedItems.Add(go.GetComponent<RectTransform>());
        }
    }

    public void MoveNext()
    {
        if (_characters == null || _characters.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % _characters.Count;
        UpdateLayout(false);
        FocusCameraOnCurrent();
    }

    public void MovePrev()
    {
        if (_characters == null || _characters.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + _characters.Count) % _characters.Count;
        UpdateLayout(false);
        FocusCameraOnCurrent();
    }

    public void SetCurrentCharacter(CharacterStats character)
    {
        if (_characters == null || _characters.Count == 0)
            return;

        int idx = character == null ? -1 : _characters.IndexOf(character);
        if (idx != -1 && idx != _currentIndex)
        {
            _currentIndex = idx;
            UpdateLayout(false);
            FocusCameraOnCurrent();
        }
        else if (character == null)
        {
            // Optionally, deselect all or reset carousel visuals here if needed
        }
    }

    void FocusCameraOnCurrent()
    {
        if (_currentIndex >= 0 && _currentIndex < _characters.Count)
        {
            var character = _characters[_currentIndex];
            if (character != null && CameraBehaviour.Instance != null)
            {
                CameraBehaviour.Instance.SetFocussed(character.gameObject);
            }
        }
    }

    IEnumerator TransitionLayout()
    {
        _isTransitioning = true;
        float t = 0f;
        const float duration = 0.2f;
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();
        List<float> startScales = new List<float>();
        List<float> endScales = new List<float>();
        List<float> startAlphas = new List<float>();
        List<float> endAlphas = new List<float>();

        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            var item = _spawnedItems[i];
            startPositions.Add(item.anchoredPosition);
            startScales.Add(item.localScale.x);
            startAlphas.Add(item.GetComponent<CanvasGroup>().alpha);

            int offset = i - _currentIndex;
            float scale = (offset == 0) ? centerScale : sideScale;
            float alpha = (offset == 0) ? 1f : sideAlpha;
            float x = offset * horizontalSpacing;

            endPositions.Add(new Vector3(x, 0, 0));
            endScales.Add(scale);
            endAlphas.Add(alpha);
        }

        while (t < duration)
        {
            t += Time.deltaTime * transitionSpeed;
            float lerp = Mathf.Clamp01(t / duration);

            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                var item = _spawnedItems[i];
                item.anchoredPosition = Vector3.Lerp(startPositions[i], endPositions[i], lerp);
                float scale = Mathf.Lerp(startScales[i], endScales[i], lerp);
                item.localScale = new Vector3(scale, scale, 1f);
                item.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(startAlphas[i], endAlphas[i], lerp);
            }

            yield return null;
        }

        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            var item = _spawnedItems[i];
            item.anchoredPosition = endPositions[i];
            float scale = endScales[i];
            item.localScale = new Vector3(scale, scale, 1f);
            item.GetComponent<CanvasGroup>().alpha = endAlphas[i];
        }

        _isTransitioning = false;
    }

    void UpdateLayout(bool immediate)
    {
        if (_spawnedItems.Count == 0) return;

        if (!immediate)
        {
            StopAllCoroutines();
            StartCoroutine(TransitionLayout());
            return;
        }

        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            var item = _spawnedItems[i];
            int offset = i - _currentIndex;
            float scale = (offset == 0) ? centerScale : sideScale;
            float alpha = (offset == 0) ? 1f : sideAlpha;
            float x = offset * horizontalSpacing;

            item.anchoredPosition = new Vector3(x, 0, 0);
            item.localScale = new Vector3(scale, scale, 1f);
            item.GetComponent<CanvasGroup>().alpha = alpha;
        }
    }
}
