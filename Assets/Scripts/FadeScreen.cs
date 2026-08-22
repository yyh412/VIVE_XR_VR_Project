using System.Collections;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public bool fadeOnStart = true;
    public float fadeDuration = 2f;
    public Color fadeColor = Color.black;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (fadeOnStart)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        Fade(1f, 0f);
    }

    public void FadeOut()
    {
        Fade(0f, 1f);
    }

    public void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        float timer = 0f;

        while (timer <= fadeDuration)
        {
            Color newColor = fadeColor;

            newColor.a = Mathf.Lerp(
                alphaIn,
                alphaOut,
                timer / fadeDuration
            );

            rend.material.SetColor("_Color", newColor);

            timer += Time.deltaTime;

            yield return null;
        }

        Color finalColor = fadeColor;
        finalColor.a = alphaOut;
        rend.material.SetColor("_Color", finalColor);
    }
}