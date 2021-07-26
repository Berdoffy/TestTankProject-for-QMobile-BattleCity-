using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

  Grid grid;

  private void Awake()
  {
    grid = GetComponent<Grid>();
  }

  public bool IsInWorld(Vector2 point)
  {
    if (point.x < -grid.GridSizeX/2 || point.x > grid.GridSizeX/2 || point.y < -grid.GridSizeY/2 || point.y > grid.GridSizeY/2)
      return false;
    else
      return true;
  }

  public List<Node> CreatePath(Vector2 start, Vector2 finish, float corridorRadius = 0)
  {
    Node startNode = grid.GetNodeFromGrid(start);
    Node endNode = grid.GetNodeFromGrid(finish);

    Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
    HashSet<Node> closedSet = new HashSet<Node>();
    Node closestNode = startNode;
    int currentMinHCost = int.MaxValue;

    openSet.Add(startNode);
    while (openSet.Count > 0)
    {
      Node currentNode = openSet.RemoveFirst();
      closedSet.Add(currentNode);

      if (currentNode == endNode)
        return RetracePath(startNode, endNode);

      foreach (Node node in grid.GetNeighbours(currentNode))
      {
        if ((!node.Walkable && node != endNode) || closedSet.Contains(node))
          continue;

        int distance = GetDistance(currentNode, node);
        if ((currentNode.MaxCorridorRadius < corridorRadius) /*|| distance == 14*/ && 
          CheckForDiagonalRestriction(currentNode, node))
          continue;

        int newDistCost = currentNode.gCost + distance;
        if (newDistCost < node.gCost || !openSet.Contains(node))
        {
          node.FitToCorridor = node.MaxCorridorRadius >= corridorRadius;
          node.gCost = newDistCost;
          node.hCost = GetDistance(node, endNode);
          node.Parent = currentNode;
          if (node.hCost < currentMinHCost)
          {
            currentMinHCost = node.hCost;
            closestNode = node;
          }
            
          if (!openSet.Contains(node))
            openSet.Add(node);
        }        
      }
      if (openSet.Count == 0 && closestNode != startNode)
        return RetracePath(startNode, closestNode);
    }
    return new List<Node>();
  }

  List<Node> RetracePath(Node start, Node end)
  {
    List<Node> path = new List<Node>();
    Node currentNode = end;
    while (currentNode != start)
    {
      path.Add(currentNode);
      currentNode = currentNode.Parent;
    }
    path.Reverse();
    return path;
  }

  int GetDistance(Node nodeA, Node nodeB)
  {
    int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
    int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

    if (distX > distY)
      return 14 * distY + 10 * (distX - distY);
    else
      return 14 * distX + 10 * (distY - distX);
  }

  bool CheckForDiagonalRestriction(Node nodeA, Node nodeB)
  {
    int distX = nodeA.gridX - nodeB.gridX;
    int distY = nodeA.gridY - nodeB.gridY;

    int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

    if(distX < 0 && distY < 0)
    {
      x1 = nodeA.gridX - 1;
      y1 = nodeA.gridY;
      x2 = nodeA.gridX;
      y2 = nodeA.gridY - 1;
    }
    else if(distX < 0 && distY > 0)
    {
      x1 = nodeA.gridX - 1;
      y1 = nodeA.gridY;
      x2 = nodeA.gridX;
      y2 = nodeA.gridY + 1;
    }
    else if (distX > 0 && distY > 0)
    {
      x1 = nodeA.gridX + 1;
      y1 = nodeA.gridY;
      x2 = nodeA.gridX;
      y2 = nodeA.gridY + 1;
    }
    else if (distX > 0 && distY < 0)
    {
      x1 = nodeA.gridX + 1;
      y1 = nodeA.gridY;
      x2 = nodeA.gridX;
      y2 = nodeA.gridY - 1;
    }

    if (x1 < grid.GridSizeX && y1 < grid.GridSizeY && x1 >= 0 && y1 >= 0 && !grid.grid[x1, y1].Walkable)
      return true;
    if (x2 < grid.GridSizeX && y2 < grid.GridSizeY && x2 >= 0 && y2 >= 0 && !grid.grid[x2, y2].Walkable)
      return true;

    return false;
  }
}
