using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class GridManager : M_Singleton<GridManager>
{
    private const string GRID_NAME_FORMAT = "Grid ({0}, {1})";
    [SerializeField] private bool deselectAtStart = true;
    [SerializeField] private VisualGrid gridPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private Vector2Int gridArea = new Vector2Int(100, 100);
    [SerializeField] private Vector3 gridOffset = Vector3.zero;
    //[SerializeField] private float colliderHalfCellSize = 0.45f;

    private VisualGrid[,] visualGrid2dArray;
    private Vector2Int configGridArea;

    protected override void Awake()
    {
        base.Awake();
        configGridArea = gridArea;
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        Vector3 position = transform.position;

        visualGrid2dArray = new VisualGrid[(int)position.x + configGridArea.x, (int)position.z + configGridArea.y];
        
        for (int y = (int)position.z; y < position.z + configGridArea.y; y++)
        {
            for (int x = (int)position.x; x < position.x + configGridArea.x; x++)
            {
                VisualGrid grid = Instantiate(gridPrefab, new Vector3(x, transform.position.y, y) + gridOffset, Quaternion.identity);
                grid.transform.SetParent(parent, true);
                grid.transform.name = string.Format(GRID_NAME_FORMAT, x, y);
                visualGrid2dArray[x, y] = grid;
                if (deselectAtStart) visualGrid2dArray[x, y].Deselect();
            }
        }
    }

    public VisualGrid GetGridAtPosition(int x, int y)
    {
        return visualGrid2dArray[x, y]; 
    }

    public List<VisualGrid> GetSurroundingCells(Vector2Int centerPos, int radius, bool ignoreObstacles)
    {
        List<VisualGrid> surroundingCells = new List<VisualGrid>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // Calculate the distance from the center cell
                if (x == 0 && y == 0) continue; // Skip the center cell

                if (x * x + y * y < radius * radius) // Check if within circular radius
                {
                    // Calculate the neighbor's grid position
                    int neighborX = centerPos.x + x;
                    int neighborY = centerPos.y + y;

                    // Check if within grid bounds
                    if (neighborX >= 0 && neighborX < configGridArea.x && neighborY >= 0 && neighborY < configGridArea.y)
                    {
                        //Don't add the cell if it collides with an obstacle [Dung].
                        if (!ignoreObstacles && !IsValidCell(new Vector3(neighborX, 0, neighborY)))
                            continue;

                        surroundingCells.Add(visualGrid2dArray[neighborX, neighborY]);
                    }
                }
            }
        }

        return surroundingCells;
    }

    public List<Vector3> GetSurroundingPositionList(Vector3 center, int radius, bool ignoreObstacles)
    {
        Vector2Int centerInt = MathUtil.RoundVector2(new Vector2(center.x, center.z), 1);
        List<VisualGrid> availableGridList = GetSurroundingCells(centerInt, radius, ignoreObstacles);  
        List<Vector3> positionList = new List<Vector3>();
        availableGridList.ForEach(grid => positionList.Add(grid.transform.position));
        return positionList;
    }

    public void EnableSurroundingCells(Vector3 centerVector3, int radius)
    {
        Vector2Int center = MathUtil.RoundVector2(new Vector2(centerVector3.x, centerVector3.z), 1);
        List<VisualGrid> gridCells = GetSurroundingCells(center, radius, false);
        gridCells.ForEach(cell => cell.Select());
    }

    public void DisableAllCells()
    {
        for (int y = 0; y < visualGrid2dArray.GetLength(1); y++)
        {
            for (int x = 0; x < visualGrid2dArray.GetLength(0); x++)
            {
                if (visualGrid2dArray[x, y].IsActive)
                    visualGrid2dArray[x, y].Deselect();
            }
        }
    }

    public static bool IsValidCell(Vector3 position)
    {
        Collider[] obstacles = Physics.OverlapBox(position, new Vector3(0.45f, 0.5f, 0.45f), Quaternion.identity, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.GridObstacle));

        return obstacles.Length == 0;
    }

    /// <summary>
    /// If the selected cell is occupied, try to get the surrounding cells (This is used for AI)
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetValidCell(Vector3 position)
    {
        if (!IsValidCell(position))
        {
            Vector3 rightOffset = new Vector3(1, 0, 0);
            Vector3 leftOffset = new Vector3(-1, 0, 0);
            Vector3 forwardOffset = new Vector3(0, 0, 1);
            Vector3 backOffset = new Vector3(0, 0, -1);
            Vector3 flOffset = new Vector3(-1, 0, 1);
            Vector3 frOffset = new Vector3(1, 0, 1);
            Vector3 blOffset = new Vector3(-1, 0, -1);
            Vector3 brOffset = new Vector3(1, 0, -1);

            List<Vector3> possiblePosList = new List<Vector3>();
            possiblePosList.Add(rightOffset);
            possiblePosList.Add(leftOffset);
            possiblePosList.Add(forwardOffset);
            possiblePosList.Add(backOffset);
            possiblePosList.Add(flOffset);
            possiblePosList.Add(frOffset);
            possiblePosList.Add(blOffset);
            possiblePosList.Add(brOffset);

            List<Vector3> validPosList = new List<Vector3>();

            possiblePosList.ForEach(pos => { 
                if (IsValidCell(pos))
                    validPosList.Add(pos);
            });

            if (validPosList.Count > 0)
            {
                return position + validPosList[Random.Range(0, validPosList.Count - 1)];
            }
            else
            {
                Debug.LogError($"No valid offset for center {position}");
                return position;
            }
            
        }
        else
        {
            return position;
        }
    }
}
