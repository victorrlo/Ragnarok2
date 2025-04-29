using UnityEngine;

public static class DirectionHelper
{
        public static Vector3Int[] _directions = new Vector3Int[]
        {
            new Vector3Int( 0, 1, 0),   //up
            new Vector3Int( 0, -1, 0),  //down
            new Vector3Int( -1, 0, 0),  //left
            new Vector3Int( 1, 0, 0),   //right
            new Vector3Int( -1, 1, 0),  //up-left
            new Vector3Int( 1, 1, 0),   //up-right
            new Vector3Int( -1, -1, 0), //down-left
            new Vector3Int( 1, -1, 0)   //down-right   
        };
}
