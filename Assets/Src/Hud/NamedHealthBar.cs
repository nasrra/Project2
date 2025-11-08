using Entropek.Combat;
using Entropek.EntityStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NamedHealthBar : HealthBar
{
    [Header(nameof(NamedHealthBar)+" Components")]
    [SerializeField] private TextMeshProUGUI nameTag;

    public void Activate(Health health, string name)
    {
        nameTag.text = name;
        Activate(health);
    }
}

