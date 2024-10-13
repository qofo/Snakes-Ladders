using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /* public ������ */

    public AudioSource efxSource;               // ȿ����
    public AudioSource musicSource;             // ��� ����
    public static SoundManager instance = null; // �̱��� ������ ���� ����
    public float lowPitchRange = 0.95f;         // ������ ��ġ���� 5% ����
    public float highPitchRange = 1.05f;        // ������ ��ġ���� 5% ����

    /* ����Ƽ API �Լ��� */
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    /* public �Լ��� */
    // �ϳ��� ����� Ŭ���� ����ϴ� �Լ�
    public void PlaySingle(AudioClip clip)
    {
        efxSource.clip = clip;
        efxSource.Play();
    }

    // ���� ����� Ŭ�� �߿��� �������� �ϳ��� ������ ��ġ�� ����ϱ�
    public void RandomizeSfx(params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);                // �Է¹��� ����� Ŭ�� �� �ϳ� �����ϱ�
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);// -5% ~ +5% �߿� �������� ��ġ ����

        efxSource.pitch = randomPitch;                                  // ������ Ŭ���� ������ ��ġ�� ����ϱ�
        efxSource.clip = clips[randomIndex];
        efxSource.Play();
    }
}
