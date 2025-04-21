using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Sprite hub_open, hub_close;
    SpriteRenderer hub;

    Coroutine coroutine;
    // Start is called before the first frame update
    void Start()
    {
        hub = GetComponent<SpriteRenderer>();
        hub.sprite = hub_close;
    }
    public void Open()
    {
        hub.sprite = hub_open;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(Close(1));
    }
    IEnumerator Close(float delay)
    {
        yield return new WaitForSeconds(delay);
        hub.sprite = hub_close;
    }
}
