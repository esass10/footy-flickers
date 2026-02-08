using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ScoreTableController : MonoBehaviour
{
    public float gameDuration = 60f;
    public Coin2DController coin;

    TextMeshProUGUI scoreText, timerText, coinInventoryText, instructionsText, meterDisplayText, gameOverText;
    int currentScore = 0;
    float timeRemaining;
    bool isGameOver = false;
    Canvas canvas;

    public bool IsGameOver => isGameOver;
    public int CurrentScore => currentScore;

    void Start()
    {
        timeRemaining = gameDuration;
        if (coin == null) coin = FindFirstObjectByType<Coin2DController>();
        CreateUI();
    }

    // creates all UI elements at runtime so we don't need prefabs
    void CreateUI()
    {
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        scoreText = MakeText("ScoreText", new Vector2(20, -20), 32, TextAlignmentOptions.TopLeft);

        timerText = MakeText("TimerText", new Vector2(-20, -20), 32, TextAlignmentOptions.TopRight);
        timerText.rectTransform.anchorMin = timerText.rectTransform.anchorMax = timerText.rectTransform.pivot = new Vector2(1, 1);

        coinInventoryText = MakeText("CoinInventoryText", new Vector2(0, -20), 24, TextAlignmentOptions.Top, new Vector2(500, 100));
        coinInventoryText.rectTransform.anchorMin = coinInventoryText.rectTransform.anchorMax = coinInventoryText.rectTransform.pivot = new Vector2(0.5f, 1);

        instructionsText = MakeText("InstructionsText", new Vector2(20, 20), 24, TextAlignmentOptions.BottomLeft, new Vector2(450, 80));
        instructionsText.rectTransform.anchorMin = instructionsText.rectTransform.anchorMax = instructionsText.rectTransform.pivot = Vector2.zero;

        meterDisplayText = MakeText("MeterText", new Vector2(0, 100), 24, TextAlignmentOptions.Center, new Vector2(500, 80));
        meterDisplayText.rectTransform.anchorMin = meterDisplayText.rectTransform.anchorMax = meterDisplayText.rectTransform.pivot = new Vector2(0.5f, 0);

        gameOverText = MakeText("GameOverText", Vector2.zero, 32, TextAlignmentOptions.Center, new Vector2(500, 250));
        gameOverText.rectTransform.anchorMin = gameOverText.rectTransform.anchorMax = gameOverText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        gameOverText.gameObject.SetActive(false);
    }

    TextMeshProUGUI MakeText(string name, Vector2 pos, int size, TextAlignmentOptions align, Vector2? customSize = null)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(canvas.transform, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = customSize ?? new Vector2(250, 40);
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = align;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    void Update()
    {
        if (isGameOver) return;
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame("TIME'S UP!");
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = $"Score: {currentScore} cents";
        timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";

        if (coin != null)
        {
            string arrow = " <--";
            string n = coin.CurrentCoinType == Coin2DController.CoinType.Nickel ? arrow : "";
            string d = coin.CurrentCoinType == Coin2DController.CoinType.Dime ? arrow : "";
            string q = coin.CurrentCoinType == Coin2DController.CoinType.Quarter ? arrow : "";

            coinInventoryText.text = $"[1] Nickels (5c): {coin.NickelsRemaining}{n}\n[2] Dimes (10c): {coin.DimesRemaining}{d}\n[3] Quarters (25c): {coin.QuartersRemaining}{q}";
            if (coin.CoinIsSpinning)
                coinInventoryText.text += $"\n\nSpin Time: {coin.SpinTimeRemaining:F1}s";
            instructionsText.text = "[1/2/3] Select coin type\n[SPACE] Hold to aim, release to shoot!";

            if (coin.IsAiming)
            {
                // build a text-based meter like [-------|--O--------]
                int pos = Mathf.RoundToInt((coin.MeterValue + 1) * 10);
                string meter = "[";
                for (int i = 0; i < 21; i++)
                    meter += i == 10 ? "|" : i == pos ? "O" : "-";
                meterDisplayText.text = $"AIM: {meter}]\nRelease SPACE to shoot!";
            }
            else if (coin.IsShotInProgress)
                meterDisplayText.text = "SHOT IN PROGRESS...";
            else if (coin.NickelsRemaining <= 0 && coin.DimesRemaining <= 0 && coin.QuartersRemaining <= 0)
                meterDisplayText.text = "OUT OF COINS!";
            else
                meterDisplayText.text = "Press & hold SPACE to aim";
        }
    }

    public void AddScore(int points) => currentScore += points;
    public void OnMiss() { }
    public void OnBlocked() { }
    public void OnOutOfCoins() => EndGame("YOU'RE OUT OF COINS!");

    void EndGame(string reason)
    {
        isGameOver = true;
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = $"GAME OVER!\n{reason}\n\nFinal Score: {currentScore} cents\n\nPress R to restart";
        StartCoroutine(WaitForRestart());
    }

    System.Collections.IEnumerator WaitForRestart()
    {
        while (true)
        {
            if (Keyboard.current?.rKey.wasPressedThisFrame == true)
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            yield return null;
        }
    }
}
