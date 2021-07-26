using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
  const int RadiusBoost = 50;
  public Vector2 Position;

  public int gCost;

  public int hCost;

  public int gridX;

  public int gridY;

  public bool Walkable;

  public Node Parent;

  public float MaxCorridorRadius;

  public bool FitToCorridor;

  int heapIndex;

  public int fCost
  {
    get
    {
      return gCost + hCost;
    }
  }

  public int HeapIndex
  {
    get
    {
      return heapIndex;
    }
    set
    {
      heapIndex = value;
    }
  }

  public int CompareTo(Node node)
  {
    int compare = 0;
      compare = fCost.CompareTo(node.fCost);
    if (compare == 0)
      compare = hCost.CompareTo(node.hCost);
    return -compare;
  }
}
