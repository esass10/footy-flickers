using UnityEngine;
using UnityEngine.InputSystem;

public class Coin2DController : MonoBehaviour
{
    public float shotPower = 15f;
    public float meterSpeed = 3f;

    // higher value coins get less spin time (harder to score)
    public float quarterSpinTime = 2f;
    public float dimeSpinTime = 3.5f;
    public float nickelSpinTime = 5f;
    public int quarterValue = 25;
    public int dimeValue = 10;
    public int nickelValue = 5;
    public int startingNickels = 5;
    public int startingDimes = 3;
    public int startingQuarters = 2;

    public ScoreTableController scoreTable;

    Rigidbody2D rb;
    Vector3 startPosition;
    float meterValue, spinTimeRemaining, maxSpinTime;
    bool isAiming, isShotInProgress, coinIsSpinning;
    CoinType currentCoinType = CoinType.Nickel;
    int nickelsRemaining, dimesRemaining, quartersRemaining;

    public enum CoinType { Nickel, Dime, Quarter }

    public float MeterValue => meterValue;
    public float SpinTimeRemaining => spinTimeRemaining;
    public bool IsAiming => isAiming;
    public bool IsShotInProgress => isShotInProgress;
    public CoinType CurrentCoinType => currentCoinType;
    public bool CoinIsSpinning => coinIsSpinning;
    public int NickelsRemaining => nickelsRemaining;
    public int DimesRemaining => dimesRemaining;
    public int QuartersRemaining => quartersRemaining;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
        nickelsRemaining = startingNickels;
        dimesRemaining = startingDimes;
        quartersRemaining = startingQuarters;

        if (scoreTable == null)
        {
            scoreTable = FindFirstObjectByType<ScoreTableController>();
            if (scoreTable == null)
            {
                var obj = new GameObject("ScoreTableController");
                scoreTable = obj.AddComponent<ScoreTableController>();
                scoreTable.coin = this;
            }
        }
        SelectFirstAvailableCoin();
    }

    void Update()
    {
        if (scoreTable != null && scoreTable.IsGameOver) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!isShotInProgress)
        {
            if (kb.digit1Key.wasPressedThisFrame && nickelsRemaining > 0) SetCoinType(CoinType.Nickel);
            else if (kb.digit2Key.wasPressedThisFrame && dimesRemaining > 0) SetCoinType(CoinType.Dime);
            else if (kb.digit3Key.wasPressedThisFrame && quartersRemaining > 0) SetCoinType(CoinType.Quarter);
            if (HasCoinsRemaining() && GetCurrentCoinCount() > 0)
            {
                if (kb.spaceKey.wasPressedThisFrame) StartAiming();
                if (isAiming)
                {
                    // oscillates between -1 and 1 for the aim meter
                    meterValue = Mathf.Sin(Time.time * meterSpeed);
                    if (kb.spaceKey.wasReleasedThisFrame) Shoot();
                }
            }
        }

        // coin spins until time runs out
        if (coinIsSpinning)
        {
            spinTimeRemaining -= Time.deltaTime;
            transform.Rotate(0, 0, 360 * Time.deltaTime);
            if (spinTimeRemaining <= 0) StopCoin();
        }
    }

    bool HasCoinsRemaining() => nickelsRemaining > 0 || dimesRemaining > 0 || quartersRemaining > 0;

    int GetCurrentCoinCount()
    {
        return currentCoinType switch
        {
            CoinType.Nickel => nickelsRemaining,
            CoinType.Dime => dimesRemaining,
            CoinType.Quarter => quartersRemaining,
            _ => 0
        };
    }

    void UseCoin()
    {
        if (currentCoinType == CoinType.Nickel) nickelsRemaining--;
        else if (currentCoinType == CoinType.Dime) dimesRemaining--;
        else if (currentCoinType == CoinType.Quarter) quartersRemaining--;
    }

    void SelectFirstAvailableCoin()
    {
        if (nickelsRemaining > 0) SetCoinType(CoinType.Nickel);
        else if (dimesRemaining > 0) SetCoinType(CoinType.Dime);
        else if (quartersRemaining > 0) SetCoinType(CoinType.Quarter);
    }

    void SetCoinType(CoinType type)
    {
        currentCoinType = type;
        maxSpinTime = type switch
        {
            CoinType.Quarter => quarterSpinTime,
            CoinType.Dime => dimeSpinTime,
            _ => nickelSpinTime
        };
        spinTimeRemaining = maxSpinTime;
    }

    void StartAiming()
    {
        isAiming = true;
        meterValue = 0f;
    }

    void Shoot()
    {
        isAiming = false;
        isShotInProgress = true;
        coinIsSpinning = true;
        spinTimeRemaining = maxSpinTime;
        UseCoin();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;

        // convert meter value to shot angle (max 30 degrees up or down)
        float angle = meterValue * 30f;
        rb.linearVelocity = (Vector2)(Quaternion.Euler(0, 0, -angle) * Vector2.right) * shotPower;
    }

    void StopCoin()
    {
        coinIsSpinning = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        if (transform.position.x < 15f && scoreTable != null)
            scoreTable.OnMiss();

        Invoke(nameof(ResetCoin), 1f);
    }

    public void OnScored()
    {
        coinIsSpinning = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        scoreTable?.AddScore(GetCurrentCoinValue());
        Invoke(nameof(ResetCoin), 1f);
    }

    public void OnBlocked()
    {
        coinIsSpinning = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        scoreTable?.OnBlocked();
        Invoke(nameof(ResetCoin), 1f);
    }

    public int GetCurrentCoinValue()
    {
        return currentCoinType switch
        {
            CoinType.Quarter => quarterValue,
            CoinType.Dime => dimeValue,
            _ => nickelValue
        };
    }

    void ResetCoin()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        isShotInProgress = false;
        coinIsSpinning = false;
        meterValue = 0f;

        if (GetCurrentCoinCount() <= 0) SelectFirstAvailableCoin();
        else SetCoinType(currentCoinType);
        if (!HasCoinsRemaining()) scoreTable?.OnOutOfCoins();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Keeper")) OnBlocked();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Keeper")) OnBlocked();
    }
}
