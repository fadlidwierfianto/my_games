using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Health Bar Reference")]
    [SerializeField]
    private HealthBar healthBar;

    [Header("Player Reference")]
    [SerializeField]
    private PlayerController playerController;

    private int lastKnownHealth;
    private int maxHealth;

    private void Start()
    {
        // Cari komponen jika tidak di-assign
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (healthBar == null)
            healthBar = FindObjectOfType<HealthBar>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController tidak ditemukan!");
            return;
        }

        if (healthBar == null)
        {
            Debug.LogError("HealthBar tidak ditemukan!");
            return;
        }

        // Inisialisasi health bar
        maxHealth = playerController.maxHealth;
        lastKnownHealth = maxHealth;
        healthBar.InitializeHealthBar(maxHealth);
    }

    private void Update()
    {
        // Cek apakah health berubah
        if (playerController != null)
        {
            int currentHealth = GetCurrentHealth();

            if (currentHealth != lastKnownHealth)
            {
                // Update health bar
                healthBar.UpdateHealthBar(currentHealth);
                lastKnownHealth = currentHealth;

                // Optional: Tambahkan efek atau sound saat health berubah
                OnHealthChanged(currentHealth);
            }
        }
    }

    /// <summary>
    /// Mendapatkan current health dari PlayerController
    /// Karena currentHealth adalah private, kita perlu menggunakan reflection atau menambahkan public property
    /// </summary>
    private int GetCurrentHealth()
    {
        // Cara ini menggunakan reflection untuk mengakses private field
        // Alternatifnya, tambahkan public property di PlayerController
        var field = typeof(PlayerController).GetField(
            "currentHealth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field != null)
            return (int)field.GetValue(playerController);

        return maxHealth; // Fallback
    }

    /// <summary>
    /// Dipanggil ketika health berubah
    /// </summary>
    private void OnHealthChanged(int newHealth)
    {
        // Di sini bisa ditambahkan efek tambahan seperti:
        // - Sound effect
        // - Screen shake
        // - Particle effect
        // - UI notification

        if (healthBar.IsHealthCritical())
        {
            // Health critical - bisa tambahkan warning sound atau efek
            Debug.Log("Health Critical!");
        }
        else if (healthBar.IsHealthWarning())
        {
            // Health warning
            Debug.Log("Health Warning!");
        }
    }

    /// <summary>
    /// Method untuk healing player (opsional)
    /// </summary>
    public void HealPlayer(int healAmount)
    {
        // Implementasi healing jika diperlukan
        // Anda perlu menambahkan method HealPlayer di PlayerController
        Debug.Log($"Healing player for {healAmount} points");
    }
}
