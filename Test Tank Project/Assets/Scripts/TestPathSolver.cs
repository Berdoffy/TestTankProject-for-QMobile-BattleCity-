using System.Collections.Generic;
using UnityEngine;

public class TestPathSolver : MonoBehaviour
{

  public Transform From;

  public Transform To;

  public PathFinding PF;

  public Grid grid;

  List<Node> path;

  // Start is called before the first frame update
  void Start()
  {
    /*Node start = grid.GetNodeFromGrid(From.position);
    Node end = grid.GetNodeFromGrid(To.position);*/
    path = PF.CreatePath(From.position, To.position, 1);
  }

  private void OnDrawGizmos()
  {
    if (path != null)
    {
      foreach(Node n in path)
      {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(n.Position, Vector2.one * grid.NodeRadius * 2);
      }
    }
  }
}
