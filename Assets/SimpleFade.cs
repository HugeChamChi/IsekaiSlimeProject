using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleFade : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    public void FadeAndLoadScene(string sceneName, float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutAndLoad(sceneName, fadeDuration));
    }

    IEnumerator FadeOutAndLoad(string sceneName, float fadeDuration)
    {
        float t = 0f;
        Color color = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t/fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
