using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
  public GameDirector Director;

  public int PointsAmount;

  public LayerMask PlayerLayer;

  public GameObject AfterEffect;

  private AudioSource Sound;

  private bool destroy;

  private void Start()
  {
    Sound = GetComponent<AudioSource>();
  }

  private void Update()
  {
    if(destroy && !Sound.isPlaying)
      Destroy(gameObject);
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (PlayerLayer.value == (PlayerLayer | 1 << collision.gameObject.layer))
    {
      Director.AddBonus(PointsAmount);
      Instantiate(AfterEffect, transform.position, new Quaternion());
      Sound.Play();
      destroy = true;
      GetComponent<SpriteRenderer>().enabled = false;
    }
  }

}
