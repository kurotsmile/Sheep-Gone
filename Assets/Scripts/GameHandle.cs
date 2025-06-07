using UnityEngine;

public class GameHandle : MonoBehaviour
{
    [Header("Main Objects")]
    public Carrot.Carrot carrot;
    public Play play;
    public IronSourceAds ads;

    [Header("UI")]
    public GameObject panel_home;
    public GameObject panel_selectLevel;
    public GameObject panel_play;

    [Header("UI Select Level")]
    public Transform allItemSelectLevel;
    public GameObject itemSelectLevelPrefab;
    

    void Start()
    {
        this.carrot.Load_Carrot();
        this.panel_home.SetActive(true);
        this.panel_play.SetActive(false);
        this.panel_selectLevel.SetActive(false);
        this.panel_selectLevel.SetActive(false);
        this.ads.On_Load();
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
        this.ads.show_ads_Interstitial();
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
        for(int i=0;i<8; i++)
        {
            var index_item = i;
            GameObject item = Instantiate(this.itemSelectLevelPrefab, this.allItemSelectLevel);
            BtnSelLevel btnSelLevel = item.GetComponent<BtnSelLevel>();
            btnSelLevel.txtName.text = "Level " + (index_item+ 1);
            btnSelLevel.SetActClick(() => {
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
}
