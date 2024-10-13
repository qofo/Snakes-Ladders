using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    /* MainScene 버튼용 함수들 */
    public void OnRestartButtonClick()
    {
        GameManager.instance.RestartGame();
    }
    public void OnExitButtonClick()
    {
        Application.Quit();
    }

    public void OnResumeButtonClick()
    {
        GameManager.instance.Pause();
    }

    /* TitleScene 버튼용 함수들 */
    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void OnScoreButtonClick()
    {
        TitleController.instance.ShowScoreboard();
    }

    public void OnScore2MenuButtonClick()
    {
        TitleController.instance.HideScoreboard();
    }
}
