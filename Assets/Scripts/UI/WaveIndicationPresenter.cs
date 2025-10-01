using TMPro;
using UnityEngine;

public class WaveIndicationPresenter : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI waveNumberText;
    
    private static readonly int AnimFlagShow = Animator.StringToHash("Show");
    private static readonly int AnimFlagIdle = Animator.StringToHash("Idle");

    public void Show(int waveNumber)
    {
        waveNumberText.text = $"Wave #{waveNumber}";
        animator.SetBool(AnimFlagShow, true);
    }

    public void Hide()
    {
        animator.SetBool(AnimFlagShow, false);
    }
}
