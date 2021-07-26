using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{

  public Camera cam;

  public GameObject PlayerTank;

  public GameObject EnemyTank;

  public Core Core;

  [Min(1)]
  public int MaxPlayersLives = 3;

  [Min(0)]
  public int TotalEnemyCount = 12;

  [Min(0)]
  public int MaxEnemiesPerTime = 5;

  public GameObject EnemySpawnPoints;

  public Transform PlayerStartSpawnPoint;

  public GameObject PathFindingObjects;

  public float EnemySpawnDelay = 1.5f;

  public float PlayerSpawnDelay = 2f;

  public float EnemySpawnTankCheckRaduis = 10; 

  public LayerMask TankLayers;

  public TMPro.TextMeshProUGUI Lives;

  public TMPro.TextMeshProUGUI Points;

  public TMPro.TextMeshProUGUI Enemies;

  public TMPro.TextMeshProUGUI RespawnTxt;

  public Image FadeLeft;

  public Image FadeRight;

  public float FadeSpeed;

  public Image GameOver;

  public Image Win;

  public Image Menu;

  public TMPro.TextMeshProUGUI TotalScore;

  public TMPro.TextMeshProUGUI ScoreOnLose;

  public GameObject ShellCollapse;

  public Button MenuBtn;

  

  private List<Transform> EnemySpawnPointsList = new List<Transform>();

  private Grid grid;

  private PathFinding PF;

  #region EnemySpawn

  private bool IsEnemySpwanDelay;

  private float EnemySpawnTimer;

  private int CurrentEnemyCount;

  private int CurrentEnemyMaxCount;

  private int EnemyDeathLine;

  private int TotalEnemiesSpawned;

  private int EnemiesDied;

  #endregion

  private int CurrentPlayersLives;

  private bool SpawnPlayer;

  private bool IsSpawnPlayerDelay;

  private float SpawnPlayerDelayTimer;

  private int PointsValue;

  private float FadeThrashold;


  private bool FadeOut;

  private bool IsGameActive;

  private bool GameIsOver;

  private List<Guid> CollapsedShells = new List<Guid>();

  private bool ClearShells;

  private float ClearShellTimer;

  private AudioSource Sound;

  private bool StopMusic;

  // Start is called before the first frame update
  void Start()
  {
    grid = PathFindingObjects.GetComponent<Grid>();
    PF = PathFindingObjects.GetComponent<PathFinding>();
    foreach (Transform trans in EnemySpawnPoints.transform)
      EnemySpawnPointsList.Add(trans);

    //AI Tank init
    var eTank = EnemyTank.GetComponent<EnemyTankAI>();
    eTank.MainTarget = Core.gameObject;
    eTank.grid = grid;
    eTank.PF = PF;
    //eTank.SetGameDirector(this);

    Core.SetDirector(this);

    //Player tank init
    var pTank = PlayerTank.GetComponent<PlayerControls>();
    pTank.grid = grid;
    var player = Instantiate(PlayerTank, PlayerStartSpawnPoint.position, new Quaternion());
    player.GetComponent<PlayerControls>().SetGameDirector(this);
    CurrentPlayersLives = MaxPlayersLives;

    Sound = GetComponent<AudioSource>();

    CurrentEnemyMaxCount = Mathf.RoundToInt(MaxEnemiesPerTime * .3f);
    IsEnemySpwanDelay = true;

    Lives.text = CurrentPlayersLives.ToString();
    Points.text = PointsValue.ToString();
    Enemies.text = (TotalEnemyCount - EnemiesDied).ToString();

    FadeOut = true;
    FadeThrashold = FadeLeft.rectTransform.position.x * (-1);

    Sound.Play();
  }

  // Update is called once per frame
  void Update()
  {
    if (IsGameActive)
    {
      UpdateEnemySpawnDelay();
      UpdateSpawnEnemy();
      UpdateSpawnPlayerDelay();
      UpdateSpawnPlayer();
      UpdateClearShell();
      if(Input.GetKeyDown(KeyCode.Escape))
      {
        if (Menu.gameObject.activeSelf)
          ReturnToGame();
        else
          ShowMenu();
      }
    }
    if(FadeOut)
    {
      FadeLeft.rectTransform.position = new Vector3(FadeLeft.rectTransform.position.x - FadeSpeed * Time.deltaTime, 
        FadeLeft.rectTransform.position.y, FadeLeft.rectTransform.position.z);
      FadeRight.rectTransform.position = new Vector3(FadeRight.rectTransform.position.x + FadeSpeed * Time.deltaTime,
        FadeRight.rectTransform.position.y, FadeRight.rectTransform.position.z);
      if (FadeLeft.rectTransform.position.x <= FadeThrashold)
      {
        FadeOut = false;
        IsGameActive = true;
        MenuBtn.interactable = true;
      }
    }
    /*if (GameIsOver && Input.anyKeyDown)
      SceneManager.LoadScene("MainMenu");*/
    if (StopMusic)
      Sound.volume -= 0.5f * Time.deltaTime;
  }
  
  private void UpdateEnemySpawnDelay()
  {
    if (!IsEnemySpwanDelay)
      return;
    EnemySpawnTimer += Time.deltaTime;
    if(EnemySpawnTimer >= EnemySpawnDelay)
    {
      IsEnemySpwanDelay = false;
      EnemySpawnTimer = 0;
    }
  }

  public void ExitMainMenu()
  {
    Time.timeScale = 1;
    SceneManager.LoadScene("MainMenu");
  }

  public void ShowMenu()
  {
    IsGameActive = false;
    Menu.gameObject.SetActive(true);
    Sound.Pause();
    Time.timeScale = 0;
  }

  public void ReturnToGame()
  {
    Menu.gameObject.SetActive(false);
    Sound.Play();
    Time.timeScale = 1;
    IsGameActive = true;
  }

  private void UpdateSpawnEnemy()
  {
    if (IsEnemySpwanDelay)
      return;
    if (CurrentEnemyCount >= CurrentEnemyMaxCount || TotalEnemiesSpawned >= TotalEnemyCount)
      return;

    Transform point = FindBestSpawnPoing();    
    var tank = Instantiate(EnemyTank, point.position, new Quaternion());
    tank.GetComponent<EnemyTankAI>().SetGameDirector(this);
    IsEnemySpwanDelay = true;
    CurrentEnemyCount++;
    TotalEnemiesSpawned++;
  }

  private void UpdateSpawnPlayerDelay()
  {
    if (!IsSpawnPlayerDelay)
      return;
    SpawnPlayerDelayTimer += Time.deltaTime;
    if(SpawnPlayerDelayTimer >= PlayerSpawnDelay)
    {
      SpawnPlayerDelayTimer = 0;
      IsSpawnPlayerDelay = false;
    }
  }

  private void UpdateSpawnPlayer()
  {
    if (!SpawnPlayer || IsSpawnPlayerDelay)
      return;
    if(!IsSpawnPlayerDelay)
      RespawnTxt.gameObject.SetActive(true);
    if (Input.GetKeyDown(KeyCode.Mouse0))
    {
      Vector3 pos = Input.mousePosition;
      Vector3 realPos = cam.ScreenToWorldPoint(pos);
      Node n = grid.GetNodeFromGrid(realPos);
      if(!n.Walkable)
      {
        n = grid.GetNeighbours(n).FirstOrDefault(x => x.Walkable);
        if(!n.Walkable)
        {
          foreach(Node neib in grid.GetNeighbours(grid.GetNodeFromGrid(realPos)))
          {
            n = grid.GetNeighbours(neib).FirstOrDefault(x => x.Walkable);
            if (n != null)
              return;
          }
        }
      }
      if (!n.Walkable)
        return;
      var player = Instantiate(PlayerTank, n.Position, new Quaternion());
      player.GetComponent<PlayerControls>().SetGameDirector(this);
      SpawnPlayer = false;
      RespawnTxt.gameObject.SetActive(false);
    }
  }

  private void UpdateClearShell()
  {
    if (!ClearShells)
      return;
    ClearShellTimer += Time.deltaTime;
    if(ClearShellTimer >= 0.5f)
    {
      lock (CollapsedShells)
        CollapsedShells.Clear();
      ClearShellTimer = 0;
      ClearShells = false;
    }
  }

  private Transform FindBestSpawnPoing()
  {
    Transform result = null;
    int count = int.MaxValue;
    foreach(Transform t in EnemySpawnPointsList)
    {
      Collider2D[] tanks = Physics2D.OverlapCircleAll(t.position, EnemySpawnTankCheckRaduis, TankLayers);
      if (result == null || tanks.Length <= count)
      {
        result = t;
        count = tanks.Length;
      }
    }
    return result;
  }

  public void OnEnemyTankDeath(int points)
  {
    CurrentEnemyCount--;
    EnemyDeathLine++;
    EnemiesDied++;
    PointsValue += points;
    Enemies.text = (TotalEnemyCount - EnemiesDied).ToString();
    Points.text = PointsValue.ToString();
    if (TotalEnemyCount - EnemiesDied <= 0)
    {
      StopMusic = true;
      IsGameActive = false;
      Win.gameObject.SetActive(true);
      TotalScore.text = Points.text;
      MenuBtn.interactable = false;
      GameIsOver = true;
      return;
    }
    IsEnemySpwanDelay = true;        
    if (EnemyDeathLine > 3)
    {
      CurrentEnemyMaxCount = Mathf.Min(CurrentEnemyMaxCount + 1, MaxEnemiesPerTime);
      EnemyDeathLine = 0;
    }
    
  }

  public void OnPlayerDeath()
  {    
    if (CurrentPlayersLives > 0)
    {
      CurrentPlayersLives--;
      IsSpawnPlayerDelay = true;      
      SpawnPlayer = true;
      Lives.text = CurrentPlayersLives.ToString();
    }
    else
    {
      StopMusic = true;
      IsGameActive = false;
      ScoreOnLose.text = Points.text;
      GameOver.gameObject.SetActive(true);
      MenuBtn.interactable = false;
      GameIsOver = true;
    }
  }

  public void OnCoreDestoryed()
  {
    StopMusic = true;
    IsGameActive = false;
    GameOver.gameObject.SetActive(true);
    ScoreOnLose.text = Points.text;
    MenuBtn.interactable = false;
    GameIsOver = true;
  }

  public bool IsInGrid(Vector2 position)
  {
    Node n = grid.grid[grid.GridSizeX - 1, 0];
    //var node = grid.GetNodeFromGrid(position);
    if (n.Position.x + grid.NodeRadius < position.x)
      return false;
    else
      return true;
  }  

  public void SetShellCollapse(Guid id1, Guid id2, Vector2 point)
  {
    bool collapse = false;
    lock(CollapsedShells)
    {
      if (!CollapsedShells.Contains(id1))
      {
        CollapsedShells.Add(id1);
        collapse = true;
      }
      if (!CollapsedShells.Contains(id2))
      {
        CollapsedShells.Add(id2);
        collapse = true;
      }
    }
    if(collapse)
    {
      Instantiate(ShellCollapse, point, new Quaternion());
      ClearShells = true;
      ClearShellTimer = 0;
    }
  }

  public void AddBonus(int amount)
  {
    PointsValue += amount;
    Points.text = PointsValue.ToString();
  }

  public bool CheckGameActive()
  {
    return IsGameActive;
  }  

  private void OnDrawGizmos()
  {
    if(EnemySpawnPoints != null)
    {
      foreach (Transform trans in EnemySpawnPoints.transform)
        Gizmos.DrawWireSphere(trans.position, EnemySpawnTankCheckRaduis);
    }
    /*Gizmos.color = Color.white;
    Gizmos.DrawWireSphere(transform.position, EnemySpawnTankCheckRaduis);*/
  }
}
