using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Enemy enemy))
        {
            enemy.Kill();
        }
        else if(other.TryGetComponent(out Player player))
        {
            player.WarpToLastSafePosition();
        }
    }
}
