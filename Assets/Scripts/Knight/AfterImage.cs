using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] float fadeSpeed = 5f;
    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Color c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        if (c.a <= 0)
            Destroy(gameObject);
    }
}
