using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    public GameObject UpgradeUI;

    void Update() { }

    public void UpgradePanel()
    {
        UpgradeUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void Close()
    {
        UpgradeUI.SetActive(false);
        Time.timeScale = 1;
    }
}
