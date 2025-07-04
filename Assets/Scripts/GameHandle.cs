using System.Collections.Generic;
using Carrot;
using UnityEngine;
using UnityEngine.UI;

public class GameHandle : MonoBehaviour
{
    [Header("Main Objects")]
    public Carrot.Carrot carrot;
    public Play play;

    public Carrot_File file;
    public IronSourceAds ads;
    public Anim_Control anim;
    public GameObject ObjSheep;
    public GameObject ObjViewPlayMain;
    public GameObject ObjViewSelectSheep;
    public GameObject ObjViewEditorLevel;
    public GameObject ObjLevel;
    public ManagerLevel mLevel;

    [Header("UI")]
    public GameObject panel_home;
    public GameObject panel_selectLevel;
    public GameObject panel_selectSheep;
    public GameObject panel_play;
    public GameObject panel_win;
    public GameObject panel_help;

    [Header("Map Select Sheep")]
    public GameObject[] ListSheepObj;
    public GameObject[] ListSheepPlayerObj;
    public string[] ListSheepName;
    public bool[] ListSheepIsBuy;
    public GameObject ObjSheepArrowSelect;
    private int currentSheepIndex = 0;
    public Text txtNameSheepSelect;
    public Image imgIconSheepSelectDone;

    [Header("UI Select Level")]
    public Transform allItemSelectLevel;
    public GameObject itemSelectLevelPrefab;
    public Color32 color_level_open;
    public Color32 color_level_lock;
    public Image[] imgBkBtnTypeMap;
    public Color32 ColorLevelNomalType;
    public Color32 ColorLevelSelectType;

    [Header("Assets Icons")]
    public Sprite icon_level_play;
    public Sprite icon_level_lock;
    public Sprite iconImportData;
    public Sprite iconExportData;
    public CameraControl cameraControl;
    public AudioSource audioBk;
    public int index_sheep_temp_buy = -1;

    void Start()
    {
        this.carrot.Load_Carrot(this.CheckExitApp);
        this.panel_home.SetActive(true);
        this.panel_play.SetActive(false);
        this.panel_win.SetActive(false);
        this.panel_selectSheep.SetActive(false);
        this.panel_selectLevel.SetActive(false);
        this.ads.On_Load();
        this.carrot.game.load_bk_music(this.audioBk);

        if(carrot.os_app==OS.Window)
            this.file.type = Carrot_File_Type.StandaloneFileBrowser;
        else
            this.file.type = Carrot_File_Type.SimpleFileBrowser;

        this.carrot.act_buy_ads_success = this.ads.RemoveAds;
        this.carrot.game.act_click_watch_ads_in_music_bk = this.ads.ShowRewardedVideo;
        this.ads.onRewardedSuccess = this.carrot.game.OnRewardedSuccess;
        this.carrot.shop.onCarrotPaySuccess += (string id_product) =>
        {
            if (id_product == this.carrot.shop.get_id_by_index(2))
            {
                this.ListSheepIsBuy[this.index_sheep_temp_buy] = true;
                this.ListSheepIsBuy[this.currentSheepIndex] = false;
                this.carrot.Show_msg("You have successfully purchased the sheep: " + this.ListSheepName[this.index_sheep_temp_buy]);
                this.UpdateUISelectSheep();
                this.UpdateSheepPlayer();
            }
        };

        this.mLevel.OnLoad();

        this.ObjSheep.SetActive(true);
        this.ObjViewPlayMain.SetActive(true);
        this.ObjViewSelectSheep.SetActive(false);
        this.ObjViewEditorLevel.SetActive(false);
        this.ObjLevel.SetActive(true);
        if (!PlayerPrefs.HasKey("currentLevel")) PlayerPrefs.SetInt("currentLevel", 0);
        this.currentSheepIndex = PlayerPrefs.GetInt("currentSheep", 0);
        this.UpdateSheepPlayer();

        Carrot_Gamepad gamepad1 = this.carrot.game.create_gamepad("Player_Sheep");
        gamepad1.set_gamepad_keydown_left(() => this.play.OnGo_gamepad_left());
        gamepad1.set_gamepad_keydown_right(() => this.play.OnGo_gamepad_right());
        gamepad1.set_gamepad_keydown_up(() => this.play.OnGo_gamepad_up());
        gamepad1.set_gamepad_keydown_down(() => this.play.OnGo_gamepad_down());
        gamepad1.set_gamepad_Joystick_left(() => this.play.OnCam_left());
        gamepad1.set_gamepad_Joystick_right(() => this.play.OnCam_right());
        gamepad1.set_gamepad_Joystick_up(() => this.play.OnCam_zoom_in());
        gamepad1.set_gamepad_Joystick_down(() => this.play.OnCam_zoom_out());
    }

    private void CheckExitApp()
    {
        if (this.panel_play.activeInHierarchy)
        {
            this.play.OnBtnBack();
            this.carrot.set_no_check_exit_app();
        }
        else if (this.panel_selectLevel.activeInHierarchy)
        {
            this.OnBtn_Home();
            this.carrot.set_no_check_exit_app();
        }
        else if (this.panel_selectSheep.activeInHierarchy)
        {
            this.OnBtn_Home();
            this.carrot.set_no_check_exit_app();
        }
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
        this.carrot.SetNameCanvasMain("Canvas");
        this.carrot.play_sound_click();
        this.panel_play.SetActive(false);
        this.panel_home.SetActive(true);
        this.panel_selectLevel.SetActive(false);
        this.panel_selectSheep.SetActive(false);
        this.panel_win.SetActive(false);
        this.panel_help.SetActive(false);
        this.ads.show_ads_Interstitial();
        this.anim.ReloadHome();
        this.ObjSheep.SetActive(true);
        this.ObjViewPlayMain.SetActive(true);
        this.ObjViewSelectSheep.SetActive(false);
        this.ObjViewEditorLevel.SetActive(false);
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
        this.OnBtn_ShowListLevel(0);
        this.carrot.play_sound_click();
    }

    public void OnBtn_ShowListLevel(int type)
    {
        this.carrot.clear_contain(this.allItemSelectLevel);
        this.ads.show_ads_Interstitial();
        this.panel_home.SetActive(false);
        this.panel_play.SetActive(false);
        this.panel_selectLevel.SetActive(true);

        this.imgBkBtnTypeMap[0].color = this.ColorLevelNomalType;
        this.imgBkBtnTypeMap[1].color = this.ColorLevelNomalType;

        if (type == 0)
        {
            this.imgBkBtnTypeMap[0].color = this.ColorLevelSelectType;
            for (int i = 0; i < 8; i++)
            {
                var index_item = i;
                GameObject item = Instantiate(this.itemSelectLevelPrefab, this.allItemSelectLevel);
                BtnSelLevel btnSelLevel = item.GetComponent<BtnSelLevel>();
                btnSelLevel.txtName.text = "Level " + (index_item + 1);
                if (i > PlayerPrefs.GetInt("currentLevel"))
                {
                    btnSelLevel.imgIcon.sprite = this.icon_level_lock;
                    btnSelLevel.imgBk.color = this.color_level_lock;
                    btnSelLevel.GetComponent<Button>().interactable = false;
                }
                else
                {
                    btnSelLevel.imgIcon.sprite = this.icon_level_play;
                    btnSelLevel.imgBk.color = this.color_level_open;
                    btnSelLevel.GetComponent<Button>().interactable = true;
                }
                btnSelLevel.SetActClick(() =>
                {
                    this.OnStartLevel(index_item);
                });
            }
        }
        else
        {
            this.imgBkBtnTypeMap[1].color = this.ColorLevelSelectType;
            List<Dictionary<string, object>> listLevel = this.mLevel.GetListLevel();
            for (int i = 0; i < listLevel.Count; i++)
            {
                var index_item = i;
                Dictionary<string, object> dataL = listLevel[i];
                GameObject item = Instantiate(this.itemSelectLevelPrefab, this.allItemSelectLevel);
                BtnSelLevel btnSelLevel = item.GetComponent<BtnSelLevel>();
                btnSelLevel.txtName.text = dataL["name"].ToString();
                btnSelLevel.imgIcon.sprite = this.icon_level_play;
                btnSelLevel.imgBk.color = this.color_level_open;
                btnSelLevel.GetComponent<Button>().interactable = true;
                btnSelLevel.SetActClick(() =>
                {
                    this.OnShowPlayCustomer(dataL);
                });
            }
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

    [ContextMenu("Show Win Panel")]
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
        Vector3 pos_vec = new(
            this.ListSheepObj[this.currentSheepIndex].transform.position.x,
            this.ListSheepObj[this.currentSheepIndex].transform.position.y + 3f,
            this.ListSheepObj[this.currentSheepIndex].transform.position.z
        );
        this.ObjSheepArrowSelect.transform.position = pos_vec;
        this.ObjSheepArrowSelect.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        this.txtNameSheepSelect.text = this.ListSheepName[this.currentSheepIndex];

        for (int i = 0; i < this.ListSheepObj.Length; i++)
        {
            if (i != this.currentSheepIndex)
            {
                Animator animNomal=this.ListSheepObj[i].GetComponent<Animator>();
                animNomal.Play("Idle");
            }
        }

        Animator anim=this.ListSheepObj[this.currentSheepIndex].GetComponent<Animator>();
        anim.Play("Atk");
        if (this.ListSheepIsBuy[this.currentSheepIndex])
        {
            this.imgIconSheepSelectDone.sprite = this.carrot.icon_carrot_buy;
        }
        else
        {
            this.imgIconSheepSelectDone.sprite = this.carrot.icon_carrot_done;
        }
    }

    public void OnBtn_DoneSelectSheep()
    {
        this.carrot.play_sound_click();
        if (this.ListSheepIsBuy[this.currentSheepIndex])
        {
            this.index_sheep_temp_buy = this.currentSheepIndex;
            this.carrot.buy_product(2);
        }
        else
        {
            this.ObjSheep.SetActive(true);
            this.ObjViewSelectSheep.SetActive(false);
            this.ObjLevel.SetActive(true);
            this.cameraControl.editor = false;
            this.panel_selectSheep.SetActive(false);
            this.panel_home.SetActive(true);
            PlayerPrefs.SetInt("currentSheep", this.currentSheepIndex);
            this.UpdateSheepPlayer();
        }
    }

    private void UpdateSheepPlayer()
    {
        for (int i = 0; i < this.ListSheepPlayerObj.Length; i++)
        {
            if (i == this.currentSheepIndex)
                this.ListSheepPlayerObj[i].SetActive(true);
            else
                this.ListSheepPlayerObj[i].SetActive(false);
        }
    }

    public void OnBtn_Help()
    {
        this.carrot.play_sound_click();
        this.panel_help.SetActive(true);
        this.panel_home.SetActive(false);
    }

    public void OnOpenEditorLevel()
    {
        this.carrot.SetNameCanvasMain("Canvas_edit_level");
        this.ObjViewPlayMain.SetActive(false);
        this.ObjViewEditorLevel.SetActive(true);
    }

    public void OnBackEditorLevel()
    {
        this.carrot.SetNameCanvasMain("Canvas_edit_level");
        this.play.CheckClearMap();
        this.cameraControl.editor = true;
        this.ObjViewPlayMain.SetActive(false);
        this.ObjViewEditorLevel.SetActive(true);
        this.panel_home.SetActive(true);
        this.panel_play.SetActive(false);
    }

    public void OnShowPlayTestLevelEditor(Dictionary<string, object> lData)
    {
        this.carrot.SetNameCanvasMain("Canvas");
        this.ObjViewPlayMain.SetActive(true);
        this.ObjViewEditorLevel.SetActive(false);
        this.panel_home.SetActive(false);
        this.panel_play.SetActive(true);
        this.panel_selectLevel.SetActive(false);
        this.cameraControl.editor = false;
        this.play.OnStartGameTest(lData);
    }

    public void OnShowPlayCustomer(Dictionary<string, object> lData)
    {
        this.carrot.SetNameCanvasMain("Canvas");
        this.ObjViewPlayMain.SetActive(true);
        this.ObjViewEditorLevel.SetActive(false);
        this.panel_home.SetActive(false);
        this.panel_play.SetActive(true);
        this.panel_selectLevel.SetActive(false);
        this.cameraControl.editor = false;
        this.play.OnStartGameCustomer(lData);
    }
}
