using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBar : MonoBehaviour
{

    //Bar height vars
    public float maxHeight = 523.0f;
    public float meterHeight = 0.0f;
    public float barHeight;
    public AnimationCurve easeOut;
    public AnimationCurve particleDecay;
    public AnimationCurve burstParticleDecay;

    //Bar Score Animation Var
    public GameObject scoreLabel;
    RectTransform rt;

    //Particle Variables
    public GameObject ParticleEdge;
    public GameObject ParticleEdgeSystem;
    public GameObject BurstParticleEdgeSystem;

    //Icon variables
    public GameObject iconImage;

    float t = 0;

    public int score;
    public bool animationStarted = false;

    public void setHeight(float bH)
    {
        barHeight = bH;
    }

    public void setUpBar(int s, float bH, Material spoilerType, Sprite icon, Color c)
    {
        score = s;
        barHeight = bH;
        //Sets particles renderers to passed material 
        ParticleEdgeSystem.GetComponent<ParticleSystemRenderer>().material = spoilerType;
        BurstParticleEdgeSystem.GetComponent<ParticleSystemRenderer>().material = spoilerType;
        //Sets sprite to passed image
        iconImage.GetComponent<Image>().sprite = icon;
        //Set bar to desried color
        GetComponent<Image>().color = c;
    }

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, maxHeight * meterHeight);
        
    }



    void Animate(float seconds)
    {
        //Grow bar w ease out
        t += Time.deltaTime / seconds;
        float height = easeOut.Evaluate(t);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, maxHeight * height * barHeight);

        //Update particle position and rate 
        ParticleEdge.transform.position = new Vector3(transform.position.x, transform.position.y + (rt.sizeDelta.y * 1.5f * height), transform.position.z);
        ParticleSystem.EmissionModule emissionModule;
        emissionModule = ParticleEdgeSystem.GetComponent<ParticleSystem>().emission;
        float particleNum = particleDecay.Evaluate(t);
        emissionModule.rateOverTime = particleNum;

        //Update burst particle position and rate
        ParticleEdge.transform.position = new Vector3(transform.position.x, transform.position.y + (rt.sizeDelta.y * 1.5f * height), transform.position.z);
        ParticleSystem.EmissionModule emissionModule2;
        emissionModule2 = BurstParticleEdgeSystem.GetComponent<ParticleSystem>().emission;
        particleNum = burstParticleDecay.Evaluate(t);
        emissionModule2.rateOverTime = particleNum;
    }


    public void startAnimating()
    {
        animationStarted = true;

        scoreLabel.GetComponent<NumberCounter>().Value = score;
        scoreLabel.GetComponent<NumberCounter>().Text.SetText(score.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        //Animate(3.0f);
        if (animationStarted)
        {
            Animate(3.0f);
        }
    }
}
