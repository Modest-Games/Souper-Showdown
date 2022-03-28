using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator : MonoBehaviour
{
    public SpriteAnimation spriteAnimation;
    public bool useOverrideDuration;
    public float overrideDuration;
    public bool isEnabled = true;
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        if (spriteAnimation != null && spriteAnimation.frames.Length > 0)
            image.sprite = spriteAnimation.frames[0];
    }

    void Update()
    {
        if (spriteAnimation == null)
            return;

        if (!isEnabled)
            return;

        float frameDuration = (useOverrideDuration ? overrideDuration : spriteAnimation.duration) / spriteAnimation.frames.Length;

        int index = Mathf.FloorToInt(Time.realtimeSinceStartup / frameDuration) % spriteAnimation.frames.Length;
        image.sprite = spriteAnimation.frames[index];
    }
}
