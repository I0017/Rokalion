using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hint : MonoBehaviour
{
    [SerializeField] private GameObject[] hintToHide;

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < hintToHide.Length; i++)
            {
                hintToHide[i].SetActive(false);
            }
        }
    }
}
