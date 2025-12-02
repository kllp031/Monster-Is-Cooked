using UnityEngine;

public class BulletMove : MonoBehaviour
{
    public float speed = 10f;  
    public Vector2 direction = Vector2.right;

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }    
}
