using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
  public Camera cam;

  public List<Button> Buttons;

  public Image FadeUp;
  public Image FadeDown;
  [Min(0)]
  public float FadeSpeed = .05f;

  public string GameScene;

  private bool IsCamOnPlace;

  private bool FadeOn;

  private bool Transition;

  private AudioSource Sound;

  // Start is called before the first frame update
  void Start()
  {
    cam.GetComponent<MainMenuCamera>().SetManager(this);
    Sound = GetComponent<AudioSource>();
    foreach (Button b in Buttons)
      b.interactable = false;
  }

  // Update is called once per frame
  void Update()
  {
    if(FadeOn)
    {
      FadeUp.rectTransform.position = new Vector3(FadeUp.rectTransform.position.x, FadeUp.rectTransform.position.y - FadeSpeed 
        * Time.deltaTime, FadeUp.rectTransform.position.z);
      FadeDown.rectTransform.position = new Vector3(FadeDown.rectTransform.position.x, 
        FadeDown.rectTransform.position.y + FadeSpeed * Time.deltaTime, FadeDown.rectTransform.position.z);
      if(Mathf.Abs(FadeUp.rectTransform.position.y) + Mathf.Abs(FadeDown.rectTransform.position.y) <=
        50)
      {
        FadeOn = false;
        Transition = true;
      }
      Sound.volume -= .5f * Time.deltaTime;
    }
    if (Transition)
      SceneManager.LoadScene(GameScene);
  }

  public void CamOnPlace()
  {
    IsCamOnPlace = true;
    foreach (Button b in Buttons)
      b.interactable = true;
    Sound.Play();
  }

  public void StartGame()
  {
    foreach (Button b in Buttons)
      b.interactable = false;
    FadeOn = true;
  }
  public void Exit()
  {
    Application.Quit();
  }
}
