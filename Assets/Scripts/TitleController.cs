using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    public static TitleController instance = null;

    public GameObject scoreText;


    private GameObject levelImage;
    private GameObject scoreImage;
    private Text indexText;
    private GameObject score2MenuButton;

    void Awake()
    {
        // 두 개의 TitleController가 생기지 않게 하기
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        levelImage = GameObject.Find("LevelImage");
        scoreImage = GameObject.Find("ScoreImage");
        indexText = GameObject.Find("IndexText").GetComponent<Text>();
        score2MenuButton = GameObject.Find("Score2MenuButton");

        scoreImage.SetActive(false);
    }

    public void HideScoreboard()
    {
        levelImage.SetActive(true);
        scoreImage.SetActive(false);
    }

    public void ShowScoreboard()
    {
        levelImage.SetActive(false);
        scoreImage.SetActive(true);
        GameObject textObject;
        RectTransform rectTransform;
        Text text;
        //GameObject scoreText = GetCom
        for (int i = 0; i < ScoreManager.scores.Count; i++)
        {
            textObject = Instantiate(scoreText);
            textObject.transform.SetParent(scoreImage.transform);
            rectTransform = textObject.GetComponent<RectTransform>();
            text = textObject.GetComponent<Text>();

            // 위치 설정하기
            Vector2 pivot = new Vector2(0.5f, 0.8f - 0.1f * i);
            rectTransform.pivot = pivot;
            rectTransform.anchorMax = pivot;
            rectTransform.anchorMin = pivot;
            rectTransform.anchoredPosition = Vector2.zero;

            int index = ScoreManager.scores.Count - 1 - i;
            //text.text = "" + (i+1) + "\t\t" + ScoreManager.scores[index].level + "Floors\t\t" + ScoreManager.scores[index].name;
            text.text = $"{i+1, -8}{ScoreManager.scores[index].level + "Floors", -12}{ScoreManager.scores[index].name}";
        }
    }

}
