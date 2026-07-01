using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShortcutManager : MonoBehaviour
{
    [SerializeField] private InputActionReference _hotkeyNumber1;
    [SerializeField] private InputActionReference _hotkeyNumber2;
    [SerializeField] private InputActionReference _hotkeyNumber3;
    [SerializeField] private InputActionReference _hotkeyNumber4;
    public static ShortcutManager Instance {get; private set;}
    [SerializeField] private GameObject _itemShortcut1;
    [SerializeField] private GameObject _skillShortcut2;
    [SerializeField] private GameObject _skillShortcut3;
    [SerializeField] private GameObject _skillShortcut4;

    public System.Action<bool> OnStartCastingSkill;
    public System.Action<bool> OnStopCastingSkill;
    private bool _isStompPuddleShortcutHeld;
    public bool IsStompPuddleShortcutHeld => _isStompPuddleShortcutHeld;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetShortcutStructure();

    }

    private void Start()
    {
        _hotkeyNumber1.action.performed += ItemHotkeyClicked;
        _hotkeyNumber2.action.performed += SkillHotkey2Clicked;
        _hotkeyNumber3.action.started += StompPuddleHotkeyStarted;
        _hotkeyNumber3.action.performed += SkillHotkey3Clicked;
        _hotkeyNumber3.action.canceled += StompPuddleHotkeyCanceled;
        _hotkeyNumber4.action.performed += SkillHotkey4Clicked;
    }

    private void OnDestroy()
    {
        _hotkeyNumber1.action.performed -= ItemHotkeyClicked;
        _hotkeyNumber2.action.performed -= SkillHotkey2Clicked;
        _hotkeyNumber3.action.started -= StompPuddleHotkeyStarted;
        _hotkeyNumber3.action.performed -= SkillHotkey3Clicked;
        _hotkeyNumber3.action.canceled -= StompPuddleHotkeyCanceled;
        _hotkeyNumber4.action.performed -= SkillHotkey4Clicked;
    }

    private void ItemHotkeyClicked(InputAction.CallbackContext callbackContext)
    {
        ItemController.Instance.TryUseApple?.Invoke(true);
    }

    private void SkillHotkey2Clicked(InputAction.CallbackContext callbackContext)
    {
        var player = GameObject.FindWithTag("Player");

        if (SkillController.Instance.HasWaterBodySkill)
        {
            SkillController.Instance.TryCastingWaterBody(player);
        }
    }

    private void SkillHotkey3Clicked(InputAction.CallbackContext callbackContext)
    {
        _isStompPuddleShortcutHeld = true;

        var player = GameObject.FindWithTag("Player");

        if (player.GetComponent<PlayerControl>().GetCurrentState() is CastingState) return; 
        
        if (SkillController.Instance.HasStompPuddleSkill)
        {
            // Debug.Log("[ShortcutManager] player has stomp puddle skill and will try using it!");
            SkillController.Instance.TryUsingStompPuddle?.Invoke(player, true);
            OnStartCastingSkill?.Invoke(true);
        }
    }

    private void StompPuddleHotkeyStarted(InputAction.CallbackContext callbackContext)
    {
        _isStompPuddleShortcutHeld = true;
    }

    private void StompPuddleHotkeyCanceled(InputAction.CallbackContext callbackContext)
    {
        _isStompPuddleShortcutHeld = false;
    }

    private void SkillHotkey4Clicked(InputAction.CallbackContext callbackContext)
    {
        var player = GameObject.FindWithTag("Player");

        if (player.GetComponent<PlayerControl>().GetCurrentState() is CastingState) return;

        if (SkillController.Instance.HasBashSkill)
        {
            PlayerControl control = player.GetComponent<PlayerControl>();
            SkillController.Instance.TryCastingBash?.Invoke(player, control.CurrentTarget);
            OnStartCastingSkill?.Invoke(true);
        }
    }

    private void Update()
    {
    }


    public void SetShortcutStructure()
    { 
        _itemShortcut1.SetActive(true);
        _skillShortcut2.SetActive(true);
        _skillShortcut3.SetActive(SkillController.Instance.HasStompPuddleSkill);
        _skillShortcut4.SetActive(SkillController.Instance.HasBashSkill);
    }
}
