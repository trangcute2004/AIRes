using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Sprite hub_open, hub_close;
    SpriteRenderer hub;

    // The Coroutine responsible for closing the door after a delay
    Coroutine coroutine;
    
    void Start()
    {
        // Get the SpriteRenderer component attached to the door object
        hub = GetComponent<SpriteRenderer>();
        // Set the initial sprite to the "closed" sprite
        hub.sprite = hub_close;
    }

    //open the door
    public void Open()
    {
        // Change the door sprite to the "open" sprite
        hub.sprite = hub_open;

        // If there is an active coroutine running, stop it (this ensures that the door won't be closed immediately after opening)
        if (coroutine != null)
            StopCoroutine(coroutine);

        // Start a new coroutine to close the door after a delay of 1 second
        coroutine = StartCoroutine(Close(1));
    }

    // Coroutine to close the door after a specified delay
    IEnumerator Close(float delay)
    {
        yield return new WaitForSeconds(delay);
        // After the delay, change the door sprite back to the "closed" sprite
        hub.sprite = hub_close;
    }
}
