using System;
using UnityEngine;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

[DisallowMultipleComponent]
public class OutlineController : MonoBehaviour
{
    [Header("Shader Graph Property Names (Reference Names)")]
    [Tooltip("Reference name of the outline color property in the Shader Graph.")]
    [SerializeField] private string outlineColorProperty = "_OutlineColor";
    [Tooltip("Optional reference name of an outline width/thickness property (float).")]
    [SerializeField] private string outlineWidthProperty = "_OutlineWidth";

    [Header("Defaults")]
    [Tooltip("Color used when enabling the outline if no color is provided.")]
    [SerializeField] private Color defaultSelectedOutlineColor = Color.white;
    [Tooltip("Color used when enabling the outline if no color is provided.")]
    [SerializeField] private Color defaultHoverOutlineColor = Color.white;
    [Tooltip("Width used when enabling (if width property exists).")]
    [SerializeField] private float defaultOutlineWidth = 0.1f;
    [Tooltip("Width applied when disabling.")]
    [SerializeField] private float disabledOutlineWidth = 0f;

    [Header("Hover Settings")]
    [Tooltip("If true, outline will be enabled on mouse hover.")]
    [SerializeField] private bool enableOnHover = true;

    [Header("Runtime State (Read Only)")]
    [SerializeField] private bool outlineEnabled;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    private int _colorId;
    private int _widthId;

    private bool _hasColor;
    private bool _hasWidth;

    private bool _isSelected = false;  

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogWarning($"{nameof(OutlineController)}: No Renderer found on {name}.");
            enabled = false;
            return;
        }

        _mpb = new MaterialPropertyBlock();

        CachePropertyIdsAndAvailability();
        DisableOutline();
    }

    private void CachePropertyIdsAndAvailability()
    {   

        if (!string.IsNullOrEmpty(outlineColorProperty))
        {
            _colorId = Shader.PropertyToID(outlineColorProperty);
            _hasColor = HasProp(_colorId);
            if (!_hasColor)
                Debug.LogWarning($"{nameof(OutlineController)}: Outline color property '{outlineColorProperty}' not found on materials of {name}.");
        }

        if (!string.IsNullOrEmpty(outlineWidthProperty))
        {
            _widthId = Shader.PropertyToID(outlineWidthProperty);
            _hasWidth = HasProp(_widthId);
            if (!_hasWidth && !string.IsNullOrWhiteSpace(outlineWidthProperty))
                Debug.LogWarning($"{nameof(OutlineController)}: Outline width property '{outlineWidthProperty}' not found on materials of {name}.");
        }
    }

    bool HasProp(int id)
    {
        foreach (var m in _renderer.sharedMaterials)
        {
            if (m != null && m.HasProperty(id)) return true;
        }
        return false;
    }

    public void EnableOutlineCharacter()
    {
        if (_renderer == null) return;

        _renderer.GetPropertyBlock(_mpb);

        if (_hasColor)
        {
            if(_isSelected)
                _mpb.SetColor(_colorId, defaultSelectedOutlineColor);
            else
                _mpb.SetColor(_colorId, defaultHoverOutlineColor);
        }

        if (_hasWidth)
            _mpb.SetFloat(_widthId, defaultOutlineWidth);

        _renderer.SetPropertyBlock(_mpb);
        outlineEnabled = true;
    }

    public void EnableOutlineCharacter(Color outlineColor, float outlineWidth)
    {
        if (_renderer == null) return;
        _renderer.GetPropertyBlock(_mpb);
        if (_hasColor)
            _mpb.SetColor(_colorId, outlineColor);
        if (_hasWidth)
            _mpb.SetFloat(_widthId, outlineWidth);
        _renderer.SetPropertyBlock(_mpb);
        outlineEnabled = true;
    }

    public void EnableOutlineInteractable()
    {
        if (_renderer == null) return;

        _renderer.GetPropertyBlock(_mpb);

        if (_hasColor)
        {
             _mpb.SetColor(_colorId, defaultSelectedOutlineColor);
        }
        if (_hasWidth)
            _mpb.SetFloat(_widthId, defaultOutlineWidth);

        _renderer.SetPropertyBlock(_mpb);
        outlineEnabled = true;
    }

    public void DisableOutline()
    {
        if (_renderer == null) return;

        _renderer.GetPropertyBlock(_mpb);

        if (_hasWidth)
            _mpb.SetFloat(_widthId, disabledOutlineWidth);

        _renderer.SetPropertyBlock(_mpb);
        outlineEnabled = false;
    }

    public void ToggleOutline()
    {
        if (outlineEnabled) 
            DisableOutline();
        else 
            EnableOutlineCharacter();
    }

    public void Reinitialize()
    {
        CachePropertyIdsAndAvailability();

        if (outlineEnabled) 
            EnableOutlineCharacter();

        else DisableOutline();
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        if (_isSelected)
            EnableOutlineCharacter();
        else
            DisableOutline();
    }

    private void OnMouseEnter()
    {
        if (!enableOnHover) 
            return;
        if (_isSelected)
        {
            return;
        }
        EnableOutlineCharacter();
    }

    private void OnMouseExit()
    {
        if (_isSelected) 
            return;

        DisableOutline();
    }
}
