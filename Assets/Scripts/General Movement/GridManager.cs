using UnityEngine;

public class GridManager : MonoBehaviour
{
    private Grid _grid;
    public static Grid Instance {get; private set;}
    private void Awake()
    {
        _grid = GetComponent<Grid>();
        if (Instance == null)
        {
            Instance = _grid;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
