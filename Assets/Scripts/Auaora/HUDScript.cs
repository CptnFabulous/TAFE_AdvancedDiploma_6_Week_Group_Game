using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private Image healthImage;
    [SerializeField] private Text healthText;
    [SerializeField] private Text displayTextRef;
    [SerializeField] private Text statText;
    [SerializeField] private Image dashImage;

    private void Start()
    {
        UpdateStatText();
    }

    public void SetHealthAmount(float healthCurrent, float healthMax)
    {
        float percent = (healthCurrent / healthMax);
        healthImage.fillAmount = percent;
        healthText.text = healthCurrent + " / " + healthMax;
        print(percent);
    }

    public void DisplayText(string displayText)
    {
        displayTextRef.enabled = true;
        displayTextRef.text = displayText;
        StartCoroutine("HideText");
    }

    public void UpdateStatText()
    {
        AbilityManager abil = AbilityManager.SoleManager;
        statText.text = "Health: " + abil.GetHealthBonus() + ", Force: " + abil.GetAttackForceBonus() + ", Range: " + abil.GetAttackRangeBonus() + ", Speed: " + abil.GetAttackSpeed() + ", Dash: " + abil.GetDashCooldownBonus();
    }

    public void UpdateDashCooldown(float current, float max)
    {
        if (current == -1f)
        {
            current = max;
        }
        dashImage.fillAmount = (current / max);
    }

    private IEnumerator HideText()
    {
        yield return new WaitForSeconds(3);
        displayTextRef.enabled = false;
    }
}
