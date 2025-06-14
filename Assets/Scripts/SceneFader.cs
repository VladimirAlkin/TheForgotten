using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public float fadeTime = 1f; // Duration of the fade effect

    private Image fadeOutUIImage;

    public enum FadeDirection
    {
        In,
        Out
    }

    void Start()
    {
        fadeOutUIImage = GetComponent<Image>();
        if (fadeOutUIImage != null)
        {
            fadeOutUIImage.color = new Color(0, 0, 0, 1); // Ensure it's black and fully opaque
            StartCoroutine(Fade(FadeDirection.In));
        }
        else
        {
            Debug.LogError("Fade Image component is missing.");
        }
    }

    public IEnumerator Fade(FadeDirection fadeDirection)
    {
        float alpha = fadeDirection == FadeDirection.Out ? 1 : 0;
        float fadeEndValue = fadeDirection == FadeDirection.Out ? 0 : 1;
        Color startColor = fadeOutUIImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, fadeEndValue);

        while (Mathf.Abs(fadeOutUIImage.color.a - fadeEndValue) > 0.01f)
        {
            alpha = Mathf.Lerp(alpha, fadeEndValue, Time.deltaTime / fadeTime);
            fadeOutUIImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        fadeOutUIImage.color = endColor;

        if (fadeDirection == FadeDirection.Out && SceneManager.GetActiveScene().name != null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, string sceneToLoad)
    {
        fadeOutUIImage.enabled = true;
        yield return Fade(fadeDirection);
        SceneManager.LoadScene(sceneToLoad);
    }

    public IEnumerator FadeIn()
    {
        return Fade(FadeDirection.In);
    }

    public IEnumerator FadeOut(string sceneToLoad)
    {
        return Fade(FadeDirection.Out);
    }
}
