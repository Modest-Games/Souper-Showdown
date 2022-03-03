using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NumberCounter : MonoBehaviour
{
    public TextMeshProUGUI Text;
    //public Animator Animator;
    public int CountFPS = 30;
    public float Duration = 1f;
    public string NumberFormat = "N0";
    private int _value;

    public int Value
    {
        get
        {
            return _value;
        }
        set
        {
            UpdateText(value);
            _value = value;
        }
    }

    private Coroutine CountingCoroutine;


    private void Awake()
    {
        Text = GetComponent<TextMeshProUGUI>();
        //Animator = GetComponent<Animator>();
        //Animator.Play("ScoreWiggle");
    }

    private void UpdateText(int newValue)
    {
        if(CountingCoroutine != null)
        {
            StopCoroutine(CountingCoroutine);
        }
        CountingCoroutine = StartCoroutine(CountText(newValue));
    }

    private IEnumerator CountText(int newValue)
    {
        WaitForSeconds Wait = new WaitForSeconds(1f / CountFPS);
        int previousValue = _value;
        int stepAmount;

        if(newValue - previousValue < 0)
        {
            stepAmount = Mathf.FloorToInt((newValue - previousValue) / (CountFPS * Duration)); //new Value = 20, previousValue = 0; CountFPS = 30, and Duration - 1;
        }
        else
        {
            stepAmount = Mathf.CeilToInt((newValue - previousValue) / (CountFPS * Duration)); //new Value = 20, previousValue = 0; CountFPS = 30, and Duration - 1;
        }

        if(previousValue < newValue)
        {
            while(previousValue < newValue)
            {
                previousValue += stepAmount;
                if(previousValue > newValue)
                {
                    previousValue = newValue;
                }
                Text.SetText(previousValue.ToString(NumberFormat)); // newValue = 9999

                yield return Wait;
            }
        }
        else
        {
            while (previousValue > newValue)
            {
                previousValue += stepAmount; 
                if (previousValue < newValue)
                {
                    previousValue = newValue;
                }

                Text.SetText(previousValue.ToString(NumberFormat)); // newValue = 9999

                yield return Wait;
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
