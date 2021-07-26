using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerControls : MonoBehaviour
{

  public Transform FireSpot;

  public GameObject projectile;

  [Min(0)]
  public float FireDelay = .5f;

  public bool IsPlayer;

  public float rotationSpeed = .5f;

  public GameObject MuzzleFlash;

  /*public float FireBackForceTime = .5f;

  public float FireBackForcePower = .5f;*/

  Camera cam;

  float FireDelayTimer = 0;

  Quaternion desiredRotaion;

  bool IsRotation;

  bool IsFireDelay;

  private float FireBackForceTimer;

  private bool FireBackForce;

  private Vector3 previousPosition;

  private GameDirector Director;

  private AudioSource Sound;

  // Start is called before the first frame update
  void Start()
  {
    cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    FireDelayTimer = 0;
    IsFireDelay = false;
    Sound = gameObject.GetComponent<AudioSource>();
  }

  public void SetDirector(GameDirector director)
  {
    Director = director;
  }

  // Update is called once per frame
  void Update()
  {
    if(IsPlayer && Director.CheckGameActive())
    {
      var direction = (cam.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
      LookAt(direction);
      

      if (!IsFireDelay && Input.GetKeyDown(KeyCode.Mouse0))
      {
        var position = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Director.IsInGrid(position))
          Fire();
        else
          Debug.Log(position);
      }
    }      
    
    CheckFireDelay();

    //CheckPowerBackForce();

    if(IsRotation)
    {
      if (desiredRotaion != transform.rotation)
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotaion, rotationSpeed * Time.deltaTime);
      else
        IsRotation = false;
    }
  }

  public void LookAt(Vector2 direction)
  {   
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    desiredRotaion = Quaternion.AngleAxis(angle, Vector3.forward);
    IsRotation = true;
  }

  public void SetRotation(Quaternion rotation)
  {
    desiredRotaion = rotation;
    IsRotation = true;
  }

  private void CheckFireDelay()
  {
    if (!IsFireDelay)
      return;
    FireDelayTimer += Time.deltaTime;
    if (FireDelayTimer >= FireDelay)
    {
      IsFireDelay = false;
      FireDelayTimer = 0;
    }
  }

  /*public void CheckPowerBackForce()
  {
    if (!FireBackForce)
      return;
    FireBackForceTimer += Time.deltaTime;
    if(FireBackForceTimer >= FireBackForceTime)
    {
      FireBackForceTimer = 0;
      FireBackForce = false;
      transform.localPosition = previousPosition;
    }
  }*/

  public void SetFireDelay()
  {
    IsFireDelay = true;
    FireDelayTimer = 0;
  }

  /*private void SetBackForce()
  {
    FireBackForce = true;
    FireBackForceTimer = 0;
    previousPosition = transform.localPosition;
    var transformP = transform.parent.parent;
    float angle = Vector2.Angle(transform.right, transformP.right);
    transform.localPosition = new Vector3(Mathf.Cos(angle) * Mathf.Sign(transformP.right.x), 
      Mathf.Sin(angle) * Mathf.Sign(transformP.right.y), 0).normalized * -FireBackForcePower;
  }*/


  public bool IsFireReady()
  {
    return !IsFireDelay;
  }

  public void Fire()
  {
    var flash = Instantiate(MuzzleFlash);
    flash.transform.position = FireSpot.position;
    flash.transform.rotation = transform.rotation;
    flash.transform.parent = transform;
    var proj = Instantiate(projectile);
    proj.transform.position = FireSpot.position;
    proj.GetComponent<Projectile>().SetDirector(Director);
    proj.SetActive(true);
    var script = proj.GetComponent<Projectile>();
    script.Fire(transform.right);
    SetFireDelay();
    Sound.Play();
    //SetBackForce();
  }
}
