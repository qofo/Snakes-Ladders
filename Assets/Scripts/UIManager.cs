using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // UI용 변수들
    private Text levelText;                     // 레벨 숫자를 표시할 텍스트 UI
    private GameObject levelImage;              // LevelImage UI의 레퍼런스
    private GameObject backgroundImage;           // 배경화면
    private GameObject boardImage;               // 바탕 보드판



    private GameObject restartButton;           // 시작 버튼 UI
    private Text restartText;                   // 시작 버튼에 들어가는 텍스트 UI
    private GameObject exitButton;              // 게임 종료 버튼 UI
    private GameObject gameoverImage;           // 게임종료시 이미지
    private GameObject pauseImage;              // 일시 정지 시 이미지
    private GameObject shopImage;
    
    public void InitMainUI(int level)
    {
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        restartButton = GameObject.Find("RestartButton");
        restartText = GameObject.Find("RestartText").GetComponent<Text>();
        exitButton = GameObject.Find("ExitButton");
        gameoverImage = GameObject.Find("GameoverImage");
        pauseImage = GameObject.Find("PauseImage");

        backgroundImage = GameObject.Find("BackgroundImage");
        shopImage = GameObject.Find("ShopImage");

        levelText.text = "Loading...";
        levelImage.SetActive(true);
        restartButton.SetActive(false);
        exitButton.SetActive(false);
        gameoverImage.SetActive(false);
        pauseImage.SetActive(false);
        shopImage.SetActive(false);

        //backgroundImage.SetActive(false);
        //boardImage.SetActive(false);
    }

    public void HideLevelImage()
    {
        levelImage.SetActive(false);
    }

    public void ShowGameOver(int level)
    {
        gameoverImage.SetActive(true);                                          // 게임 오버 이미지 보이기

        restartText.text = "RESTART";
        levelText.text = "After " + level + "floors, you failed.";              // 게임 오버 텍스트
        levelText.rectTransform.anchoredPosition = new Vector3(0f, -150f, 0f);   // 게임 오버 텍스트 위치 이동

        RectTransform rectTransform;
        rectTransform = restartButton.GetComponent<RectTransform>();            // 재시작 버튼 위치 이동
        rectTransform.anchoredPosition = new Vector3(0f, 60f, 0f);

        rectTransform = exitButton.GetComponent<RectTransform>();               // 종료 버튼 위치 이동
        rectTransform.anchoredPosition = Vector3.zero;

        levelImage.SetActive(true);
        restartButton.SetActive(true);
        exitButton.SetActive(true);
    }

    // 이미 Pause되어 있으면 true, 아니면 false를 반환하고 PauseImage를 끄고 킴
    public bool IsPaused()
    {
        return ToggleUI(pauseImage);
    }

    public bool ToggleUI(GameObject ui)
    {
        if (ui.activeSelf == false)
        {
            ui.SetActive(true);
            return false;
        }
        else
        {
            ui.SetActive(false);
            return true;
        }
    }

    public bool ToggleShop()
    {
        return ToggleUI(shopImage);
    }
}
