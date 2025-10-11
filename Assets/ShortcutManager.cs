using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShortcutManager : MonoBehaviour
{
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
    }

    private void Update()
    {
    }


    public void SetShortcutStructure()
    { 
        _itemShortcut1.SetActive(true);
        _skillShortcut2.SetActive(true);
        _skillShortcut3.SetActive(GameController.Instance.HasStompPuddleSkill);
        _skillShortcut4.SetActive(GameController.Instance.HasWaterBombSkill);
    }
}
