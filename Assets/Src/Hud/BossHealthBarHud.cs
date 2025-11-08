using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class BossHealthBarHud : Entropek.SingletonMonoBehaviour<BossHealthBarHud>
{
    [Header(nameof(BossHealthBarHud)+" Components")]
    [SerializeField] private NamedHealthBar namedHealthBar;
    public NamedHealthBar NamedHealthBar => namedHealthBar;

    protected override void Awake()
    {
        base.Awake();
        namedHealthBar.Deactivate();
    }
}
