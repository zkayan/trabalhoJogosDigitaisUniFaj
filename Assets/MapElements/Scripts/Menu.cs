using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void closeGame()
    {
        Application.Quit();
    }

    public void play()
    {
        SceneManager.LoadScene("Scene1");
    }
}
