using UnityEngine;

public class BulletMove : MonoBehaviour
{
    public float speed = 10f;  
    public Vector2 direction = Vector2.right;

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        //Debug.Log("direction x:" + direction.x + " , y:" + direction.y);
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }    
}
