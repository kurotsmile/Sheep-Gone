using UnityEngine;

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
    }
    
}
