using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
  public float MoveSpeed = .2f;

  private MainMenuManager Manager;

  private bool IsOnPlace;

  private bool Ready => Manager != null;
  // Start is called before the first frame update
  void Start()
  {

  }

  public void SetManager(MainMenuManager manager)
  {
    Manager = manager;
  }

  // Update is called once per frame
  void Update()
  {
    if(Ready && !IsOnPlace)
    {
      transform.position = new Vector3(transform.position.x, transform.position.y + MoveSpeed * Time.deltaTime, transform.position.z);
      if(transform.position.y >= 0)
      {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        IsOnPlace = true;
        Manager.CamOnPlace();
      }
    }
  }
}
