using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

  public float Speed = 15;

  public float LifeTime = 5;

  public bool CanRicochet;

  public LayerMask RicochetLayers;

  public LayerMask WallLayers;

  public LayerMask ProgectilesLayers;

  public LayerMask TargetLayers;

  public GameObject ReflectEffect;

  public GameObject WallExpFront;

  public GameObject WallExpBack;

  private Guid Id;

  private Rigidbody2D rb2d;

  private GameDirector Director;


  float lifeTimer;

  private Vector2 currentDirection;

  // Start is called before the first frame update
  void Start()
  {
    Id = Guid.NewGuid();
  }

  public void SetDirector(GameDirector director)
  {
    Director = director;
  }

  public Guid GetId()
  {
    return Id;
  }

  // Update is called once per frame
  void Update()
  {
    lifeTimer += Time.deltaTime;
    if (lifeTimer >= LifeTime)
      Destroy(gameObject);
  }

  public void Fire(Vector2 direction)
  {
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    rb2d = gameObject.GetComponent<Rigidbody2D>();
    gameObject.SetActive(true);
    rb2d.velocity = direction * Speed;
    currentDirection = direction;
  }

  void Rotate(Vector2 direction)
  {
    if (direction == Vector2.zero)
      return;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (TargetLayers.value == (TargetLayers | 1 << collision.collider.gameObject.layer))
    {
      Destroy(gameObject);
    }
    else if(CanRicochet && RicochetLayers.value == (RicochetLayers | 1 << collision.collider.gameObject.layer))
    {
      var hit = Physics2D.Raycast(transform.position, currentDirection.normalized, float.MaxValue, RicochetLayers);
      if (hit)
      {
        var effect = Instantiate(ReflectEffect);
        effect.transform.position = hit.point + hit.normal * 1.05f;
        effect.transform.rotation = GetRotation(hit.normal);
        var direction = Vector2.Reflect(currentDirection.normalized, hit.normal).normalized;
        rb2d.velocity = direction * Speed;
        Rotate(direction);
        currentDirection = direction;
      }
    }
    else if(WallLayers.value == (WallLayers | 1 << collision.collider.gameObject.layer))
    {
      SetExplosion(collision.contacts[0].point);
      Destroy(gameObject);
    }
    else if(ProgectilesLayers.value == (ProgectilesLayers | 1 << collision.collider.gameObject.layer))
    {
      Director.SetShellCollapse(Id, collision.gameObject.GetComponent<Projectile>().GetId(), collision.contacts[0].point);
      Destroy(gameObject);
    }
    else
    {
      var hit = Physics2D.Raycast(transform.position, currentDirection.normalized, float.MaxValue, RicochetLayers);
      var effect = Instantiate(ReflectEffect);
      effect.transform.position = hit.point + hit.normal * 1.05f;
      effect.transform.rotation = GetRotation(hit.normal);
      Destroy(gameObject);
    }     
  }

  public void SetExplosion(Vector2 point)
  {
    /*var hit = Physics2D.Raycast(transform.position, currentDirection.normalized, float.MaxValue, WallLayers);
    if (!hit)
      return;*/
    var front = Instantiate(WallExpFront);
    front.transform.position = point + (Vector2)transform.right * -1;
    front.transform.rotation = GetRotation(transform.right * -1);
    var back = Instantiate(WallExpBack);
    back.transform.position = point + (Vector2)transform.right * 2f;
    back.transform.rotation = GetRotation(transform.right);
  }  

  private Quaternion GetRotation(Vector2 direction)
  {
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }  
}
