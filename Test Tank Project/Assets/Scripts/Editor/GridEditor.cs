using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{

  int GridSizeX, GridSizeY;
  List<Node> nodes = new List<Node>();
  Node[,] Grid;
  //Clearence[,] WallPaths;
  // Use this for initialization
  private void OnSceneGUI()
  {
    Grid grid = (Grid)target;
    if (!grid.EditorView)
      return;
    Handles.color = Color.green;

    float nodeDiametr = grid.NodeRadius * 2;
    GridSizeX = Mathf.RoundToInt(grid.GridSize.x / nodeDiametr);
    GridSizeY = Mathf.RoundToInt(grid.GridSize.y / nodeDiametr);
    Grid = new Node[GridSizeX, GridSizeY];
    if (nodes.Count == 0 && grid != null)
    {
      CreateGrid(grid, nodeDiametr);
    }
    for (int x = 0; x < GridSizeX; x++)
    {
      for (int y = 0; y < GridSizeY; y++)
      {
        Handles.DrawWireCube(Grid[x, y].Position, Vector3.one * nodeDiametr);
      }
    }
  }

  void CreateGrid(Grid grid, float nodeDiametr)
  {
    Vector3 realPosition = new Vector3(grid.transform.position.x + grid.Deviatiation.x, grid.transform.position.y + 
      grid.Deviatiation.y, 0);
    Vector2 worldBottomLeft = (Vector2)realPosition - Vector2.right * (grid.GridSize.x / 2) - Vector2.up * (grid.GridSize.y / 2);

    for (int x = 0; x < GridSizeX; x++)
    {
      for (int y = 0; y < GridSizeY; y++)
      {
        Vector2 point = worldBottomLeft + Vector2.right * (x * nodeDiametr + grid.NodeRadius) + Vector2.up * (y * nodeDiametr + grid.NodeRadius);
        Node node = new Node
        {
          Position = point,
          //Walkable = col.Length == 0
        };
        Grid[x, y] = node;
      }
    }
  }
}
