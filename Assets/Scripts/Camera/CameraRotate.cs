using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotate : MonoBehaviour
{

    void Start()
    {

    }

    private void LateUpdate()
    {
        
    }

    private IEnumerator RotateCamera()
    {
        yield return null;
    }

    
    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
    }
}
