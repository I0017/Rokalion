using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    private GameObject[] heartContainers;
    private Image[] heartFills;
    public Transform heartParent;
    public GameObject heartContainerPrefab;
    void Start()
    {
        heartContainers = new GameObject[PlayerController.Instance.maxHealth];
        heartFills = new Image[PlayerController.Instance.maxHealth];
        PlayerController.Instance.onHealthChangedCallback += UpdateHeartsHUD;
        InsantiateHeartContainers();
        UpdateHeartsHUD();
    }
    void Update()
    {
        
    }
    void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < PlayerController.Instance.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }
    void SetFilledHearts()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < PlayerController.Instance.Health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }
    void InsantiateHeartContainers()
    {
        for (int i = 0; i < PlayerController.Instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }
    void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }
}
