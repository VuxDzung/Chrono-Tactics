using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private const string GRID_NAME_FORMAT = "Grid ({0}, {1})";
    [SerializeField] private VisualGrid gridPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private Vector2Int gridArea = new Vector2Int(100, 100);

    private VisualGrid[,] visualGrid2dArray;

    private List<Grid> gridList = new List<Grid>();

    private void Awake()
    {
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        visualGrid2dArray = new VisualGrid[gridArea.x, gridArea.y];
        for (int y = 0; y < gridArea.y; y++)
        {
            for (int x = 0; x < gridArea.x; x++)
            {
                VisualGrid grid = Instantiate(gridPrefab, new Vector3(x, 0.1f, y), Quaternion.identity);
                grid.transform.SetParent(parent, true);
                grid.transform.name = string.Format(GRID_NAME_FORMAT, x, y);
                visualGrid2dArray[x, y] = grid;
            }
        }
    }

    public VisualGrid GetGridAtPosition(int x, int y)
    {
        return visualGrid2dArray[x, y]; 
    }

    public List<VisualGrid> GetSurroundingCells(Vector2Int centerPos, int range = 1)
    {
        List<VisualGrid> surroundingCells = new List<VisualGrid>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                // Skip the center cell
                if (x == 0 && y == 0) continue;

                // Calculate the neighbor's grid position
                int neighborX = centerPos.x + x;
                int neighborY = centerPos.y + y;

                // Check if within grid bounds
                if (neighborX >= 0 && neighborX < gridArea.x && neighborY >= 0 && neighborY < gridArea.y)
                {
                    // Add the visual cell at this position
                    surroundingCells.Add(visualGrid2dArray[neighborX, neighborY]);
                }
            }
        }
        return surroundingCells;
    }
}
