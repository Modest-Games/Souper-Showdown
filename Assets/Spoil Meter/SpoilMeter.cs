using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpoilMeter : NetworkBehaviour
{
    public static SpoilMeter Instance { get; private set; }

    public delegate void SpoilMeterDelegate();
    public static event SpoilMeterDelegate SoupSpoiled;

    [SerializeField] private float value;
    public float maxValue;

    public RectTransform maskTransform;
    public Image fillLine;

    private float smoothTime;
    private float velocity;
    private float acceptableDifference;

    private Color green;

    private float currentTarget;

    private void Awake()
    {
        // singleton stuff
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // 0-100 value of the Spoil Meter:
        value = 0;

        // The transform for the Spoil Meter's mask:
        //maskTransform = GetComponent<RectTransform>();
        //fillLine = transform.GetChild(0).gameObject.GetComponent<Image>();

        // Variables for smoothing value changes to the Spoil Meter:
        smoothTime = 1.0f;
        velocity = 0.0f;
        acceptableDifference = 0.5f;

        green = new Color(152f/255f, 174f/255f, 81f/255f);

        currentTarget = 150;
    }

    private void ChangeSpoilMeterValue(float pollutantValue)
    {
        value = Mathf.Clamp(value + pollutantValue, 0f, maxValue);

        // Change Spoil Meter for all clients:
        if (IsServer)
            ChangeSpoilMeterClientRpc(pollutantValue);

        // handle the soup being spoiled (avengers end game)
        if (value >= maxValue)
        {
            // end the game for clients
            EndGameClientRpc();
            EndScreenManager.Instance.SpoilersWin();

            // end the game for server
            if (SoupSpoiled != null)
                SoupSpoiled();
        }
    }

    private IEnumerator SpoilMeterSmoothing(float target)
    {
        currentTarget = target;

        // Check if the position of the mask is "close enough" to where it should be:
        while (Mathf.Abs(target - maskTransform.anchoredPosition.x) > acceptableDifference)
        {
            float newPosition = Mathf.SmoothDamp(maskTransform.anchoredPosition.x, target, ref velocity, smoothTime);
            maskTransform.anchoredPosition = new Vector2(newPosition, 0f);

            yield return new WaitForEndOfFrame();
        }

        // After the smoothing is done, set the mask's position to the actual target for consistency:
        maskTransform.anchoredPosition = new Vector2(target, 0f);
    }
    

    private float CalculateMaskPosition(float current, float difference)
    {
        return (current + (difference * 18));
    }

    [ClientRpc]
    private void ChangeSpoilMeterClientRpc(float pollutantValue)
    {
        maskTransform.anchoredPosition = new Vector2(currentTarget, 0f);
        StopAllCoroutines();
        
        var maskTarget = Mathf.Clamp(CalculateMaskPosition(maskTransform.anchoredPosition.x, pollutantValue), 150f, 1800f);
        StartCoroutine(SpoilMeterSmoothing(maskTarget));

        // Change Fill Line's color to green: (animate this later)
        if(value > 66.66f)
        {
            fillLine.color = green;
        }
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        if (SoupSpoiled != null)
            SoupSpoiled();
    }

    private void OnSoupReceivedTrash(float influence, ulong throwerId)
    {
        ChangeSpoilMeterValue(influence);
    }

    private void OnEnable()
    {
        // setup event listeners
        SoupPot_Behaviour.SoupReceivedTrash += OnSoupReceivedTrash;
    }

    private void OnDisable()
    {
        // clear event listeners
        SoupPot_Behaviour.SoupReceivedTrash -= OnSoupReceivedTrash;
    }
}
