using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTankAI : MonoBehaviour
{
  [Min(0)]
  public float Speed;

  [Min(0)]
  public float DetectionRadius = 6;

  [Min(0)]
  public float DetectionUpdateSpeed = .3f;

  [Min(0)]
  public float AttackRadius;

  [Min(0)]
  public float AttackDelay = .5f;

  public TowerControls Tower;

  //public Transform FireSpot;

  public GameObject MainTarget;

  public GameObject Progectile;

  public LayerMask PlayerLayer;

  public LayerMask ObstacleMask;

  public LayerMask DamageLayerMask;

  public Grid grid;

  public PathFinding PF;

  public float EnemyRadius;

  public Vector2 RandomShotDelay;

  public float ActionDelay;

  public float ChaseTime;

  public GameObject DeathEffect;

  public float FriendDetectionDelay = 0.2f;

  public float FriendDetectionRadius = 5;

  public int MyPoints = 20;

  public float SpawnDelay = 1;

  public GameObject EnemySpawnEffect;

  private Transform currentTarget;

  private List<Node> Path;

  private Node currentDestination;

  private Rigidbody2D rb2d;

  bool Attack;

  bool IsDead;
  
  bool IsMoving;

  bool IsWaiting;

  private float RandomShotCurrentDelay;

  private float RandomShotTimer;

  private bool RandomShot;

  private float ActionDelayTimer;

  private bool IsActionDelay;

  private bool IsChasing;

  private float ChaseTimer;
  private Guid Id;

  private float AfterSpawnDelayTimer;

  private GameDirector Director;

  private bool IsGameActive => Director.CheckGameActive();

  bool IsPlayerDetected => currentTarget != null;

  bool IsSpawnDelay => AfterSpawnDelayTimer > 0;

  public Guid GetId()
  {
    return Id;
  }
  public bool IsBuisy()
  {
    return !IsMoving;
  }

  void Start()
  {
    AfterSpawnDelayTimer = SpawnDelay;
    Id = Guid.NewGuid();
    transform.position = GetMyNode().Position;
    rb2d = gameObject.GetComponent<Rigidbody2D>();
    UpdateTarget(MainTarget.transform);
    grid.OnGridUpdate += OnGridUpdate;
    SetRandomShotDelay();
    RandomShotTimer = 0;
    StartCoroutine("FindPlayerWithDelay", DetectionUpdateSpeed);
    StartCoroutine("FindFriensWithDelay", FriendDetectionDelay);
    Instantiate(EnemySpawnEffect, GetMyNode().Position, new Quaternion());
  }

  public void SetGameDirector(GameDirector director)
  {
    Director = director;
    Tower.SetDirector(director);
  }

  private float SetRandomShotDelay()
  {
    return RandomShotCurrentDelay = UnityEngine.Random.Range(RandomShotDelay.x, RandomShotDelay.y);
  }

  private void OnDisable()
  {
    grid.OnGridUpdate -= OnGridUpdate;
  }

  private void OnDestroy()
  {
    grid.OnGridUpdate -= OnGridUpdate;
  }

  #region Updates

  void Update()
  {
    if(IsSpawnDelay)
    {
      AfterSpawnDelayTimer -= Time.deltaTime;
      return;
    }
    if(!IsGameActive)
    {
      StopMoving();
      return;
    }
    UpdateActionDelay();
    UpdateRandomShot();
    if (IsPlayerDetected && !RandomShot)
    {
      var direction = Utility.GetDirection(currentTarget.transform.position, transform.position);
      Tower.LookAt(direction);
      if (IsInAttackRadius(currentTarget.position) && !Attack)
      {
        IsActionDelay = true;
        Attack = true;
        StopMoving();
      }
    }
    else if (IsChasing)
    {
      ChaseTimer += Time.deltaTime;
      if (ChaseTimer >= ChaseTime)
      {
        ChaseTimer = 0;
        IsChasing = false;
        UpdateTarget(MainTarget.transform);
      }
    }
    else if (!IsPlayerDetected && (Path.Count == 0 || IsWaiting) && IsInAttackRadius(MainTarget.transform.position))
    {
      var direction = Utility.GetDirection(MainTarget.transform.position, transform.position);
      
      if (!Attack)
      {
        Attack = true;
        IsActionDelay = true;
        StopMoving();
      }
      Tower.LookAt(direction);
    }
    else if(!IsPlayerDetected && rb2d.velocity != Vector2.zero && !RandomShot)
    {
      Tower.LookAt(rb2d.velocity.normalized);
      
    }
    else if(RandomShot && !Tower.IsFireReady())
    {
      RandomShot = false;
      Tower.Fire();
    }
    if (Attack && !IsActionDelay && Tower.IsFireReady())
    {
      Tower.Fire();
    }
    if (Attack && !IsPlayerDetected && Path.Count > 0 && ! IsWaiting)
    {
      Attack = false;
    }
  }

  private void UpdateActionDelay()
  {
    if (!IsActionDelay)
      return;
    ActionDelayTimer += Time.deltaTime;
    if(ActionDelayTimer >= ActionDelay)
    {
      IsActionDelay = false;
      ActionDelayTimer = 0;
    }
  }
  private void UpdateRandomShot()
  {
    if (!RandomShot)
    {
      if (IsPlayerDetected)
      {
        RandomShotTimer = 0;
        return;
      }
      RandomShotTimer += Time.deltaTime;
      if (RandomShotTimer >= RandomShotCurrentDelay)
      {
        RandomShotTimer = 0;
        RandomShot = true;
        StopMoving();
        IsActionDelay = true;
        var newRotation = UnityEngine.Random.rotation;
        newRotation.x = 0;
        newRotation.y = 0;
        Tower.SetRotation(newRotation);
      }
    }
    if (RandomShot)
    {
      if (IsPlayerDetected || IsChasing)
      {
        RandomShot = false;
        SetRandomShotDelay();
        DropActionDelay();
        return;
      }
      if (Tower.IsFireReady() && !IsActionDelay)
      {
        Tower.Fire();
        SetRandomShotDelay();
        RandomShot = false;
      }
    }
  }

  public void OnGridUpdate()
  {
    if (Path != null)
    {
      if (IsPlayerDetected)
        UpdateTarget(currentTarget);
      else
        UpdateTarget(MainTarget.transform);
    }
  }

  private void FixedUpdate()
  {
    if (!IsGameActive || IsSpawnDelay)
      return;
    if (!Attack && !RandomShot && !IsWaiting)
    {
      CheckPath();
      if (currentDestination != null)
      {
        IsMoving = true;
        Vector2 direction = (currentDestination.Position - (Vector2)transform.position).normalized;
        RotateTank(direction);
        rb2d.velocity = direction * Speed;
      }
    }
  }

  void CheckPath()
  {
    if (currentDestination != null)
    {
      if ((currentDestination.Position - (Vector2)transform.position).sqrMagnitude <= 0.01f)
        currentDestination = null;
      else
        return;
    }
    if (Path != null && Path.Count > 0)
    {
      currentDestination = Path[0];
      Path.RemoveAt(0);
    }
    else if (IsMoving)
    {
      StopMoving();
    }

  }

  IEnumerator FindPlayerWithDelay(float delay)
  {
    while (true)
    {
      yield return new WaitForSeconds(delay);
      FindPlayer();
    }
  }

  void FindPlayer()
  {
    if (!IsGameActive || IsDead || IsSpawnDelay)
    {
      return;
    }

    Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, DetectionRadius, PlayerLayer);
    if (players.Length == 0)
    {
      if(Path == null || Path.Count == 0)
        UpdateTarget(MainTarget.transform);

      IsChasing = false;
      ChaseTimer = 0;

      currentTarget = null;

      if (Attack)
        Attack = false;
    }
    else
    {
      var direction = (players[0].transform.position - transform.position).normalized;
      RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, DetectionRadius, ObstacleMask);
      if (hit.transform != null && hit.transform == players[0].transform)
      {
        if (currentTarget == null)
        {
          IsChasing = false;
          ChaseTimer = 0;
          currentTarget = players[0].transform;
          UpdateTarget(players[0].transform);
        }
      }
      else
      {
        if (currentTarget)
        {
          IsChasing = true;
          UpdateTarget(currentTarget);
          IsMoving = true;
        }

        currentTarget = null;

        if (Attack)
          Attack = false;
      }
      
    }         
  }

  IEnumerator FindFriensWithDelay(float delay)
  {
    while (true)
    {
      yield return new WaitForSeconds(delay);
      FindFriends();
    }
  }

  void FindFriends()
  {
    if (!IsGameActive || IsDead || Attack || RandomShot || IsSpawnDelay)
      return;
    
    RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.right * 1.5f, transform.right, 
      FriendDetectionRadius/2);
    if (hit)
    {
      var tank = hit.collider.GetComponent<EnemyTankAI>();
      if (tank == null)
        return;
      IsWaiting = tank.IsBuisy();
      if (IsWaiting)
        StopMoving();
    }
    else
      IsWaiting = false;
  }

  void UpdateTarget(Transform target)
  {
    Path = PF.CreatePath(transform.position, target.position, EnemyRadius);
  }

  #endregion

  private void DropActionDelay()
  {
    IsActionDelay = false;
    ActionDelayTimer = 0;
  }

  private void StopMoving()
  {
    rb2d.velocity = Vector2.zero;
    IsMoving = false;
  }

  

  private bool IsInAttackRadius(Vector2 position)
  {
    return Utility.GetSqrMagintude(position, transform.position) <= AttackRadius * AttackRadius;
  }

  Node GetMyNode()
  {
    Node n = grid.GetNodeFromGrid(transform.position);
    if (!n.Walkable)
    {
      n = grid.GetNeighbours(n).FirstOrDefault(x => x.Walkable);
    }
    return n;
  }


  private void RotateTank(Vector2 direction)
  {
    if (direction == Vector2.zero)
      return;
    if (Vector2.Dot(direction, transform.right) < -.99999f)
      return;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    //currentDirection = direction;
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if(DamageLayerMask.value == (DamageLayerMask | 1 << collision.collider.gameObject.layer))
    {
      IsDead = true;
      var death = Instantiate(DeathEffect);
      death.transform.position = transform.position;
      Director.OnEnemyTankDeath(MyPoints);
      Destroy(gameObject);
    }
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, AttackRadius);
    Gizmos.color = Color.magenta;
    Gizmos.DrawWireSphere(transform.position, FriendDetectionRadius);
    Gizmos.color = Color.black;
    Gizmos.DrawLine(transform.position, transform.position + transform.right * 1.5f);
    if (Path != null)
    {
      foreach (Node n in Path)
      {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(n.Position, Vector2.one * grid.NodeRadius * 2);
      }
    }
  }

}
