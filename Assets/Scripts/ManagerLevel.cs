using System.Collections;
using System.Collections.Generic;
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
        PlayerPrefs.SetInt("LengthLevel", length);
    }

    public List<Dictionary<string,object>> GetListLevel()
    {
        List<Dictionary<string, object>> listData = new();
        for (int i = 0; i < this.length; i++)
        {
            if (PlayerPrefs.GetString("data_level_" + i, "") != "")
            {
                Dictionary<string, object> levelData = Json.Deserialize(PlayerPrefs.GetString("data_level_" + i, "")) as Dictionary<string, object>;
                listData.Add(levelData);
            }
        }
        return listData;
    }
}
