using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private Image healthImage;
    [SerializeField] private Text healthText;

    public void SetHealthAmount(float healthCurrent, float healthMax)
    {
        float percent = (healthCurrent / healthMax);
        healthImage.fillAmount = percent;
        healthText.text = healthCurrent + " / " + healthMax;
        print(percent);
    }
}
