using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnManager : MonoBehaviour
{
    public GameObject informationPanel;

    public GameObject cloudBundle;

    public GameObject mainPanel;

    void Start()
    {
        DontDestroyOnLoad(cloudBundle);
        informationPanel.SetActive(false);
    }

    public void ContinueBtn()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void GoGameScene()
    {
        SceneManager.LoadScene("BotGame");
    }

    public void OnInformationPanel()
    {
        informationPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void OffInformationPanel()
    {
        informationPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
}
