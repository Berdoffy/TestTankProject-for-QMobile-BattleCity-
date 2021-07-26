using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EffectAfterDestroyer : MonoBehaviour
{
  private AudioSource Sound;

  private bool DestroyReady;
  private void Start()
  {
    Sound = GetComponent<AudioSource>();
    if (Sound != null)
      Sound.Play();
  }

  private void Update()
  {
    if (!DestroyReady || (Sound != null && Sound.isPlaying))
      return;
    else
      Destroy(gameObject);
  }

  public void DestroyMe()
  {
    DestroyReady = true;
    GetComponent<SpriteRenderer>().enabled = false;
    //
  }
}
