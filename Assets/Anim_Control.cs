using UnityEngine;

public class Anim_Control : MonoBehaviour
{
    public Animator ani;

    void Start()
    {
        this.ani.Play("UI_home_load");
    }
    public void OnStopAnim()
    {
        this.ani.enabled = false;
    }

    public void OnMainHome()
    {
        this.ani.enabled = true;
        this.ani.Play("Ui_main");
    }
    
    public void ReloadHome(){
        this.ani.enabled = true;
        this.ani.Play("UI_home_load");
    }
}
