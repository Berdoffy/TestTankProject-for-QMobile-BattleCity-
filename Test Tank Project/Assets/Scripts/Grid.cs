using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class Grid : MonoBehaviour
{

  List<Node> Nodes = new List<Node>();

  [HideInInspector]
  public Node[,] grid;

  public LayerMask ObstacleMask;

  public bool EditorView;

  public float NodeRadius;

  public Vector2 GridSize;

  public Vector2 Deviatiation;

  public float MinCorridorRadius;

  public float MaxCarridorRadius;

  float nodeDiametr;
  [HideInInspector]
  public int GridSizeX, GridSizeY;

  public delegate void GridUpdate();
  public event GridUpdate OnGridUpdate;

  // Use this for initialization
  void Start ()
  {
    nodeDiametr = NodeRadius * 2;
    GridSizeX = Mathf.RoundToInt(GridSize.x / nodeDiametr);
    GridSizeY = Mathf.RoundToInt(GridSize.y / nodeDiametr);   
    grid = new Node[GridSizeX, GridSizeY];
    CreateGrid();
    //if (DrawPnts)
    //  DrawPoints();
  }

  public int MaxSize
  {
    get
    {
      return GridSizeX * GridSizeY;
    }
  }

  void CreateGrid()
  {
    Vector3 realPosition = new Vector3(transform.position.x + Deviatiation.x, transform.position.y + Deviatiation.y, 0);
    Vector2 worldBottomLeft = (Vector2)realPosition - Vector2.right * GridSize.x / 2 - Vector2.up * GridSize.y/2;
    for (int x = 0; x < GridSizeX; x++)
    {
      for (int y = 0; y < GridSizeY; y++)
      {
        Vector2 point = worldBottomLeft + Vector2.right * (x * nodeDiametr + NodeRadius) + Vector2.up * (y * nodeDiametr + NodeRadius);
        Collider2D[] col = Physics2D.OverlapCircleAll(point, NodeRadius, ObstacleMask);
        Node node = new Node
        {
          Position = point,
          Walkable = col.Length == 0,
          gridX = x,
          gridY = y,
        };
        if (node.Walkable)
        {
          node.MaxCorridorRadius = SetCorridorRadiusForNode(point, x, y);
          if (node.MaxCorridorRadius < MinCorridorRadius)
            node.Walkable = false;
        }
        else
          node.MaxCorridorRadius = 0;
        grid[x, y] = node;        
        //Nodes.Add(node);
      }
    }
  }

  int SetEfficiency(Vector2 point)
  {
    int result = 0;
    for(int i = 0; i < 3; i++)
    {
      int coaf = 0;
      switch(i)
      {
        case 0: coaf = 3; break;
        case 1: coaf = 5; break;
        case 2: coaf = 7; break;
      }
      Collider2D[] col = Physics2D.OverlapCircleAll(point, NodeRadius * coaf, ObstacleMask);
    /*if (col.Length > 0)
      return -5;
    else
      return 0;*/
      if (col.Length > 0)
      {
        result = i;
        break;
      }
      result = 0;
    }
    switch (result)
    {
      case 2: return 10;
      case 1: return 7;
      default: return 0;
    }
  }

  float SetCorridorRadiusForNode(Vector2 point, int x, int y)
  {
    int scanlimit = Mathf.Max(GridSizeX, GridSizeY);
    float radius = NodeRadius;
    while (radius <= MaxCarridorRadius)
    {
      radius += NodeRadius;
      Collider2D[] col = Physics2D.OverlapCircleAll(point, radius, ObstacleMask);
      if(col.Length > 0)
      {
        return radius;
      }
      if (x + radius > GridSizeX || y + radius > GridSizeY || x - radius < 0 || y - radius < 0)
        return radius;
    }
    return radius;
  }

  public bool UpdateGrid(List<Node> nodes)
  {
    bool updated = false;
    foreach (Node n in nodes)
    {
      updated = UpdateNode(n);
      if (updated)
      {
        UpdateGrid(GetNeighbours(n));
      }
    }
    
    return updated;
  }

  public bool UpdateNode(Node n)
  {
    Collider2D[] col = Physics2D.OverlapCircleAll(n.Position, NodeRadius/2, ObstacleMask);
    if(col.Length == 0)
    {
      float prevRad = n.MaxCorridorRadius;
      bool prevWalk = n.Walkable;
      n.MaxCorridorRadius = SetCorridorRadiusForNode(n.Position, n.gridX, n.gridY);
      if (n.MaxCorridorRadius < MinCorridorRadius)
        n.Walkable = false;
      else
        n.Walkable = true;
      if (prevWalk != n.Walkable || prevRad != n.MaxCorridorRadius)
        return true;
    }
    return false;
  }

  public List<Node> GetNeighbours(Node node)
  {
    List<Node> result = new List<Node>();

    for (int x = -1; x <= 1; x++)
    {
      for (int y = -1; y <= 1; y++)
      {
        if (x == 0 && y == 0)
          continue;
        int checkX = node.gridX + x;
        int checkY = node.gridY + y;
        if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY)
          result.Add(grid[checkX, checkY]);
      }
    }

    return result;
  }

  public Node GetNodeFromGrid(Vector2 worldPosition)
  {
    float percentX = (worldPosition.x + GridSize.x / 2) / GridSize.x;
    float percentY = (worldPosition.y + GridSize.y / 2) / GridSize.y;

    percentX = Mathf.Clamp01(percentX);
    percentY = Mathf.Clamp01(percentY);

    int x = Mathf.RoundToInt((GridSizeX - 1) * percentX);
    int y = Mathf.RoundToInt((GridSizeY - 1) * percentY);
    float deltaX = Mathf.Abs(grid[x, y].Position.x) - Mathf.Abs(worldPosition.x);
    float deltaY = Mathf.Abs(grid[x, y].Position.y) - Mathf.Abs(worldPosition.y);
    if(deltaX > NodeRadius || deltaY > NodeRadius || deltaX < - NodeRadius || deltaY < - NodeRadius)
    {

      if (deltaX > NodeRadius && x < GridSizeX - 1)
        x++;
      else if (deltaX < -NodeRadius && x > 0)
        x--;
      if (deltaY > NodeRadius && y < GridSizeY - 1)
        y++;
      else if (deltaY < -NodeRadius && y > 0 )
        y--;
    }

    return grid[x, y];
  }

  private void OnDrawGizmos()
  {
    Vector3 position = new Vector3(transform.position.x + Deviatiation.x, transform.position.y + Deviatiation.y, 0);
    Gizmos.DrawWireCube(position, new Vector3(GridSize.x, GridSize.y , 0));
    if (Nodes != null)
    {
      for (int x = 0; x < GridSizeX; x++)
      {
        for (int y = 0; y < GridSizeY; y++)
        {
          if (grid[x, y].Walkable)
          {
            Gizmos.color = Color.green;
          }
          else
            Gizmos.color = Color.black;
          Gizmos.DrawWireCube(grid[x, y].Position, Vector2.one * nodeDiametr);
        }
      }      
    }
  }
}
