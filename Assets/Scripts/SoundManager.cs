using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /* public 변수들 */

    public AudioSource efxSource;               // 효과음
    public AudioSource musicSource;             // 배경 음악
    public static SoundManager instance = null; // 싱글톤 패턴을 위한 변수
    public float lowPitchRange = 0.95f;         // 윈음의 피치에서 5% 낮음
    public float highPitchRange = 1.05f;        // 원음의 피치에서 5% 높음

    /* 유니티 API 함수들 */
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    /* public 함수들 */
    // 하나의 오디오 클립을 재생하는 함수
    public void PlaySingle(AudioClip clip)
    {
        efxSource.clip = clip;
        efxSource.Play();
    }

    // 여러 오디오 클립 중에서 랜덤으로 하나를 랜덤한 피치로 재생하기
    public void RandomizeSfx(params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);                // 입력받은 오디오 클립 중 하나 선택하기
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);// -5% ~ +5% 중에 랜덤으로 피치 고르기

        efxSource.pitch = randomPitch;                                  // 정해진 클립을 정해진 피치로 재생하기
        efxSource.clip = clips[randomIndex];
        efxSource.Play();
    }
}
