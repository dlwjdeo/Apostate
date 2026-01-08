using UnityEngine;

public class PlayerIcon : MonoBehaviour
{
    public void MoveTo(Vector2 targetPos)
    {
        transform.position = targetPos;
    }
}