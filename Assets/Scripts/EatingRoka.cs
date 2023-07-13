using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatingRoka : MonoBehaviour
{
    [SerializeField] private Sprite eatenRokaSprite;
    private bool isEaten = false;

    void Update()
    {
        if (Distance() <= 3 && PlayerController.Instance.Inspect && !isEaten)
        {
            this.GetComponent<SpriteRenderer>().sprite = eatenRokaSprite;
            PlayerController.Instance.maxAirJumps = 1;
            isEaten = true;
        }
    }
    private float Distance()
    {
        return Vector3.Distance(PlayerController.Instance.transform.position, this.transform.position);
    }
}
