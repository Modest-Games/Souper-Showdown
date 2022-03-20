using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;

public class GoogleyEyes_Effect : MonoBehaviour
{
    public Transform eye1;
    public Transform eye2;

    public float movementRange;
    public string objectTag;
    public float searchInterval;
    public float blinkInterval;
    public float blinkDuration;

    [ReadOnly] public List<GameObject> nearbyObjects;

    [ReadOnly] [SerializeField] private Transform closestObject;
    private Transform pupil1;
    private Transform pupil2;
    private Vector3 pupil1IdlePos;
    private Vector3 pupil2IdlePos;

    private void Awake()
    {
        pupil1 = eye1.Find("Pupil");
        pupil2 = eye2.Find("Pupil");

        pupil1IdlePos = pupil1.localPosition;
        pupil2IdlePos = pupil2.localPosition;

        // start looping coroutines
        StartCoroutine(GetClosestObject());
        StartCoroutine(EnableBlinking());
    }
    
    void Update()
    {
        LookAtClosestObject();
    }

    private void LookAtClosestObject()
    {
        // don't look if there's no closest object
        if (closestObject == null)
        {
            pupil1.localPosition = pupil1IdlePos;
            pupil2.localPosition = pupil2IdlePos;

            return;
        }

        Vector2 objPos = Camera.main.WorldToScreenPoint(closestObject.position);
        Vector2 eye1Pos = Camera.main.WorldToScreenPoint(eye1.transform.position);
        Vector2 eye2Pos = Camera.main.WorldToScreenPoint(eye2.transform.position);

        Vector2 eye1ToObj = Vector2.ClampMagnitude((objPos - eye1Pos).normalized, movementRange);
        Vector2 eye2ToObj = Vector2.ClampMagnitude((objPos - eye2Pos).normalized, movementRange);

        pupil1.localPosition = new Vector3(eye1ToObj.x, eye1ToObj.y, pupil1.transform.localPosition.z);
        pupil2.localPosition = new Vector3(eye2ToObj.x, eye2ToObj.y, pupil2.transform.localPosition.z);
    }
    
    private IEnumerator GetClosestObject()
    {
        while(true)
        {
            yield return new WaitForSeconds(searchInterval);

            // get nearby objects
            nearbyObjects = GameObject.FindGameObjectsWithTag(objectTag).ToList();

            if (nearbyObjects.Count > 0)
            {
                // sort the objects by their distance to the googley eye
                nearbyObjects = nearbyObjects.OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position)).ToList();

                closestObject = nearbyObjects[0].transform;
            } else
            {
                closestObject = null;
            }
        }
    }

    private IEnumerator EnableBlinking()
    {
        while(true)
        {
            yield return new WaitForSeconds(blinkInterval * Random.Range(0.3f, 1f));

            StartCoroutine(Blink());
        }
    }

    private IEnumerator Blink()
    {
        eye1.gameObject.SetActive(false);
        eye2.gameObject.SetActive(false);

        yield return new WaitForSeconds(blinkDuration);

        eye1.gameObject.SetActive(true);
        eye2.gameObject.SetActive(true);
    }
}
