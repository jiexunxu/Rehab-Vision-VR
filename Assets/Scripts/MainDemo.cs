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
    public AudioClip missionComplete;

    private int trialCounter = 0;
    private bool fixationStarted;
    private bool trialStarted;
    private Stopwatch trialTimer;
    private AudioSource sound;
    private int stimulusDisplayTime;

    private GameObject sightLine;
    private int currentDot;

    private float[] intensities = {1f, 0.6f, 0.3f, 0.18f, 0.08f, 0.03f};

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
        RenderSettings.skybox.SetFloat("_Exposure", 1f);
        DynamicGI.UpdateEnvironment();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CubeTracker>() != null)
        {

            if (currentDot == dots.Length - 1)
            {
                sound.clip = missionComplete;
                sound.Play();
            }
            else
            {
                sound.clip = trialSuccess;
                sound.Play();
            }
                dots[currentDot].SetActive(false);
            StartCoroutine(WaitThenIncrement(1f));

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
        dots = new GameObject[6];
        trialsSuccessful = new bool[dots.Length];
        for(int i = 0; i < dots.Length; i++)
        {
            dots[i] = newDot(20 + Random.Range(-3f, 3f), 2 + Random.Range(1f, 3f), 20 + Random.Range(-3f, 3f));
        }       
        for (int i = 0; i < dots.Length; i++)
            dots[i].SetActive(false);
        new WaitForSeconds(1f);
    }

    private GameObject newDot(float x, float y, float z)
    {
        float dotScale = 0.07f;
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dot.transform.localScale = new Vector3(dotScale, dotScale, dotScale);
        dot.GetComponent<Renderer>().material.color = Color.blue;
        dot.transform.position = new Vector3(x, y, z);
        dot.GetComponent<BoxCollider>().isTrigger = true;
        dot.AddComponent<CubeTracker>();
        return dot;
    }

    IEnumerator WaitThenIncrement(float second)
    {
        yield return new WaitForSeconds(second);        
        currentDot++;
        if (currentDot < dots.Length)
        {

            float cl = intensities[currentDot];
            RenderSettings.ambientLight = new Color(cl, cl, cl);
            RenderSettings.skybox.SetFloat("_Exposure", intensities[currentDot]);
            RenderSettings.fogDensity = intensities[currentDot] * 0.05f;
            dirLight.intensity = intensities[currentDot] * 2f;
            DynamicGI.UpdateEnvironment();
            dots[currentDot].SetActive(true);
        }
    }
}
