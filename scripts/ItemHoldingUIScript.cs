using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemHoldingUIScript : MonoBehaviour

{
    [SerializeField] Sprite handReference;
    static Sprite hand;
    [SerializeField] static private Animator animator;
    [SerializeField] static private Image targetImage;
    static public float playerSpeed = 0;
    static private float useAnimationDuration = 0.4f;
    static private float useAnimationTimer = 0;
    void Awake()
    {
        hand = handReference;
        animator = GetComponent<Animator>();
        targetImage = GetComponent<Image>();
    }
    void Update()
    {
        if (useAnimationTimer > 0)
        {
            useAnimationTimer -= Time.deltaTime;
            if (useAnimationTimer <= 0)
            {
                useAnimationTimer = 0;
                if (playerSpeed > 0)
                    PlayWalkAnimation();
                else
                    PlayIdleAnimation();
            }
        }
        else
        {
            if (playerSpeed > 0)
            {
                PlayWalkAnimation();
            }
            else
            {
                PlayIdleAnimation();
            }
        }
    }

    static public void SetSprite(Sprite sprite)
    {
        targetImage.enabled = true;
        targetImage.sprite = sprite;
    }

    static public void ClearSprite()
    {
        targetImage.sprite = hand;
        targetImage.enabled = true;
    }
    static public void PlayUseAnimation()
    {
        animator.speed = 1;
        animator.Play("ItemHoldingUse");
        useAnimationTimer = useAnimationDuration;
    }
    static public void SetSpeed(float speed)
    {
        playerSpeed = speed;
    }

    void PlayIdleAnimation()
    {
        if (useAnimationTimer > 0)
            return;
        animator.speed = 1;
        animator.Play("ItemHoldingIdle");
    }

    void PlayWalkAnimation()
    {
        if (useAnimationTimer > 0)
            return;
        animator.speed = playerSpeed;
        animator.Play("ItemHoldingWalking");
    }
}
