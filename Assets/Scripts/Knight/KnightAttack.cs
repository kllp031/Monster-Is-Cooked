using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightAttack : MonoBehaviour
{
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private float coolDownTime = 1.0f;

    private Coroutine coolDownCoroutine = null;
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && coolDownCoroutine == null)
        {
            if (attackEffect != null)
            {
                attackEffect.SetActive(true);
                attackEffect.GetComponent<Animator>().SetTrigger("Attack");
                coolDownCoroutine = StartCoroutine(CoolDownCoroutine());
            }
        }
    }
    IEnumerator CoolDownCoroutine()
    {
        yield return new WaitForSeconds(coolDownTime);
        coolDownCoroutine = null;
    }
}
