using UnityEngine;

[DefaultExecutionOrder(-10)]
public class HudManager : MonoBehaviour
{
    [SerializeField] GameObject hud;

    void Awake()
    {
        hud.SetActive(true);
    }
}
