using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMap : MonoBehaviour
{
    private Renderer _renderer;
    private Vector2 _mouseTile;
    private int _mousePosX;
    private int _mousePosZ; 
    public LayerMask GroundTiles;


    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        
        if (Physics.Raycast(ray, out hit, 100, GroundTiles))
        {
            Debug.Log(hit.point.x + "," + hit.point.z);
            _mousePosX = Mathf.FloorToInt(hit.point.x);
            _mousePosZ = Mathf.FloorToInt(hit.point.z);
         
            _mouseTile = new Vector2(_mousePosX, _mousePosZ);

            _renderer.sharedMaterial.SetTextureOffset("_MainTex", -_mouseTile);
        }
        else
        {
            _renderer.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
        }
    }

    private void DefineAimPosition()
    {
       
    }
}
