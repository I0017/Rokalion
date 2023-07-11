using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string transitionTo;
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Vector2 exitDir;
    [SerializeField] private float exitTime;

    private void Start()
    {
        if (transitionTo == GameManager.Instance.transitionedFromScene)
        {
            PlayerController.Instance.transform.position = startingPoint.position;
            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDir, exitTime));
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.transitionedFromScene = SceneManager.GetActiveScene().name;
            PlayerController.Instance.pState.cutscene = true;
            SceneManager.LoadScene(transitionTo);
        }
    }
}
