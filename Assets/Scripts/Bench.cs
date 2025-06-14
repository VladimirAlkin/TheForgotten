using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bench : MonoBehaviour
{
    bool inRange = false;
    [SerializeField] public bool interacted;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact") && inRange) interacted = true;
    }

    void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player")) inRange = true;
    }

    void OnTriggerExit2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player")) inRange = false;
    }
}
