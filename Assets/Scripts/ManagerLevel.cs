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

    public List<Dictionary<string, object>> GetListLevel()
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
    
    public void DeleteItem(int index)
    {
        if (index < 0 || index >= length) return;
        
        PlayerPrefs.DeleteKey("data_level_" + index);
        for (int i = index + 1; i < length; i++)
        {
            string nextKey = "data_level_" + i;
            string newKey = "data_level_" + (i - 1);
            PlayerPrefs.SetString(newKey, PlayerPrefs.GetString(nextKey, ""));
            PlayerPrefs.DeleteKey(nextKey);
        }
        
        length--;
        PlayerPrefs.SetInt("LengthLevel", length);
    }
}
