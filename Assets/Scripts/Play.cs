using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MiniJSON;

public enum TYPE_PLAY{main,customer,test}
sealed public class Play : MonoBehaviour
{

    [Header("Objects Main")]
    public GameHandle g;
    public GameObject ObjSheep;
    static int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { -1, 0 } };

    public GameObject GameplayCanvas;
    public TYPE_PLAY typePlay = TYPE_PLAY.main;

    public bool levelWon;
    public Text timeText;
    public Text levelText;

    private Level currentLevel;
    public Entity player;
    private bool levelLoaded = false;
    private bool inputReady;
    private bool gamePaused;
    private bool moved;
    private float levelTimer;
    private float cameraZoom = 35f;
    private byte clockCycle;
    private float clockTimer;
    private int[] dir = new int[] { 0, 0 };

    void Start()
    {
        if (GameData.initialized == false) GameData.Initialize();
        Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;
    }

    public void CheckClearMap()
    {
        if (GameData.initialized == false) GameData.Initialize();
        if (levelLoaded)
        {
            Debug.LogWarning("Level already loaded, restarting level.");
            levelLoaded = false;
            inputReady = false;
            currentLevel.EndLevel();
        }
    }

    public void OnStartGame(int levelID)
    {
        this.CheckClearMap();
        this.typePlay = TYPE_PLAY.main;
        Debug.Log("Starting Game at Level: " + levelID);
        StartLevel(levelID);
    }

    public void OnStartGameCustomer(Dictionary<string, object> lData)
    {
        this.CheckClearMap();
        this.typePlay = TYPE_PLAY.customer;
        StartLevelTest(lData);
    }

    public void OnStartGameTest(Dictionary<string, object> lData)
    {
        this.CheckClearMap();
        this.typePlay = TYPE_PLAY.test;
        StartLevelTest(lData);
    }

    void Update()
    {
        if (!levelLoaded || gamePaused)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

        levelTimer += Time.deltaTime;
        timeText.text = Mathf.FloorToInt(levelTimer / 60F).ToString("00") + ":" + Mathf.FloorToInt(levelTimer % 60).ToString("00");

        if (levelWon)
        {
            levelLoaded = false;
            inputReady = false;
            currentLevel.EndLevel();

            if (currentLevel.LevelID == -1)
            {
                LevelEditor.loadingLevel = true;
                LevelEditor.loadingWonLevel = levelWon;
                StartCoroutine(StartLevelWait(-1));
            }
            if (currentLevel.LevelID == -2)
            {
                StartCoroutine(StartLevelWait(-2));
            }
            else
            {
                this.g.carrot.play_vibrate();
                if (currentLevel.LevelID == 8)
                {
                    this.g.ShowWin();
                }
                else
                {
                    StartCoroutine(StartLevelWait(currentLevel.LevelID + 1));
                }
                this.g.ads.show_ads_Interstitial();
            }
            levelWon = false;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            dir = GetMovementDirection(KeyCode.W);
            float cameraRotY = Camera.main.transform.rotation.eulerAngles.y;
            this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            dir = GetMovementDirection(KeyCode.S);
            float cameraRotY = Camera.main.transform.rotation.eulerAngles.y - 180f;
            this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dir = GetMovementDirection(KeyCode.A);
            float cameraRotY = Camera.main.transform.rotation.eulerAngles.y - 90f;
            this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dir = GetMovementDirection(KeyCode.D);
            float cameraRotY = Camera.main.transform.rotation.eulerAngles.y + 90f;
            this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
        }
        else if (moved)
        {
            dir = new int[] { 0, 0 };
            moved = false;
        }

        //if (Time.time - clockTimer >= 0.05f && !player.IsMoving && inputReady)
        if (!player.IsMoving && inputReady)
        {
            if (dir[0] != 0 || dir[1] != 0)
            {
                currentLevel.MoveEntity(player, dir, true, clockCycle);
                StartCoroutine(InputCooldown(0.3f));
                moved = true;
            }
        }

        if (Input.GetKey(KeyCode.Q))
            Camera.main.transform.RotateAround(player.transform.position, Vector3.up, 60f * Time.deltaTime);
        if (Input.GetKey(KeyCode.E))
            Camera.main.transform.RotateAround(player.transform.position, Vector3.up, -60f * Time.deltaTime);
        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
            cameraZoom = Mathf.Clamp(cameraZoom - (75 * Time.deltaTime), 15f, 35f);
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            cameraZoom = Mathf.Clamp(cameraZoom + (75 * Time.deltaTime), 15f, 35f);

        if (Input.GetKey(KeyCode.R))
            RestartLevel();
        if (Input.GetKey(KeyCode.P))
            Pause();

        Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;

        //Utilise a clock cycle based system to syncronise movement between objects
        if (Time.time - clockTimer > 0.05f)
        {
            if (clockCycle != 6)
                clockCycle++;
            else
                clockCycle = 0;

            clockTimer = Time.time;
        }
    }

    //Called when starting a level used for testing in the level editor
    private IEnumerator StartLevelWait()
    {
        yield return new WaitForSeconds(1.25f);
        StartLevel(LevelLoader.JSONToLoad);
    }

    private IEnumerator StartLevelWait(int ID)
    {
        yield return new WaitForSeconds(1.25f);

        if (ID == -1)
        {
            this.levelLoaded = false;
            inputReady = false;
            currentLevel.EndLevel();
            if (this.typePlay == TYPE_PLAY.test) this.g.OnBackEditorLevel();
            if (this.typePlay == TYPE_PLAY.customer) this.g.OnBtn_ShowListLevel(1);
        }
        else if (ID == -2)
            SceneManager.LoadScene(0);
        else
        {
            if (PlayerPrefs.GetInt("currentLevel") < ID) PlayerPrefs.SetInt("currentLevel", ID);
            StartLevel(ID);
        }
    }

    private void StartLevel(int ID)
    {
        currentLevel = LevelLoader.Load("level", ID);
        currentLevel.InputCooldown = Time.time;
        levelLoaded = true;
        StartCoroutine(InputCooldown(1f));
        levelTimer = 0;
        levelText.text = "Level " + (currentLevel.LevelID + 1).ToString();
    }

    private void StartLevelTest(Dictionary<string, object> lData)
    {
        currentLevel = LevelLoader.LoadFromJSON(lData, -1, false);
        currentLevel.InputCooldown = Time.time;
        levelLoaded = true;
        StartCoroutine(InputCooldown(1f));
        levelTimer = 0;
        levelText.text = lData["name"].ToString();
    }

    private void StartLevel(Dictionary<string, object> rawJSON)
    {
        try
        {
            currentLevel = LevelLoader.LoadFromJSON(rawJSON, LevelLoader.levelToLoad, false);
            levelLoaded = true;
            StartCoroutine(InputCooldown(1f));
            levelTimer = 0;
            levelText.text = "Level " + (currentLevel.LevelID + 1 < 0 ? "0" : ((currentLevel.LevelID + 1).ToString()));
        }
        catch (Exception ex)
        {
            if (ex is System.IO.FileNotFoundException)
                Debug.Log("Level File Missing");
            else
                Debug.Log(ex.ToString());
        }
    }

    public void RestartLevel()
    {
        if (!player.IsMoving && inputReady && levelLoaded)
        {
            levelLoaded = false;
            inputReady = false;
            currentLevel.EndLevel();
            if (currentLevel.LevelID == -1)
                StartCoroutine(StartLevelWait());
            else
                StartCoroutine(StartLevelWait(currentLevel.LevelID));

            this.g.carrot.play_vibrate();
        }
    }

    public void Pause()
    {
        if (!player.IsMoving && inputReady && levelLoaded)
        {
            gamePaused = true;
        }
    }

    public void ErrorButtonClicked()
    {
        SceneManager.LoadScene(0);
    }

    public void UnPause()
    {
        GameplayCanvas.GetComponent<Canvas>().enabled = true;
        gamePaused = false;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    IEnumerator InputCooldown(float time)
    {
        inputReady = false;
        yield return new WaitForSeconds(time);
        inputReady = true;
        currentLevel.InputCooldown = 0f;
    }

    //Get direction to move player based off the camera angle when a movement key was input
    int[] GetMovementDirection(KeyCode key)
    {
        int[] dir = directions[0];
        float cameraRot = Camera.main.transform.rotation.eulerAngles.y;
        int angleDirection = 0;

        if (cameraRot >= 315 || cameraRot < 45)
            angleDirection = 0;
        else if (cameraRot >= 45 && cameraRot < 135)
            angleDirection = 1;
        else if (cameraRot >= 135 && cameraRot < 225)
            angleDirection = 2;
        else if (cameraRot >= 225 && cameraRot < 315)
            angleDirection = 3;

        if (key == KeyCode.W)
            dir = directions[angleDirection];
        if (key == KeyCode.D)
            dir = directions[(angleDirection + 1) % 4];
        if (key == KeyCode.S)
            dir = directions[(angleDirection + 2) % 4];
        if (key == KeyCode.A)
            dir = directions[(angleDirection + 3) % 4];

        return dir;
    }

    public void OnGo_gamepad_left()
    {
        this.g.carrot.play_sound_click();
        this.dir = GetMovementDirection(KeyCode.A);
        float cameraRotY = Camera.main.transform.rotation.eulerAngles.y - 90f;
        this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
    }

    public void OnGo_gamepad_right()
    {
        this.g.carrot.play_sound_click();
        this.dir = GetMovementDirection(KeyCode.D);
        float cameraRotY = Camera.main.transform.rotation.eulerAngles.y + 90f;
        this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
    }

    public void OnGo_gamepad_up()
    {
        this.g.carrot.play_sound_click();
        this.dir = GetMovementDirection(KeyCode.W);
        float cameraRotY = Camera.main.transform.rotation.eulerAngles.y;
        this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
    }

    public void OnGo_gamepad_down()
    {
        this.g.carrot.play_sound_click();
        this.dir = GetMovementDirection(KeyCode.S);
        float cameraRotY = Camera.main.transform.rotation.eulerAngles.y - 180f;
        this.ObjSheep.transform.rotation = Quaternion.Euler(0, cameraRotY, 0);
    }

    public void OnCam_left()
    {
        this.g.carrot.play_sound_click();
        Camera.main.transform.RotateAround(player.transform.position, Vector3.up, +220f * Time.deltaTime);
    }

    public void OnCam_right()
    {
        this.g.carrot.play_sound_click();
        Camera.main.transform.RotateAround(player.transform.position, Vector3.up, 220f * Time.deltaTime);
    }

    public void OnCam_zoom_in()
    {
        this.g.carrot.play_sound_click();
        cameraZoom = Mathf.Clamp(cameraZoom - (75 * Time.deltaTime), 15f, 35f);
        Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;
    }

    public void OnCam_zoom_out()
    {
        this.g.carrot.play_sound_click();
        cameraZoom = Mathf.Clamp(cameraZoom + (75 * Time.deltaTime), 15f, 35f);
        Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;
    }

    public void OnCam_Reset()
    {
        this.g.carrot.play_sound_click();
        Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;
    }

    public void OnBtnBack()
    {
        if (this.typePlay == TYPE_PLAY.main) this.g.OnBtn_ShowListLevel();
        if (this.typePlay == TYPE_PLAY.customer) this.g.OnBtn_ShowListLevel(1);
        if (this.typePlay == TYPE_PLAY.test) this.g.OnBackEditorLevel();
    }
}