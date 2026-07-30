using LogiCard.Sim;
using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Lets a raycast hit on a board tile resolve back to its <see cref="GridCoordinate"/>
    /// without parsing the tile GameObject's name.
    /// </summary>
    public sealed class TileMarker : MonoBehaviour
    {
        public GridCoordinate Coordinate { get; private set; }

        public void Init(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
        }
    }
}
