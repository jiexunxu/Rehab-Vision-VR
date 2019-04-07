using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Main : MonoBehaviour {
    public int RNGSeed;

    public bool hideLineOfSight;

    public float fixationDistance;
    public float fixationScale;
    public Color fixationColor;
    public bool hideFixationInTrial;

    public float innerRadius;
    public int numInnerDots;
    public float midRadius;
    public int numMidDots;
    public float outerRadius;
    public int numOuterDots;
    public float dotScale;
    public Color dotColor;

    public Color backgroundColor;

    public int fixationTime;
    public int stimulusFixationTime;
    public int stimulusDisplayMinTime;
    public int stimulusDisplayMaxTime;
    public int maxStimulusTime;
    public float fixationTolerance;

    public AudioClip trialStart;
    public AudioClip trialSuccess;
    public AudioClip trialFail;



    private int trialCounter = 0;
    private bool fixationStarted;
    private bool trialStarted;
    private Stopwatch trialTimer;
    private AudioSource sound;
    private int stimulusDisplayTime;

    void Start () {
        UnityEngine.Random.InitState(RNGSeed);
        fixationTimer = new Stopwatch();
        fixationStarted = true;
        trialStarted = false;
        trialTimer = new Stopwatch();
        sound = GameObject.Find("Main").GetComponent<AudioSource>();
        if(hideLineOfSight)
            GameObject.Find("LOSOrigin").SetActive(false);
        initDots();
	}

    
	void Update () {
        Quaternion sight = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        if (!hideLineOfSight)
            GameObject.Find("LOSOrigin").transform.rotation = sight;

        // Fixation 
        if (fixationStarted && trialCounter < trialsSuccessful.Length)
        {
            fixateAtPoint(sight, fixationDot.transform.position, fixationTime);
            // Fixation successful
            if (fixateAtPointSuccessful)
            {
                fixateAtPointSuccessful = false;
                trialStarted = true;
                fixationStarted = false;
                trialTimer.Start();
                stimulusDisplayTime = UnityEngine.Random.Range(stimulusDisplayMinTime, stimulusDisplayMaxTime);
                dots[trialCounter].SetActive(true);
                if (hideFixationInTrial)
                    fixationDot.SetActive(false);
                sound.clip = trialStart;
                sound.Play();
            }
        }

        // Trial
        if (trialStarted && trialCounter<trialsSuccessful.Length)
        {
            fixateAtPoint(sight, dots[trialCounter].transform.position, stimulusFixationTime);
            // Fixation successful
            if (fixateAtPointSuccessful)
            {
                trialsSuccessful[trialCounter] = true;
                finishTrial();
                sound.clip = trialSuccess;
                sound.Play();
            }
            // Time over
            if (trialTimer.ElapsedMilliseconds > maxStimulusTime)
            {
                finishTrial();
                sound.clip = trialFail;
                sound.Play();
            }
            // Stimulus display time over
            if (trialTimer.ElapsedMilliseconds > stimulusDisplayTime)
                dots[trialCounter].SetActive(false);

        }

        // Finish up
        if (trialCounter >= trialsSuccessful.Length)
        {
            for(int i = 0; i < dots.Length; i++)
            {
                if (trialsSuccessful[i])
                    dots[i].GetComponent<Renderer>().material.color = new Color(0, 1f, 0);
                else
                    dots[i].GetComponent<Renderer>().material.color = new Color(1f, 0, 0);
                dots[i].SetActive(true);
            }
        }
    }

    private GameObject lineOfSight;

    private void finishTrial()
    {
        fixateAtPointSuccessful = false;
        trialStarted = false;
        fixationStarted = true;
        trialTimer.Stop();
        trialTimer.Reset();
        dots[trialCounter].SetActive(false);
        fixationDot.SetActive(true);
        trialCounter++;
    }

    private GameObject[] dots;
    private GameObject fixationDot;
    private bool[] trialsSuccessful;
    private void initDots()
    {
        dots = new GameObject[numInnerDots+numMidDots+numOuterDots];
        trialsSuccessful = new bool[dots.Length];
        for (int i = 0; i < numInnerDots; i++)
            dots[i] = newDot(innerRadius, Mathf.PI * 2 / numInnerDots * i);
        for (int i = 0; i < numMidDots; i++)
            dots[i + numInnerDots] = newDot(midRadius, Mathf.PI * 2 / numMidDots * i);
        for (int i = 0; i < numOuterDots; i++)
            dots[i + numInnerDots + numMidDots] = newDot(outerRadius, Mathf.PI * 2 / numOuterDots * i);
        fixationDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fixationDot.transform.position = new Vector3(0, 0, fixationDistance);
        fixationDot.transform.localScale = new Vector3(fixationScale, fixationScale, fixationScale);
        fixationDot.GetComponent<Renderer>().material.color = fixationColor;
        shuffle(dots);
        for (int i = 0; i < dots.Length; i++)
            dots[i].SetActive(false);
        new WaitForSeconds(1);
    }

    private void shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int k = (int)(UnityEngine.Random.value * i);
            T value = array[k];
            array[k] = array[i];
            array[i] = value;
        }
    }

    private GameObject newDot(float r, float theta)
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.localScale = new Vector3(dotScale, dotScale, dotScale);
        dot.GetComponent<Renderer>().material.color = dotColor;
        dot.transform.position = new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), fixationDistance);
        return dot;
    }

    // If user started staring at point, and kept staring at it for enough duration (in milliseconds), then this function will set the variable fixateAtPointSuccessful to true.
    // This function must be called repeatedly in Update()
    private Stopwatch fixationTimer;
    private bool startedFixation=false;
    private bool fixateAtPointSuccessful = false;
    private void fixateAtPoint(Quaternion sight, Vector3 point, int duration)
    {
        if (staringAtPoint(sight, point) && !startedFixation)
        {
            startedFixation = true;
            fixationTimer.Start();
        }
        if(startedFixation && !(staringAtPoint(sight, point)))
        {
            startedFixation = false;
            fixationTimer.Stop();
            fixationTimer.Reset();
        }
        if(startedFixation && fixationTimer.ElapsedMilliseconds > duration)
        {
            fixateAtPointSuccessful = true;
            startedFixation = false;
            fixationTimer.Stop();
            fixationTimer.Reset();
        }
    }

    // Returns true if user line of sight aligns with the given point with an error less than fixationTolerance
    private bool staringAtPoint(Quaternion sight, Vector3 point)
    {
        return (Vector3.Normalize(point) - sight * new Vector3(0, 0, 1f)).magnitude < fixationTolerance;
    }
}
