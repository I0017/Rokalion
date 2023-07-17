using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teleportation : MonoBehaviour
{
    [Header("Teleportation Settings")]
    [SerializeField] private string teleportToLocationName;
    [SerializeField] private GameObject teleportTo;
    [SerializeField] private Vector2 exitDir;
    [SerializeField] private float exitTime;
    [Space(7)]

    [Header("Fade Settings")]
    [SerializeField] private float fadeTime;
    [SerializeField] private Image fadeImage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            fadeImage.enabled = true;
            PlayerController.Instance.transform.position = teleportTo.transform.position;
            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDir, exitTime));
            PlayerController.Instance.pState.cutscene = true;
            if (teleportToLocationName == "Cliffs")
            {
                PlayerController.Instance.pState.isInCliffs = true;
                PlayerController.Instance.pState.isInMoss = false;
            }
            if (teleportToLocationName == "Moss")
            {
                PlayerController.Instance.pState.isInMoss = true;
                PlayerController.Instance.pState.isInCliffs = false;
            }
        }
    }
}
