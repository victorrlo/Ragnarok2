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
        _hotkeyNumber3.action.performed += SkillHotkey3Clicked;
        _hotkeyNumber4.action.performed += SkillHotkey4Clicked;
    }

    private void OnDestroy()
    {
        _hotkeyNumber1.action.performed -= ItemHotkeyClicked;
    }

    private void ItemHotkeyClicked(InputAction.CallbackContext callbackContext)
    {
        ItemController.Instance.TryUseApple?.Invoke(true);
    }

    private void SkillHotkey2Clicked(InputAction.CallbackContext callbackContext)
    {
        if (SkillController.Instance.HasWaterBodySkill)
        {
            Debug.Log("use skill 2");
        }
    }

    private void SkillHotkey3Clicked(InputAction.CallbackContext callbackContext)
    {
        if (SkillController.Instance.HasWaterBodySkill)
        {
            var player = GameObject.FindWithTag("Player");
            SkillController.Instance.TryUsingStompPuddle?.Invoke(player, true);
        }
    }

    private void SkillHotkey4Clicked(InputAction.CallbackContext callbackContext)
    {
         // I need to prevent these actions if the conditions needed are not met.
        // player has skill 4 already?
        // player has SP?
        Debug.Log("use skill 4");
    }

    private void Update()
    {
    }


    public void SetShortcutStructure()
    { 
        _itemShortcut1.SetActive(true);
        _skillShortcut2.SetActive(true);
        _skillShortcut3.SetActive(SkillController.Instance.HasStompPuddleSkill);
        _skillShortcut4.SetActive(SkillController.Instance.HasWaterBombSkill);
    }
}
