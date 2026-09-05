using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Timer visual exibido acima de uma construção
/// durante construção ou upgrade.
///
/// Mostra:
/// - ação atual
/// - tempo restante
/// - barra de progresso
///
/// O timer acompanha a construção e fica sempre
/// voltado para a câmera.
/// </summary>
public class ConstructionTimer : MonoBehaviour
{
    // =========================================================
    // CONFIGURAÇÃO VISUAL
    // =========================================================

    [Header("Posição do Timer")]
    [SerializeField]
    private float heightAboveBuilding = 2.5f;


    [Header("Tamanho do Timer")]
    [SerializeField]
    private float canvasWidth = 3.8f;

    [SerializeField]
    private float canvasHeight = 1.15f;


    [Header("Escala")]
    [SerializeField]
    private float canvasScale = 0.02f;


    // =========================================================
    // ESTADO
    // =========================================================

    private float duration;

    private float progress;

    private string actionText;


    // =========================================================
    // UI
    // =========================================================

    private Canvas canvas;

    private TextMeshProUGUI actionLabel;

    private TextMeshProUGUI timerLabel;

    private Image progressBackground;

    private Image progressFill;


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    public void Initialize(
        float constructionDuration,
        string currentAction
    )
    {
        duration =
            Mathf.Max(
                0.1f,
                constructionDuration
            );

        actionText =
            string.IsNullOrEmpty(
                currentAction
            )
                ? "CONSTRUINDO"
                : currentAction;


        CreateWorldCanvas();

        CreateUI();

        SetProgress(0f);
    }


    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (canvas == null)
            return;


        Camera mainCamera =
            Camera.main;


        if (mainCamera == null)
            return;


        // =====================================================
        // SEMPRE OLHAR PARA A CÂMERA
        // =====================================================

        canvas.transform.rotation =
            mainCamera.transform.rotation;
    }


    // =========================================================
    // CRIAR CANVAS
    // =========================================================

    private void CreateWorldCanvas()
    {
        GameObject canvasObject =
            new GameObject(
                "TimerCanvas"
            );


        // =====================================================
        // PRENDER O TIMER À CONSTRUÇÃO
        // =====================================================

        canvasObject.transform.SetParent(
            transform,
            false
        );


        // =====================================================
        // POSIÇÃO ACIMA DA CONSTRUÇÃO
        // =====================================================

        canvasObject.transform.localPosition =
            new Vector3(
                0f,
                heightAboveBuilding,
                0f
            );


        canvasObject.transform.localRotation =
            Quaternion.identity;


        canvasObject.transform.localScale =
            Vector3.one *
            canvasScale;


        // =====================================================
        // CANVAS WORLD SPACE
        // =====================================================

        canvas =
            canvasObject.AddComponent<
                Canvas
            >();


        canvas.renderMode =
            RenderMode.WorldSpace;


        canvas.worldCamera =
            Camera.main;


        canvas.sortingOrder =
            500;


        // =====================================================
        // TAMANHO
        // =====================================================

        RectTransform rect =
            canvas.GetComponent<
                RectTransform
            >();


        rect.sizeDelta =
            new Vector2(
                canvasWidth * 100f,
                canvasHeight * 100f
            );
    }


    // =========================================================
    // CRIAR UI
    // =========================================================

    private void CreateUI()
    {
        RectTransform canvasRect =
            canvas.GetComponent<
                RectTransform
            >();


        // =====================================================
        // FUNDO
        // =====================================================

        GameObject backgroundObject =
            new GameObject(
                "Background"
            );


        backgroundObject.transform.SetParent(
            canvasRect,
            false
        );


        Image background =
            backgroundObject.AddComponent<
                Image
            >();


        background.color =
            new Color(
                0.05f,
                0.05f,
                0.05f,
                0.88f
            );


        RectTransform backgroundRect =
            backgroundObject.GetComponent<
                RectTransform
            >();


        backgroundRect.anchorMin =
            Vector2.zero;


        backgroundRect.anchorMax =
            Vector2.one;


        backgroundRect.offsetMin =
            Vector2.zero;


        backgroundRect.offsetMax =
            Vector2.zero;


        // =====================================================
        // TEXTO DA AÇÃO
        // =====================================================

        GameObject actionObject =
            new GameObject(
                "ActionText"
            );


        actionObject.transform.SetParent(
            canvasRect,
            false
        );


        actionLabel =
            actionObject.AddComponent<
                TextMeshProUGUI
            >();


        actionLabel.text =
            actionText;


        actionLabel.fontSize =
            26f;


        actionLabel.alignment =
            TextAlignmentOptions.Center;


        actionLabel.color =
            Color.white;


        actionLabel.fontStyle =
            FontStyles.Bold;


        actionLabel.enableWordWrapping =
            false;


        RectTransform actionRect =
            actionObject.GetComponent<
                RectTransform
            >();


        actionRect.anchorMin =
            new Vector2(
                0f,
                0.53f
            );


        actionRect.anchorMax =
            new Vector2(
                1f,
                0.98f
            );


        actionRect.offsetMin =
            new Vector2(
                5f,
                0f
            );


        actionRect.offsetMax =
            new Vector2(
                -5f,
                0f
            );


        // =====================================================
        // RELÓGIO
        // =====================================================

        GameObject timerObject =
            new GameObject(
                "TimerText"
            );


        timerObject.transform.SetParent(
            canvasRect,
            false
        );


        timerLabel =
            timerObject.AddComponent<
                TextMeshProUGUI
            >();


        timerLabel.text =
            FormatTime(
                duration
            );


        timerLabel.fontSize =
            30f;


        timerLabel.alignment =
            TextAlignmentOptions.Center;


        timerLabel.color =
            Color.white;


        timerLabel.fontStyle =
            FontStyles.Bold;


        timerLabel.enableWordWrapping =
            false;


        RectTransform timerRect =
            timerObject.GetComponent<
                RectTransform
            >();


        timerRect.anchorMin =
            new Vector2(
                0f,
                0.20f
            );


        timerRect.anchorMax =
            new Vector2(
                1f,
                0.55f
            );


        timerRect.offsetMin =
            new Vector2(
                5f,
                0f
            );


        timerRect.offsetMax =
            new Vector2(
                -5f,
                0f
            );


        // =====================================================
        // FUNDO DA BARRA
        // =====================================================

        GameObject progressBackgroundObject =
            new GameObject(
                "ProgressBackground"
            );


        progressBackgroundObject.transform.SetParent(
            canvasRect,
            false
        );


        progressBackground =
            progressBackgroundObject.AddComponent<
                Image
            >();


        progressBackground.color =
            new Color(
                0.20f,
                0.20f,
                0.20f,
                1f
            );


        RectTransform progressBackgroundRect =
            progressBackgroundObject.GetComponent<
                RectTransform
            >();


        progressBackgroundRect.anchorMin =
            new Vector2(
                0.08f,
                0.06f
            );


        progressBackgroundRect.anchorMax =
            new Vector2(
                0.92f,
                0.17f
            );


        progressBackgroundRect.offsetMin =
            Vector2.zero;


        progressBackgroundRect.offsetMax =
            Vector2.zero;


        // =====================================================
        // PREENCHIMENTO DA BARRA
        // =====================================================

        GameObject progressFillObject =
            new GameObject(
                "ProgressFill"
            );


        progressFillObject.transform.SetParent(
            progressBackgroundObject.transform,
            false
        );


        progressFill =
            progressFillObject.AddComponent<
                Image
            >();


        progressFill.color =
            new Color(
                1f,
                0.65f,
                0.1f,
                1f
            );


        progressFill.type =
            Image.Type.Filled;


        progressFill.fillMethod =
            Image.FillMethod.Horizontal;


        progressFill.fillOrigin =
            0;


        progressFill.fillAmount =
            0f;


        RectTransform progressFillRect =
            progressFillObject.GetComponent<
                RectTransform
            >();


        progressFillRect.anchorMin =
            Vector2.zero;


        progressFillRect.anchorMax =
            Vector2.one;


        progressFillRect.offsetMin =
            Vector2.zero;


        progressFillRect.offsetMax =
            Vector2.zero;
    }


    // =========================================================
    // ATUALIZAR PROGRESSO
    // =========================================================

    public void SetProgress(
        float newProgress
    )
    {
        progress =
            Mathf.Clamp01(
                newProgress
            );


        // =====================================================
        // BARRA
        // =====================================================

        if (progressFill != null)
        {
            progressFill.fillAmount =
                progress;
        }


        // =====================================================
        // TEMPO RESTANTE
        // =====================================================

        float remainingTime =
            duration *
            (1f - progress);


        if (timerLabel != null)
        {
            timerLabel.text =
                FormatTime(
                    remainingTime
                );
        }
    }


    // =========================================================
    // FORMATAR TEMPO
    // =========================================================

    private string FormatTime(
        float seconds
    )
    {
        int totalSeconds =
            Mathf.Max(
                0,
                Mathf.CeilToInt(
                    seconds
                )
            );


        int minutes =
            totalSeconds / 60;


        int remainingSeconds =
            totalSeconds % 60;


        if (minutes > 0)
        {
            return
                minutes.ToString("00") +
                ":" +
                remainingSeconds.ToString("00");
        }


        return
            remainingSeconds.ToString("00") +
            "s";
    }
}