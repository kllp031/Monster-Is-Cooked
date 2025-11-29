using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/* This class is used by the player, it detects any objects that have the "Interactable" script attached to them
   When player hit the specified "Interact button", simply call the "Interact()" function on the interactable object
   That object will handle the logic, not the player*/

[RequireComponent(typeof(Collider2D))]
public class InteractableDetector : MonoBehaviour
{
    private Interactable interactable;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable obj = collision?.GetComponent<Interactable>();
        if (obj)
        {
            interactable = obj;
            print("Interactable detected: " + obj.name);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable obj = collision?.GetComponent<Interactable>();
        if (obj)
        {
            if (interactable == obj) interactable = null;
        }
    }
    public void OnInteract(InputAction.CallbackContext callbackContext)
    {
        if(callbackContext.started)
        {
            interactable?.OnInteract(this.gameObject);
        }
    }
}
