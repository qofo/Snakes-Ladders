using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // UI�� ������
    private Text levelText;                     // ���� ���ڸ� ǥ���� �ؽ�Ʈ UI
    private GameObject levelImage;              // LevelImage UI�� ���۷���
    private GameObject backgroundImage;           // ���ȭ��
    private GameObject boardImage;               // ���� ������



    private GameObject restartButton;           // ���� ��ư UI
    private Text restartText;                   // ���� ��ư�� ���� �ؽ�Ʈ UI
    private GameObject exitButton;              // ���� ���� ��ư UI
    private GameObject gameoverImage;           // ��������� �̹���
    private GameObject pauseImage;              // �Ͻ� ���� �� �̹���
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
        gameoverImage.SetActive(true);                                          // ���� ���� �̹��� ���̱�

        restartText.text = "RESTART";
        levelText.text = "After " + level + "floors, you failed.";              // ���� ���� �ؽ�Ʈ
        levelText.rectTransform.anchoredPosition = new Vector3(0f, -150f, 0f);   // ���� ���� �ؽ�Ʈ ��ġ �̵�

        RectTransform rectTransform;
        rectTransform = restartButton.GetComponent<RectTransform>();            // ����� ��ư ��ġ �̵�
        rectTransform.anchoredPosition = new Vector3(0f, 60f, 0f);

        rectTransform = exitButton.GetComponent<RectTransform>();               // ���� ��ư ��ġ �̵�
        rectTransform.anchoredPosition = Vector3.zero;

        levelImage.SetActive(true);
        restartButton.SetActive(true);
        exitButton.SetActive(true);
    }

    // �̹� Pause�Ǿ� ������ true, �ƴϸ� false�� ��ȯ�ϰ� PauseImage�� ���� Ŵ
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
