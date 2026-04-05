using Fusion;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}
