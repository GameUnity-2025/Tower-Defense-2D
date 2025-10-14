using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MennuController : MonoBehaviour
{
    public void OnStartButton()
    {
        int lastLevel = PlayerPrefs.GetInt("LastLevel", 1); 
        Debug.Log("Loading Level: Level" + lastLevel); 
        SceneManager.LoadScene("Level" + lastLevel);
    }
}
