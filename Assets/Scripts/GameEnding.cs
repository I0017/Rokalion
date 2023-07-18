using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class GameEnding : MonoBehaviour
{
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private GameObject toBeContinued;
    [SerializeField] private GameObject moveBlackScreenTo;
    [SerializeField] private GameObject moveToBeContinuedTo;
    [SerializeField] private GameObject bgMusicToDisable;
    [SerializeField] private AudioSource endingMusic;
    private bool moveNeeded = true;

    void Start()
    {
        blackScreen.SetActive(false);
        //blackScreen.transform.position = new Vector3(1702, 0, 0);
        toBeContinued.SetActive(false);
        //toBeContinued.transform.position = new Vector3(0, -897, 0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController.Instance.pState.end = true;
        bgMusicToDisable.SetActive(false);
        endingMusic.Play();
        blackScreen.SetActive(true); 
        toBeContinued.SetActive(true);
        if (moveNeeded)
        {
            blackScreen.transform.position = moveBlackScreenTo.transform.position;
            toBeContinued.transform.position = moveToBeContinuedTo.transform.position;
            moveNeeded = false;
        }
    }
}
