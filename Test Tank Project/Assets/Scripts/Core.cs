using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour
{
  public LayerMask DamageLayers;

  private GameDirector Director;

  private AudioSource Sound;

  // Start is called before the first frame update
  void Start()
  {
    Sound = GetComponent<AudioSource>();
  }

  public void SetDirector(GameDirector director)
  {
    Director = director;
  }

  // Update is called once per frame
  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (DamageLayers.value == (DamageLayers | 1 << collision.collider.gameObject.layer))
    {
      Director.OnPlayerDeath();
      Sound.Play();
      gameObject.SetActive(false);
      Director.OnCoreDestoryed();
      
      //Instantiate(TankExplosion, transform.position, new Quaternion());
      //Destroy(gameObject);
    }
  }
}
