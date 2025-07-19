using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class StatUpgrade
{
    public string statName;
    public int currentLevel;
    public int maxLevel;
    public int baseCost;
    public float costMultiplier;
    public float baseValue;
    public float upgradeIncrement;

    public int GetUpgradeCost()
    {
        if (currentLevel >= maxLevel)
            return -1; // Max level reached
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public float GetCurrentValue()
    {
        return baseValue + (upgradeIncrement * currentLevel);
    }

    public float GetNextValue()
    {
        if (currentLevel >= maxLevel)
            return GetCurrentValue();
        return baseValue + (upgradeIncrement * (currentLevel + 1));
    }
}

public class PlayerUpgradeSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject upgradePanel;
    public Button closeButton;
    public TextMeshProUGUI pointDisplayText;

    [Header("Stat Upgrade Buttons")]
    public Button healthUpgradeButton;
    public Button attackDamageUpgradeButton;
    public Button attackCooldownUpgradeButton;
    public Button attackSpeedUpgradeButton;
    public Button movementSpeedUpgradeButton;
    public Button attackRangeUpgradeButton;

    [Header("Stat Name Texts")]
    public TextMeshProUGUI healthNameText;
    public TextMeshProUGUI attackDamageNameText;
    public TextMeshProUGUI attackCooldownNameText;
    public TextMeshProUGUI attackSpeedNameText;
    public TextMeshProUGUI movementSpeedNameText;
    public TextMeshProUGUI attackRangeNameText;

    [Header("Cost Texts")]
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI attackDamageCostText;
    public TextMeshProUGUI attackCooldownCostText;
    public TextMeshProUGUI attackSpeedCostText;
    public TextMeshProUGUI movementSpeedCostText;
    public TextMeshProUGUI attackRangeCostText;

    [Header("Current Stat Texts")]
    public TextMeshProUGUI healthCurrentText;
    public TextMeshProUGUI attackDamageCurrentText;
    public TextMeshProUGUI attackCooldownCurrentText;
    public TextMeshProUGUI attackSpeedCurrentText;
    public TextMeshProUGUI movementSpeedCurrentText;
    public TextMeshProUGUI attackRangeCurrentText;

    [Header("Plus Stat Texts")]
    public TextMeshProUGUI healthPlusText;
    public TextMeshProUGUI attackDamagePlusText;
    public TextMeshProUGUI attackCooldownPlusText;
    public TextMeshProUGUI attackSpeedPlusText;
    public TextMeshProUGUI movementSpeedPlusText;
    public TextMeshProUGUI attackRangePlusText;

    [Header("Upgrade Settings")]
    public StatUpgrade healthUpgrade = new StatUpgrade
    {
        statName = "Health",
        currentLevel = 0,
        maxLevel = 30,
        baseCost = 10,
        costMultiplier = 1.5f,
        baseValue = 200f,
        upgradeIncrement = 25f
    };

    public StatUpgrade attackDamageUpgrade = new StatUpgrade
    {
        statName = "Attack Damage",
        currentLevel = 0,
        maxLevel = 30,
        baseCost = 15,
        costMultiplier = 1.6f,
        baseValue = 25f,
        upgradeIncrement = 5f
    };

    public StatUpgrade attackCooldownUpgrade = new StatUpgrade
    {
        statName = "Attack Cooldown",
        currentLevel = 0,
        maxLevel = 10,
        baseCost = 20,
        costMultiplier = 1.8f,
        baseValue = 0.5f,
        upgradeIncrement = -0.03f // Negative karena kita ingin mengurangi cooldown
    };

    public StatUpgrade attackSpeedUpgrade = new StatUpgrade
    {
        statName = "Attack Speed",
        currentLevel = 0,
        maxLevel = 12,
        baseCost = 18,
        costMultiplier = 1.7f,
        baseValue = 0.5f,
        upgradeIncrement = -0.02f // Negative karena kita ingin mengurangi attack duration
    };

    public StatUpgrade movementSpeedUpgrade = new StatUpgrade
    {
        statName = "Movement Speed",
        currentLevel = 0,
        maxLevel = 30,
        baseCost = 12,
        costMultiplier = 1.4f,
        baseValue = 3f,
        upgradeIncrement = 0.2f
    };

    public StatUpgrade attackRangeUpgrade = new StatUpgrade
    {
        statName = "Attack Range",
        currentLevel = 0,
        maxLevel = 10,
        baseCost = 25,
        costMultiplier = 2f,
        baseValue = 1f,
        upgradeIncrement = 0.1f
    };

    private PlayerController playerController;
    private bool isPanelOpen = false;

    // Original player stats (to restore base values)
    private float originalMoveSpeed;
    private int originalMaxHealth;
    private int originalAttackDamage;
    private float originalAttackCooldown;
    private float originalAttackDuration;
    private float originalGridCellSize;
    private Vector2 originalAttackAreaSize;

    private void Start()
    {
        // Find PlayerController
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found!");
            return;
        }

        // Store original values
        StoreOriginalStats();

        // Initialize upgrade base values with current player stats
        InitializeUpgradeBaseValues();

        // Setup UI
        SetupUI();

        // Close panel initially
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        // Update UI
        UpdateAllUI();
    }

    private void StoreOriginalStats()
    {
        // Get original values from PlayerController
        originalMoveSpeed = playerController.MoveSpeed;
        originalMaxHealth = playerController.MaxHealth;
        originalAttackDamage = playerController.AttackDamage;
        originalAttackCooldown = playerController.AttackCooldownValue;
        originalAttackDuration = playerController.AttackDuration;
        originalGridCellSize = playerController.GridCellSize;
        originalAttackAreaSize = playerController.AttackAreaSize;
    }

    private void InitializeUpgradeBaseValues()
    {
        healthUpgrade.baseValue = originalMaxHealth;
        movementSpeedUpgrade.baseValue = originalMoveSpeed;
        attackDamageUpgrade.baseValue = originalAttackDamage;
        attackCooldownUpgrade.baseValue = originalAttackCooldown;
        attackSpeedUpgrade.baseValue = originalAttackDuration;
        attackRangeUpgrade.baseValue = originalGridCellSize;
    }

    private void SetupUI()
    {
        // Setup close button
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUpgradePanel);

        // Setup upgrade buttons
        if (healthUpgradeButton != null)
            healthUpgradeButton.onClick.AddListener(() => UpgradeStat(healthUpgrade, "Health"));

        if (attackDamageUpgradeButton != null)
            attackDamageUpgradeButton.onClick.AddListener(
                () => UpgradeStat(attackDamageUpgrade, "AttackDamage")
            );

        if (attackCooldownUpgradeButton != null)
            attackCooldownUpgradeButton.onClick.AddListener(
                () => UpgradeStat(attackCooldownUpgrade, "AttackCooldown")
            );

        if (attackSpeedUpgradeButton != null)
            attackSpeedUpgradeButton.onClick.AddListener(
                () => UpgradeStat(attackSpeedUpgrade, "AttackSpeed")
            );

        if (movementSpeedUpgradeButton != null)
            movementSpeedUpgradeButton.onClick.AddListener(
                () => UpgradeStat(movementSpeedUpgrade, "MovementSpeed")
            );

        if (attackRangeUpgradeButton != null)
            attackRangeUpgradeButton.onClick.AddListener(
                () => UpgradeStat(attackRangeUpgrade, "AttackRange")
            );

        // Setup stat names (static text)
        if (healthNameText != null)
            healthNameText.text = "Health";
        if (attackDamageNameText != null)
            attackDamageNameText.text = "Attack Damage";
        if (attackCooldownNameText != null)
            attackCooldownNameText.text = "Attack Cooldown";
        if (attackSpeedNameText != null)
            attackSpeedNameText.text = "Attack Speed";
        if (movementSpeedNameText != null)
            movementSpeedNameText.text = "Movement Speed";
        if (attackRangeNameText != null)
            attackRangeNameText.text = "Attack Range";
    }

    private void Update()
    {
        // Toggle upgrade panel with a key (optional)
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradePanel();
        }
    }

    public void OpenUpgradePanel()
    {
        if (upgradePanel != null && playerController != null && !playerController.IsDead)
        {
            upgradePanel.SetActive(true);
            isPanelOpen = true;
            playerController.DisableControls();
            UpdateAllUI();

            // Pause game if you want
            // Time.timeScale = 0f;
        }
    }

    public void CloseUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            isPanelOpen = false;

            if (playerController != null)
                playerController.EnableControls();

            // Unpause game if paused
            // Time.timeScale = 1f;
        }
    }

    public void ToggleUpgradePanel()
    {
        if (isPanelOpen)
            CloseUpgradePanel();
        else
            OpenUpgradePanel();
    }

    private void UpgradeStat(StatUpgrade statUpgrade, string statType)
    {
        if (playerController == null || playerController.IsDead)
            return;

        int cost = statUpgrade.GetUpgradeCost();

        // Check if upgrade is possible
        if (cost == -1) // Max level
        {
            Debug.Log($"{statUpgrade.statName} sudah mencapai level maksimum!");
            return;
        }

        if (playerController.CurrentPoints < cost)
        {
            Debug.Log($"Point tidak cukup! Dibutuhkan {cost} point.");
            return;
        }

        // Deduct points
        playerController.RemovePoints(cost);

        // Upgrade stat
        statUpgrade.currentLevel++;

        // Apply upgrade to player
        ApplyStatUpgrade(statType, statUpgrade.GetCurrentValue());

        // Update UI
        UpdateAllUI();

        Debug.Log($"{statUpgrade.statName} upgraded to level {statUpgrade.currentLevel}!");
    }

    private void ApplyStatUpgrade(string statType, float newValue)
    {
        switch (statType)
        {
            case "Health":
                playerController.SetMaxHealth((int)newValue);
                Debug.Log($"Health upgraded to {newValue}");
                break;

            case "AttackDamage":
                playerController.SetAttackDamage((int)newValue);
                Debug.Log($"Attack Damage upgraded to {newValue}");
                break;

            case "AttackCooldown":
                playerController.SetAttackCooldown(newValue);
                Debug.Log($"Attack Cooldown upgraded to {newValue}");
                break;

            case "AttackSpeed":
                playerController.SetAttackDuration(newValue);
                Debug.Log($"Attack Speed upgraded to {newValue}");
                break;

            case "MovementSpeed":
                playerController.SetMoveSpeed(newValue);
                Debug.Log($"Movement Speed upgraded to {newValue}");
                break;

            case "AttackRange":
                playerController.SetAttackRange(newValue);
                Debug.Log($"Attack Range upgraded to {newValue}");
                break;
        }
    }

    private void UpdateAllUI()
    {
        if (playerController == null)
            return;

        // Update point display
        if (pointDisplayText != null)
            pointDisplayText.text = $"Points: {playerController.CurrentPoints}";

        // Update each stat UI
        UpdateStatUI(
            healthUpgrade,
            healthCostText,
            healthCurrentText,
            healthPlusText,
            healthUpgradeButton
        );
        UpdateStatUI(
            attackDamageUpgrade,
            attackDamageCostText,
            attackDamageCurrentText,
            attackDamagePlusText,
            attackDamageUpgradeButton
        );
        UpdateStatUI(
            attackCooldownUpgrade,
            attackCooldownCostText,
            attackCooldownCurrentText,
            attackCooldownPlusText,
            attackCooldownUpgradeButton
        );
        UpdateStatUI(
            attackSpeedUpgrade,
            attackSpeedCostText,
            attackSpeedCurrentText,
            attackSpeedPlusText,
            attackSpeedUpgradeButton
        );
        UpdateStatUI(
            movementSpeedUpgrade,
            movementSpeedCostText,
            movementSpeedCurrentText,
            movementSpeedPlusText,
            movementSpeedUpgradeButton
        );
        UpdateStatUI(
            attackRangeUpgrade,
            attackRangeCostText,
            attackRangeCurrentText,
            attackRangePlusText,
            attackRangeUpgradeButton
        );
    }

    private void UpdateStatUI(
        StatUpgrade statUpgrade,
        TextMeshProUGUI costText,
        TextMeshProUGUI currentText,
        TextMeshProUGUI plusText,
        Button upgradeButton
    )
    {
        if (costText == null || currentText == null || plusText == null || upgradeButton == null)
            return;

        int cost = statUpgrade.GetUpgradeCost();
        float currentValue = statUpgrade.GetCurrentValue();
        float nextValue = statUpgrade.GetNextValue();

        // Update cost text
        if (cost == -1)
        {
            costText.text = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            costText.text = cost.ToString();
            upgradeButton.interactable = playerController.CurrentPoints >= cost;
        }

        // Update current stat text
        if (statUpgrade.statName.Contains("Cooldown") || statUpgrade.statName.Contains("Speed"))
        {
            currentText.text = currentValue.ToString("F2") + "s";
            if (cost != -1)
                plusText.text = "+" + nextValue.ToString("F2") + "s";
            else
                plusText.text = "MAX";
        }
        else
        {
            currentText.text = currentValue.ToString("F0");
            if (cost != -1)
                plusText.text = "+" + nextValue.ToString("F0");
            else
                plusText.text = "MAX";
        }
    }

    // Save/Load system (optional)
    public void SaveUpgrades()
    {
        PlayerPrefs.SetInt("HealthUpgradeLevel", healthUpgrade.currentLevel);
        PlayerPrefs.SetInt("AttackDamageUpgradeLevel", attackDamageUpgrade.currentLevel);
        PlayerPrefs.SetInt("AttackCooldownUpgradeLevel", attackCooldownUpgrade.currentLevel);
        PlayerPrefs.SetInt("AttackSpeedUpgradeLevel", attackSpeedUpgrade.currentLevel);
        PlayerPrefs.SetInt("MovementSpeedUpgradeLevel", movementSpeedUpgrade.currentLevel);
        PlayerPrefs.SetInt("AttackRangeUpgradeLevel", attackRangeUpgrade.currentLevel);
        PlayerPrefs.Save();
    }

    public void LoadUpgrades()
    {
        healthUpgrade.currentLevel = PlayerPrefs.GetInt("HealthUpgradeLevel", 0);
        attackDamageUpgrade.currentLevel = PlayerPrefs.GetInt("AttackDamageUpgradeLevel", 0);
        attackCooldownUpgrade.currentLevel = PlayerPrefs.GetInt("AttackCooldownUpgradeLevel", 0);
        attackSpeedUpgrade.currentLevel = PlayerPrefs.GetInt("AttackSpeedUpgradeLevel", 0);
        movementSpeedUpgrade.currentLevel = PlayerPrefs.GetInt("MovementSpeedUpgradeLevel", 0);
        attackRangeUpgrade.currentLevel = PlayerPrefs.GetInt("AttackRangeUpgradeLevel", 0);

        // Apply all upgrades
        ApplyAllUpgrades();
        UpdateAllUI();
    }

    private void ApplyAllUpgrades()
    {
        ApplyStatUpgrade("Health", healthUpgrade.GetCurrentValue());
        ApplyStatUpgrade("AttackDamage", attackDamageUpgrade.GetCurrentValue());
        ApplyStatUpgrade("AttackCooldown", attackCooldownUpgrade.GetCurrentValue());
        ApplyStatUpgrade("AttackSpeed", attackSpeedUpgrade.GetCurrentValue());
        ApplyStatUpgrade("MovementSpeed", movementSpeedUpgrade.GetCurrentValue());
        ApplyStatUpgrade("AttackRange", attackRangeUpgrade.GetCurrentValue());
    }

    public void ResetAllUpgrades()
    {
        healthUpgrade.currentLevel = 0;
        attackDamageUpgrade.currentLevel = 0;
        attackCooldownUpgrade.currentLevel = 0;
        attackSpeedUpgrade.currentLevel = 0;
        movementSpeedUpgrade.currentLevel = 0;
        attackRangeUpgrade.currentLevel = 0;

        ApplyAllUpgrades();
        UpdateAllUI();
    }
}
