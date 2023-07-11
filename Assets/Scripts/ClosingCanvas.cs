using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosingCanvas : MonoBehaviour
{
    [SerializeField] GameObject canvasToShow;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (ItemInteraction.Instance.canvasIsShown)
            {
                canvasToShow.gameObject.SetActive(false);
                ItemInteraction.Instance.canvasIsShown = false;
                ItemInteraction.Instance.canvasWasJustShown = true;
            }
        }
    }
}
