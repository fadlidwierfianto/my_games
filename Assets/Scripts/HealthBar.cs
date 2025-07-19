using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField]
    private Image healthBarFill; // Bagian merah yang akan berkurang

    [SerializeField]
    private Image healthBarBackground; // Background health bar

    [SerializeField]
    private Image healthBarFrame; // Frame/border health bar (opsional)

    [Header("Health Bar Settings")]
    [SerializeField]
    private float animationSpeed = 2f; // Kecepatan animasi health bar

    [SerializeField]
    private bool useAnimation = true; // Apakah menggunakan animasi smooth

    [Header("Color Settings")]
    [SerializeField]
    private Color healthyColor = Color.green; // Warna saat health tinggi

    [SerializeField]
    private Color warningColor = Color.yellow; // Warna saat health menengah

    [SerializeField]
    private Color criticalColor = Color.red; // Warna saat health rendah

    [SerializeField]
    private float warningThreshold = 0.5f; // Threshold untuk warning color (50%)

    [SerializeField]
    private float criticalThreshold = 0.25f; // Threshold untuk critical color (25%)

    [Header("Animation Effects")]
    [SerializeField]
    private bool usePulseEffect = true; // Pulse effect saat health rendah

    [SerializeField]
    private float pulseSpeed = 2f;

    [SerializeField]
    private float pulseIntensity = 0.1f;

    private float currentHealth;
    private float maxHealth;
    private float targetFillAmount;
    private Coroutine healthUpdateCoroutine;
    private Coroutine pulseCoroutine;

    private void Start()
    {
        // Inisialisasi health bar
        if (healthBarFill == null)
        {
            Debug.LogError(
                "Health Bar Fill tidak ditemukan! Pastikan untuk assign Image component."
            );
            return;
        }

        // Set initial values
        targetFillAmount = 1f;
        healthBarFill.fillAmount = 1f;

        // Set initial color
        UpdateHealthBarColor();
    }

    /// <summary>
    /// Inisialisasi health bar dengan nilai maksimum
    /// </summary>
    public void InitializeHealthBar(float maxHealthValue)
    {
        maxHealth = maxHealthValue;
        currentHealth = maxHealthValue;
        targetFillAmount = 1f;

        if (healthBarFill != null)
            healthBarFill.fillAmount = 1f;

        UpdateHealthBarColor();
    }

    /// <summary>
    /// Update health bar dengan nilai health saat ini
    /// </summary>
    public void UpdateHealthBar(float newHealth)
    {
        if (maxHealth <= 0)
            return;

        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        targetFillAmount = currentHealth / maxHealth;

        // Stop previous animation if running
        if (healthUpdateCoroutine != null)
            StopCoroutine(healthUpdateCoroutine);

        // Start new animation
        if (useAnimation)
            healthUpdateCoroutine = StartCoroutine(AnimateHealthBar());
        else
            healthBarFill.fillAmount = targetFillAmount;

        UpdateHealthBarColor();
        HandlePulseEffect();
    }

    /// <summary>
    /// Update health bar dengan persentase (0-1)
    /// </summary>
    public void UpdateHealthBarPercentage(float percentage)
    {
        percentage = Mathf.Clamp01(percentage);
        targetFillAmount = percentage;

        if (healthUpdateCoroutine != null)
            StopCoroutine(healthUpdateCoroutine);

        if (useAnimation)
            healthUpdateCoroutine = StartCoroutine(AnimateHealthBar());
        else
            healthBarFill.fillAmount = targetFillAmount;

        UpdateHealthBarColor();
        HandlePulseEffect();
    }

    /// <summary>
    /// Animasi smooth untuk perubahan health bar
    /// </summary>
    private IEnumerator AnimateHealthBar()
    {
        float startFillAmount = healthBarFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            float currentFillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, elapsedTime);
            healthBarFill.fillAmount = currentFillAmount;
            yield return null;
        }

        healthBarFill.fillAmount = targetFillAmount;
    }

    /// <summary>
    /// Update warna health bar berdasarkan persentase health
    /// </summary>
    private void UpdateHealthBarColor()
    {
        if (healthBarFill == null)
            return;

        float healthPercentage = targetFillAmount;

        if (healthPercentage <= criticalThreshold)
        {
            healthBarFill.color = criticalColor;
        }
        else if (healthPercentage <= warningThreshold)
        {
            // Interpolasi antara critical dan warning color
            float t =
                (healthPercentage - criticalThreshold) / (warningThreshold - criticalThreshold);
            healthBarFill.color = Color.Lerp(criticalColor, warningColor, t);
        }
        else
        {
            // Interpolasi antara warning dan healthy color
            float t = (healthPercentage - warningThreshold) / (1f - warningThreshold);
            healthBarFill.color = Color.Lerp(warningColor, healthyColor, t);
        }
    }

    /// <summary>
    /// Handle pulse effect untuk health rendah
    /// </summary>
    private void HandlePulseEffect()
    {
        if (!usePulseEffect)
            return;

        if (targetFillAmount <= criticalThreshold)
        {
            if (pulseCoroutine == null)
                pulseCoroutine = StartCoroutine(PulseEffect());
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;

                // Reset scale
                if (healthBarFill != null)
                    healthBarFill.transform.localScale = Vector3.one;
            }
        }
    }

    /// <summary>
    /// Pulse effect untuk health bar
    /// </summary>
    private IEnumerator PulseEffect()
    {
        Vector3 originalScale = healthBarFill.transform.localScale;

        while (true)
        {
            // Scale up
            float time = 0;
            while (time < 1)
            {
                time += Time.deltaTime * pulseSpeed;
                float scale = Mathf.Lerp(1f, 1f + pulseIntensity, time);
                healthBarFill.transform.localScale = originalScale * scale;
                yield return null;
            }

            // Scale down
            time = 0;
            while (time < 1)
            {
                time += Time.deltaTime * pulseSpeed;
                float scale = Mathf.Lerp(1f + pulseIntensity, 1f, time);
                healthBarFill.transform.localScale = originalScale * scale;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Set visibility health bar
    /// /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// Get current health percentage
    /// </summary>
    public float GetHealthPercentage()
    {
        return targetFillAmount;
    }

    /// <summary>
    /// Check if health is critical
    /// </summary>
    public bool IsHealthCritical()
    {
        return targetFillAmount <= criticalThreshold;
    }

    /// <summary>
    /// Check if health is in warning range
    /// </summary>
    public bool IsHealthWarning()
    {
        return targetFillAmount <= warningThreshold;
    }
}
