using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallScript : MonoBehaviour
{
  enum CornerType
  { 
    NotACorner,
    TopLeft,
    TopRigth,
    BottomLeft,
    BottomRight
  }

  public LayerMask DamageLayerMask;

  public List<Sprite> Corners;

  public List<Sprite> DestructionParts;

  public Grid grid;

  [Min(.1f)]
  public float GridUpdateDelay = .2f;

  List<Node> NodesToUpdate;
  float gridUpdateTimer;

  Tilemap walls;

  // Start is called before the first frame update
  void Start()
  {
    NodesToUpdate = new List<Node>();
    walls = GetComponent<Tilemap>();
  }

  // Update is called once per frame
  void Update()
  {
    if (NodesToUpdate.Count > 0)
    {
      gridUpdateTimer += Time.deltaTime;
      if (gridUpdateTimer >= GridUpdateDelay)
      {
        lock (NodesToUpdate)
        {
          grid.UpdateGrid(NodesToUpdate);
          NodesToUpdate.Clear();
          gridUpdateTimer = 0;
        }
      }
    }
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (DamageLayerMask.value == (DamageLayerMask | 1 << collision.collider.gameObject.layer))
    {
       foreach(ContactPoint2D cp in collision.contacts)
      {
        //Vector3Int tilePoint = walls.layoutGrid.WorldToCell(cp.point);
        Vector3 hit = Vector3.zero;
        hit.x = cp.point.x + .5f * cp.normal.x;
        hit.y = cp.point.y + .5f * cp.normal.y;
        Vector3Int tilePoint = walls.layoutGrid.WorldToCell(hit);
        var tile = walls.GetTile(tilePoint);
        if(tile != null)
        {
          walls.SetTile(tilePoint, null);
          SetDestructionToNeighbours(tilePoint);
          lock(NodesToUpdate)
            NodesToUpdate.Add(grid.GetNodeFromGrid(new Vector2(hit.x, hit.y))); 
        }
      }
    }
  }

  private void SetDestructionToNeighbours(Vector3Int tilePoint)
  {
    for(int i = 0; i < 4; i++)
    {
      if(i == 0)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x, tilePoint.y + 1, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
        {
          Tile newTile = ScriptableObject.CreateInstance<Tile>();
          if (IsLastaOne(newPos))
            newTile.sprite = DestructionParts[4];
          else
          {
            var sprite = walls.GetSprite(newPos);
            var ct = CheckForCorner(sprite);
            newTile.sprite = SetDestuctionSprite(ct, i);
          }
          walls.SetTile(newPos, newTile);
        }
        continue;
      }
      if(i == 1)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x, tilePoint.y - 1, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
        {
          Tile newTile = ScriptableObject.CreateInstance<Tile>();
          if (IsLastaOne(newPos))
            newTile.sprite = DestructionParts[4];
          else
          {
            var sprite = walls.GetSprite(newPos);
            var ct = CheckForCorner(sprite);
            newTile.sprite = SetDestuctionSprite(ct, i);
          }
          walls.SetTile(newPos, newTile);
        }
        continue;
      }
      if (i == 2)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x - 1, tilePoint.y, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
        {
          Tile newTile = ScriptableObject.CreateInstance<Tile>();
          if (IsLastaOne(newPos))
            newTile.sprite = DestructionParts[4];
          else
          {
            var sprite = walls.GetSprite(newPos);
            var ct = CheckForCorner(sprite);
            newTile.sprite = SetDestuctionSprite(ct, i);
          }
          walls.SetTile(newPos, newTile);
        }
        continue;
      }
      if (i == 3)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x + 1, tilePoint.y, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
        {
          Tile newTile = ScriptableObject.CreateInstance<Tile>();
          if (IsLastaOne(newPos))
            newTile.sprite = DestructionParts[4];
          else
          {
            var sprite = walls.GetSprite(newPos);
            var ct = CheckForCorner(sprite);
            newTile.sprite = SetDestuctionSprite(ct, i);
          }
          walls.SetTile(newPos, newTile);
        }
        continue;
      }
    }
  }

  private CornerType CheckForCorner(Sprite sprite)
  {
    for(int i = 0; i < 4; i++)
    {
      if(sprite.GetHashCode() == Corners[i].GetHashCode())
      {
        return (CornerType)i+1;
      }
    }
    return CornerType.NotACorner;
  }

  private bool IsLastaOne(Vector3Int tilePoint)
  {
    for (int i = 0; i < 4; i++)
    {
      if (i == 0)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x, tilePoint.y + 1, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
          return false;
      }
      if (i == 1)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x, tilePoint.y - 1, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
          return false;
      }
      if (i == 2)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x - 1, tilePoint.y, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
          return false;
      }
      if (i == 3)
      {
        Vector3Int newPos = new Vector3Int(tilePoint.x + 1, tilePoint.y, 0);
        var tile = walls.GetTile(newPos);
        if (tile != null)
          return false;
      }
    }
    return true;
  }

  private Sprite SetDestuctionSprite(CornerType ct, int i)
  {
    switch (ct)
    {
      case CornerType.TopLeft:
        {
          if (i == 2)
            return DestructionParts[1];
          else if (i == 0)
            return DestructionParts[3];
          break;
        }
      case CornerType.TopRigth:
        {
          if (i == 3)
            return DestructionParts[1];
          else if (i == 0)
            return DestructionParts[2];
          break;
        }
      case CornerType.BottomLeft:
        {
          if (i == 1)
            return DestructionParts[3];
          else if (i == 2)
            return DestructionParts[0];
          break;
        }
      case CornerType.BottomRight:
        {
          if (i == 1)
            return DestructionParts[2];
          else if (i == 3)
            return DestructionParts[0];
          break;
        }
    }
    return DestructionParts[i];
  }
}
