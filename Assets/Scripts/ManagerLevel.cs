using System.Collections;
using Carrot;
using UnityEngine;

public class ManagerLevel : MonoBehaviour
{
    int length = 0;

    public void OnLoad()
    {
        length = PlayerPrefs.GetInt("LengthLevel", 0);
    }

    public void Add(string s_data)
    {
        PlayerPrefs.SetString("data_level_" + this.length, s_data);
        length++;
    }

    public IList getListNameLevel()
    {
        IList listNames = Json.Deserialize("[]") as IList;
        for (int i = 0; i < this.length; i++)
        {
            if (PlayerPrefs.GetString("")!="") listNames.Add("s");
        }
        return listNames;
    }
}
