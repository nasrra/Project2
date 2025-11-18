using UnityEngine;

[DefaultExecutionOrder(-10)]
public class UiManager : MonoBehaviour
{
    [SerializeField] GameObject hud;

    void Awake()
    {
        hud.SetActive(true);
    }
}
