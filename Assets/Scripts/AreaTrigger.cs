using System.Collections;
using UnityEngine;
using TMPro;

public class AreaTriggerFade : MonoBehaviour
{
    public GameObject uiPanel;
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private TMP_Text[] tmpTexts;
    private Color[] originalTextColors;

    private void Start()
    {
        if (uiPanel != null)
        {
            canvasGroup = uiPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("UI Panel must have a CanvasGroup component attached.");
                return;
            }

            canvasGroup.alpha = 0f;
            uiPanel.SetActive(true);


            tmpTexts = uiPanel.GetComponentsInChildren<TMP_Text>();
            originalTextColors = new Color[tmpTexts.Length];
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                originalTextColors[i] = tmpTexts[i].color;
            }
        }
        StartCoroutine(FadeCanvasGroupAndText(1f, 0f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroupAndText(0f, 1f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroupAndText(1f, 0f));
        }
    }

    private IEnumerator FadeCanvasGroupAndText(float fromAlpha, float toAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float newAlpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / fadeDuration);
            canvasGroup.alpha = newAlpha;
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                Color newColor = originalTextColors[i];
                newColor.a = newAlpha;
                tmpTexts[i].color = newColor;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = toAlpha;
        for (int i = 0; i < tmpTexts.Length; i++)
        {
            Color newColor = originalTextColors[i];
            newColor.a = toAlpha;
            tmpTexts[i].color = newColor;
        }
    }
}
