using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackAnimation : MonoBehaviour
{

  public List<Sprite> AnimationFrames;

  public float Speed = .4f;

  public List<SpriteRenderer> renderers;

  private int CurrentFrame;

  private bool IsOn;

  // Start is called before the first frame update
  void Start()
  {
    
    CurrentFrame = 0;
    //StartCoroutine("OnChangeFrame");
  }


  public void StartAnim()
  {
    OnChangeFrame();
    StartCoroutine("OnChangeFrame");
  }

  public void StopAnim()
  {
    StopCoroutine("OnChangeFrame");
  }

  public IEnumerator ChangeFrame()
  {
    while (true)
    {
      yield return new WaitForSeconds(Speed);
      OnChangeFrame();
    }
  }

  public void OnChangeFrame()
  {
    CurrentFrame++;
    if (CurrentFrame > AnimationFrames.Count - 1)
      CurrentFrame = 0;
    foreach(SpriteRenderer r in renderers)
      r.sprite = AnimationFrames[CurrentFrame];
  }
}
