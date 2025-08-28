using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SpriteRotation : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField] private GameObject _headSprite;
    [SerializeField] private GameObject _bodySprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<GameObject> playerSprites = new List<GameObject>();
        playerSprites.Add(_headSprite);
        playerSprites.Add(_bodySprite);
        
        if (playerSprites.Any(sprite => sprite == null))
        {
            Debug.LogError($"{gameObject.name} sprites are null!");
            return;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _headSprite.transform.position = new Vector3(_headSprite.transform.position.x, _headSprite.transform.position.x, _mainCamera.transform.position.z);
        _bodySprite.transform.position = new Vector3(_bodySprite.transform.position.x, _bodySprite.transform.position.x, _mainCamera.transform.position.z);
    }
}
