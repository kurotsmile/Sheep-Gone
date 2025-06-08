using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BtnSelLevel : MonoBehaviour
{
    public Image imgIcon;
    public Image imgBk;
    public Text txtName;
    private UnityAction onActClick;

    public void SetActClick(UnityAction act)
    {
        onActClick = act;
    }

    public void OnClick()
    {
        onActClick?.Invoke();
    }
}
