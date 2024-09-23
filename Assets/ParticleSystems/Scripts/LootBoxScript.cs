using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class LootBoxScript : MonoBehaviour
{
    public GameObject lootBox;
    public GameObject lootBoxFractured;
    public GameObject lootReward;
    public GameObject lootImpactPrefab;

    public GuidComponent guid;

    private Animator animator;
    private bool hasLootBeenRewarded = false; // Flag to track loot reward


    public Action<Transform> OnGet;

    [SerializeField] private Transform interactionTimerParent;
    [SerializeField] private Image interactionTimerFill;

    private float interactionTimer;
    private float interactionTimerMax = 2f;

    private Tween interactionReverseTimerTween;
    private Coroutine interactionTimerCoroutine;

    private string ID => guid.GetGuid().ToString();

    private void OnTriggerEnter(Collider other)
    {
        Hover();

        if (!interactionTimerParent.gameObject.activeSelf)
        {
            interactionTimerParent.gameObject.SetActive(true);
        }
        interactionReverseTimerTween?.Kill();

        interactionTimerCoroutine = StartCoroutine(InteractionTimerRoutine());
    }

    private IEnumerator InteractionTimerRoutine()
    {
        while (interactionTimer < interactionTimerMax)
        {
            interactionTimer += Time.deltaTime;
            interactionTimerFill.fillAmount = interactionTimer / interactionTimerMax;
            yield return null;
        }

        GiveReward();
    }

    private void GiveReward()
    {
        Open();

        PlayerPrefs.SetInt(ID, 1);
        interactionTimerParent.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        Idle();
        interactionReverseTimerTween = DOTween.To(() => interactionTimer, x => interactionTimer = x, 0, interactionTimer / (interactionTimerMax * 4)).OnUpdate(() =>
        {
            interactionTimerFill.fillAmount = interactionTimer / interactionTimerMax;
        }).OnComplete(() =>
        {
            interactionTimerParent.gameObject.SetActive(false);
        });

        if (interactionTimerCoroutine != null)
        {
            StopCoroutine(interactionTimerCoroutine);
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        bool isRecived = PlayerPrefs.GetInt(ID, 0) == 1;

        if (isRecived)
            transform.parent.gameObject.SetActive(false);
    }

    private void Hover()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Hover", true);
    }

    private void Idle()
    {
        animator.SetBool("Idle", true);
        animator.SetBool("Hover", false);
    }

    private void Open()
    {
        animator.SetBool("Open", true);
    }

    public void LootReward()
    {
        var loot = Instantiate(lootReward, transform) as GameObject;
        loot.SetActive(true);
        loot.transform.localPosition = new Vector3(0, -1.56f, 0);

        if (lootImpactPrefab != null && !hasLootBeenRewarded)
        {
            var lootImpactVFX = Instantiate(lootImpactPrefab, transform.position, transform.rotation) as GameObject;
            Destroy(lootImpactVFX, 2f);
        }

        hasLootBeenRewarded = true; // Set flag after rewarding loot
        Destroy(lootBox);

        if (lootBoxFractured != null)
        {
            var fracturedObject = Instantiate(lootBoxFractured) as GameObject;
        }
    }
}
