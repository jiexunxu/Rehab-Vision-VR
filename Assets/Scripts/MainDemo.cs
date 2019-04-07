using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MainDemo : MonoBehaviour
{
    public Light dirLight;
    
    public int RNGSeed;

    public bool hideLineOfSight;

    public float fixationDistance;
    public float fixationScale;
    public Color fixationColor;
    public bool hideFixationInTrial;

    public Color backgroundColor;

    public AudioClip trialSuccess;


    private int trialCounter = 0;
    private bool fixationStarted;
    private bool trialStarted;
    private Stopwatch trialTimer;
    private AudioSource sound;
    private int stimulusDisplayTime;

    private GameObject sightLine;
    private int currentDot;

    void Start()
    {
        UnityEngine.Random.InitState(RNGSeed);
        sound = GameObject.Find("Sight").GetComponent<AudioSource>();
        sound.clip = trialSuccess;
        sightLine = GameObject.Find("Sight");
        GameObject.Find("BGM").GetComponent<AudioSource>().Play();
        initDots();
        currentDot = 0;
        dots[currentDot].SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentDot < dots.Length - 1)
        {
            float cl = 1f - currentDot * 0.06f;
            RenderSettings.ambientLight = new Color(cl, cl, cl);
            dirLight.intensity = 2f - currentDot * 0.6f;
            sound.Play();
            dots[currentDot].SetActive(false);
            currentDot++;
            dots[currentDot].SetActive(true);
            DynamicGI.UpdateEnvironment();
        }
    }

    void Update()
    {
        Quaternion sight = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        sightLine.transform.localRotation = sight;
        // Fixation 
       
    }

    private GameObject lineOfSight;

    private GameObject[] dots;
    private GameObject fixationDot;
    private bool[] trialsSuccessful;
    private void initDots()
    {
        dots = new GameObject[20];
        trialsSuccessful = new bool[dots.Length];
        for(int i = 0; i < dots.Length; i++)
        {
            dots[i] = newDot(20 + Random.Range(-5f, 5f), 2 + Random.Range(0, 5f), 20 + Random.Range(-5f, 5f));
        }       
        for (int i = 0; i < dots.Length; i++)
            dots[i].SetActive(false);
        new WaitForSeconds(1);
    }

    private GameObject newDot(float x, float y, float z)
    {
        float dotScale = 0.07f;
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dot.transform.localScale = new Vector3(dotScale, dotScale, dotScale);
        dot.GetComponent<Renderer>().material.color = Color.blue;
        dot.transform.position = new Vector3(x, y, z);
        dot.GetComponent<BoxCollider>().isTrigger = true;
        return dot;
    }
}
