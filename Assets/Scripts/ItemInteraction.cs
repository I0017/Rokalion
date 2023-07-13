using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    [SerializeField] GameObject canvasToShow;
    public bool canvasIsShown = false;
    public bool canvasWasJustShown = false;

    public static ItemInteraction Instance;
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

    void Update()
    {
        if (Distance() <= 3 && PlayerController.Instance.Inspect && !canvasIsShown && !canvasWasJustShown)
        {
            canvasToShow.gameObject.SetActive(true);
            canvasIsShown = true;
        }
        if (Distance() >= 5 && canvasWasJustShown)
        {
            canvasWasJustShown = false;
        }
    }
    private float Distance()
    {
        return Vector3.Distance(PlayerController.Instance.transform.position, Instance.transform.position);
    }
}
