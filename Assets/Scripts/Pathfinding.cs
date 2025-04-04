using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _walkableTilemap;
    [SerializeField] private Tilemap _obstacleTilemap;
    private Vector3Int _startPos;
    private Vector3Int _endPos;



}
