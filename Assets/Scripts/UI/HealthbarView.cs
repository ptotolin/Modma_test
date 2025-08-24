using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarView : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI barText;

    private float health;
    private float healthMax;

    public void SetHealth(float health, float healthMax)
    {
        this.health = health;
        this.healthMax = healthMax;
        
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        barText.text = $"{(int)health}/{(int)healthMax}";
        barImage.fillAmount = health / healthMax;
    }
}
