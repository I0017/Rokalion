using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] private Image fade;
    public bool fadeNeeded = true;
    public static Fade Instance;
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
    private void Start()
    {
        fade.canvasRenderer.SetAlpha(0f);
    }
    void Update()
    {
        if (fadeNeeded)
        {
            fade.canvasRenderer.SetAlpha(1f);
            fade.CrossFadeAlpha(0, 5, false);
            fadeNeeded = false;
        }
    }
}
