using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;                                           // DateTime Ŭ������ ����ϱ� ����
using System.IO;                                        // FileStream Ŭ������ ����ϱ� ����
using System.Runtime.Serialization.Formatters.Binary;   // BinaryFormatter Ŭ������ ����ϱ� ���� 
using UnityEngine.UI;

[System.Serializable]
public struct Score
{
    public int level;
    public string name;
    public Score(int level, string name)
    {
        this.level = level;
        this.name = name;
    }
}

public class ScoreManager : MonoBehaviour
{
    public static List<Score> scores = null;

    // ������ �����ϴ� �Լ�
    public void LoadScores()
    {
        scores = new List<Score>();
        for (int i = 0; i < 5; i++)
        {
            if (File.Exists(Application.streamingAssetsPath + "/Datas/score" + i + ".dat"))
            {
                FileStream file = new FileStream(Application.streamingAssetsPath + "/Datas/score" + i + ".dat", FileMode.Open);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                scores.Add((Score)binaryFormatter.Deserialize(file));
                file.Close();
            }
            else
                break;
        }
    }

    public void SaveScores()
    {
        for (int i = 0; i < scores.Count; i++)
        {
            FileStream file = new FileStream(Application.streamingAssetsPath + "/Datas/score" + i + ".dat", FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(file, scores[i]);
            file.Close();
        }
    }

    public void AddScore(int level)
    {
        Score new_score = new Score(level, DateTime.Now.ToString("yyyy.MM.dd HH:mm"));
        scores.Add(new_score);
        scores.Sort((Score a, Score b) =>
        {
            if (a.level > b.level)
                return 1;
            else if (a.level < b.level)
                return -1;
            else
                return 0;
        }
        );
        if (scores.Count > 5)
            scores.RemoveAt(0);

        SaveScores();
    }
}
