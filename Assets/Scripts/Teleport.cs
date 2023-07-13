using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private GameObject transitionTo;
    [SerializeField] public Transform startingPointOfTransition;
    [SerializeField] private Vector2 exitDir;
    [SerializeField] private float exitTime;
    [Space(7)]

    [Header("Fade Settings")]
    [SerializeField] private float fadeTime;
    [SerializeField] private Image fadeImage;

    public static Teleport Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("enter");
            fadeImage.enabled = true;
            fadeImage.CrossFadeAlpha(0, fadeTime, true);
            PlayerController.Instance.transform.position = startingPointOfTransition.position;
            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDir, exitTime));
            PlayerController.Instance.pState.cutscene = true;
            fadeImage.enabled = false;
        }
    }
}
