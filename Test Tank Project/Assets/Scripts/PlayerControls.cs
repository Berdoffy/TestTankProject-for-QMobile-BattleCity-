using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{


  private enum MovementDirection
  {
    None,
    Up,
    Right,
    Down,
    Left
  }

  [Min(0)]
  public float Speed = 1;

  public Transform TowerT;

  public LayerMask DamageLayer;


  public Grid grid;

  public GameObject TankExplosion;

  public GameObject PlayerSpawnEffect;

  Rigidbody2D rb2d;

  Vector2 currentDirection;

  bool IsMoving;

  TrackAnimation trackAnim;

  Node currentNode;
  Node nextNode;

  private GameDirector Director;

  [HideInInspector]
  public bool IsGameActive => Director?.CheckGameActive() ?? false;

  private MovementDirection CurrentMovementDirection;
  // Start is called before the first frame update
  void Start()
  {
    currentNode = grid.GetNodeFromGrid(transform.position);
    transform.position = currentNode.Position;
    currentDirection = Vector2.up; 
    RotateTank(currentDirection);
    rb2d = gameObject.GetComponent<Rigidbody2D>();
    trackAnim = gameObject.GetComponent<TrackAnimation>();
    Instantiate(PlayerSpawnEffect, transform.position, new Quaternion());
  }

  public void SetGameDirector(GameDirector director)
  {
    Director = director;
    TowerT.GetComponent<TowerControls>().SetDirector(director);
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  private void FixedUpdate()
  {
    /*if(IsMoving)
    {
      CheckDistance();
      if (nextNode == null)
        return;
      var direction = (nextNode.Position - currentNode.Position).normalized;
      rb2d.velocity = direction * Speed;
      if (direction != currentDirection && direction != Vector2.zero)
        currentDirection = direction;
      RotateTank(direction);
    }*/

    if (!IsGameActive)
    {
      rb2d.velocity = Vector2.zero;
      return;
    }
    
    var direction = SetDirection();
    if (direction == Vector2.zero && IsMoving)
    {
      IsMoving = false;
      trackAnim.StopAnim();
    }
    else if(direction != Vector2.zero && !IsMoving)
    {
      IsMoving = true;
      trackAnim.StartAnim();
    }
    rb2d.velocity = direction.normalized * Speed;
    if (direction != currentDirection && direction != Vector2.zero)
      currentDirection = direction;
    if (direction != Vector2.zero)
    {
      var towerR = TowerT.rotation;
      RotateTank(direction);
      TowerT.rotation = towerR;
    }
  }

  void CheckDistance()
  {
    if (nextNode == null)
      return;
    if ((nextNode.Position - (Vector2)transform.position).sqrMagnitude <= .1f)
    {
      currentNode = nextNode;
      nextNode = null;
      transform.position = currentNode.Position;
    }
  }



  private void RotateTank(Vector2 direction)
  {
    if (direction == Vector2.zero)
      return;
    if (Vector2.Dot(direction, transform.right) < -.99999f)
      return;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
  }

  private Vector2 SetDirection()
  {
    return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));    
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if(DamageLayer.value == (DamageLayer | 1 << collision.collider.gameObject.layer))
    {
      Director.OnPlayerDeath();
      Instantiate(TankExplosion, transform.position, new Quaternion());
      Destroy(gameObject);
    }
  }
}
