using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;
    public event Action<SimData> OnSimulationStart;
    public event Action OnSimulationStop;

    [SerializeField] private CollisionStrategy[] collisionStrategies = new CollisionStrategy[3];


    [SerializeField] TMP_Dropdown strategyDropdown;
    [SerializeField] Button StartSimulation;
    [SerializeField] TMP_InputField TimeInSeconds;
    [SerializeField] TMP_InputField ObjectCount;
    [SerializeField] TMP_InputField RandomSeedInput;

    [SerializeField] GameObject Simulation;
    [SerializeField] GameObject simSelectUI;
    private SpawnObjects objSpawner;
    private int seed = 12345678;

    Camera cam;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartSimulation.onClick.AddListener(SimulationStart);
        objSpawner = Simulation.GetComponent<SpawnObjects>();
        RandomSeedInput.text = seed.ToString();
        cam = Camera.main;
    }


    private void SimulationStart()
    {
        //parse time and object count
        int objAmount = int.Parse(ObjectCount.text);
        int simulationTime = int.Parse(TimeInSeconds.text);

        //initalize random with seed
        if (int.TryParse(RandomSeedInput.text, out int seed))
        {
            this.seed = seed;
        }
        UnityEngine.Random.InitState(this.seed);

        //apply settings
        objSpawner.objectAmount = objAmount;
        objSpawner.myCollisionStrategy = collisionStrategies[strategyDropdown.value];
        cam.orthographicSize = Mathf.Sqrt(objAmount / 4f);

        //start simulation
        OnSimulationStart?.Invoke(new SimData(objAmount, simulationTime, objSpawner));
        simSelectUI.SetActive(false);
        StartCoroutine(SimulationTimer(simulationTime));
    }


    private IEnumerator SimulationTimer(float simTime)
    {
        for(int i = 0; i < 5; i++)
        {
            Debug.LogError(GetRandom(0, 10));
        }
        yield return new WaitForSecondsRealtime(simTime);
        SimulationEnd();
    }


    private void SimulationEnd()
    {
        OnSimulationStop?.Invoke();
        Simulation.SetActive(false);
    }

    public float GetRandom(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
}


public struct SimData
{
    public int objAmount;
    public int simulationDuration;
    public SpawnObjects simulation;

    public SimData(int objAmount, int simulationDuration, SpawnObjects simulation)
    {
        this.objAmount = objAmount;
        this.simulationDuration = simulationDuration;
        this.simulation = simulation;
    }

    public override string ToString()
    {
        return $"objects in simulation: {objAmount} for {simulationDuration} seconds";
    }
}