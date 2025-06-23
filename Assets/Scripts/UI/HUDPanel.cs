using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDPanel : UIPanel
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider energyBar;

    private void Update()
    {
        if (timeText != null)
        {
            timeText.text = System.DateTime.Now.ToString("HH:mm");
        }
    }

    public void UpdateMoney(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + amount.ToString();
        }
    }

    public void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    public void UpdateEnergy(float current, float max)
    {
        if (energyBar != null)
        {
            energyBar.maxValue = max;
            energyBar.value = current;
        }
    }
}