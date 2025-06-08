using System;
using UnityEngine;
using UnityEngine.UI;

public class GameHandle : MonoBehaviour
{
    [Header("Main Objects")]
    public Carrot.Carrot carrot;
    public Play play;
    public IronSourceAds ads;
    public Anim_Control anim;
    public GameObject ObjSheep;
    public GameObject ObjViewSelectSheep;
    public GameObject ObjLevel;

    [Header("UI")]
    public GameObject panel_home;
    public GameObject panel_selectLevel;
    public GameObject panel_selectSheep;
    public GameObject panel_play;
    public GameObject panel_win;

    [Header("Map Select Sheep")]
    public GameObject[] ListSheepObj;
    public String[] ListSheepName;
    public GameObject ObjSheepArrowSelect;
    private int currentSheepIndex = 0;
    public Text txtNameSheepSelect;

    [Header("UI Select Level")]
    public Transform allItemSelectLevel;
    public GameObject itemSelectLevelPrefab;

    [Header("Assets Icons")]
    public Sprite icon_level_play;
    public Sprite icon_level_lock;
    public CameraControl cameraControl;

    void Start()
    {
        this.carrot.Load_Carrot();
        this.panel_home.SetActive(true);
        this.panel_play.SetActive(false);
        this.panel_win.SetActive(false);
        this.panel_selectSheep.SetActive(false);
        this.panel_selectLevel.SetActive(false);
        this.ads.On_Load();
        this.ObjSheep.SetActive(true);
        this.ObjViewSelectSheep.SetActive(false);
        this.ObjLevel.SetActive(true);
        if (!PlayerPrefs.HasKey("currentLevel")) PlayerPrefs.SetInt("currentLevel", 0);
    }

    public void Btn_setting()
    {
        carrot.Create_Setting();
    }

    public void OnBtn_Play()
    {
        this.carrot.play_sound_click();
        this.panel_home.SetActive(false);
        this.panel_play.SetActive(false);
        this.panel_selectLevel.SetActive(true);
        this.ads.show_ads_Interstitial();
    }

    public void OnBtn_Home()
    {
        this.carrot.play_sound_click();
        this.panel_play.SetActive(false);
        this.panel_home.SetActive(true);
        this.panel_selectLevel.SetActive(false);
        this.panel_selectSheep.SetActive(false);
        this.panel_win.SetActive(false);
        this.ads.show_ads_Interstitial();
        this.anim.ReloadHome();
        this.ObjSheep.SetActive(true);
        this.ObjViewSelectSheep.SetActive(false);
        this.ObjLevel.SetActive(true);
    }

    public void OnBtn_Rate()
    {
        this.carrot.play_sound_click();
        this.carrot.show_rate();
    }

    public void OnBtn_Share()
    {
        this.carrot.play_sound_click();
        this.carrot.show_share();
    }

    public void OnOther_App()
    {
        this.carrot.play_sound_click();
        this.carrot.show_list_carrot_app();
    }

    public void OnBtn_ShowListLevel()
    {
        this.carrot.clear_contain(this.allItemSelectLevel);
        this.ads.show_ads_Interstitial();
        this.carrot.play_sound_click();
        this.panel_home.SetActive(false);
        this.panel_play.SetActive(false);
        this.panel_selectLevel.SetActive(true);
        for (int i = 0; i < 8; i++)
        {
            var index_item = i;
            GameObject item = Instantiate(this.itemSelectLevelPrefab, this.allItemSelectLevel);
            BtnSelLevel btnSelLevel = item.GetComponent<BtnSelLevel>();
            btnSelLevel.txtName.text = "Level " + (index_item + 1);
            if (i > PlayerPrefs.GetInt("currentLevel")) btnSelLevel.imgIcon.sprite = this.icon_level_lock;
            btnSelLevel.SetActClick(() =>
            {
                this.OnStartLevel(index_item);
            });
        }
    }

    public void OnStartLevel(int levelID)
    {
        this.cameraControl.editor = false;
        this.ads.show_ads_Interstitial();
        this.carrot.play_sound_click();
        this.panel_selectLevel.SetActive(false);
        this.panel_play.SetActive(true);
        this.play.OnStartGame(levelID);
        this.play.UnPause();
    }

    public void ShowWin()
    {
        this.panel_play.SetActive(false);
        this.panel_selectLevel.SetActive(false);
        this.panel_win.SetActive(true);
        this.play.levelWon = true;
    }

    public void OnBtn_ShowSelectSheep()
    {
        this.carrot.play_sound_click();
        this.panel_selectSheep.SetActive(true);
        this.panel_home.SetActive(false);
        this.ObjSheep.SetActive(false);
        this.ObjViewSelectSheep.SetActive(true);
        this.ObjLevel.SetActive(false);
        this.cameraControl.editor = true;
        this.cameraControl.SetPivotPoint(new Vector3(0, 0, 0));
        this.UpdateUISelectSheep();
    }

    public void OnBtn_next_sheep()
    {
        this.currentSheepIndex++;
        if (this.currentSheepIndex >= this.ListSheepObj.Length) this.currentSheepIndex = 0;
        this.carrot.play_sound_click();
        this.UpdateUISelectSheep();
    }

    public void OnBtn_prev_sheep()
    {
        this.currentSheepIndex--;
        if (this.currentSheepIndex < 0) this.currentSheepIndex = this.ListSheepObj.Length - 1;
        this.carrot.play_sound_click();
        this.UpdateUISelectSheep();
    }

    private void UpdateUISelectSheep()
    {
        Vector3 pos_vec = new Vector3(
            this.ListSheepObj[this.currentSheepIndex].transform.position.x,
            this.ListSheepObj[this.currentSheepIndex].transform.position.y + 3f,
            this.ListSheepObj[this.currentSheepIndex].transform.position.z
        );
        this.ObjSheepArrowSelect.transform.position = pos_vec;
        this.ObjSheepArrowSelect.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        this.txtNameSheepSelect.text = this.ListSheepName[this.currentSheepIndex];
    }

    public void OnBtn_DoneSelectSheep()
    {
        this.carrot.play_sound_click();
        this.ObjSheep.SetActive(true);
        this.ObjViewSelectSheep.SetActive(false);
        this.ObjLevel.SetActive(true);
        this.cameraControl.editor = false;
        this.panel_selectSheep.SetActive(false);
        this.panel_home.SetActive(true);
    }
}
