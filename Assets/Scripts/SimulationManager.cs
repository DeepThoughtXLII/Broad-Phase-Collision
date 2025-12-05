using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{

    public static event Action OnSimulationStart;
    public static event Action OnSimulationStop;

    [SerializeField] Button StartSimulation;
    [SerializeField] TMP_InputField TimeInSeconds;

    [SerializeField] GameObject Simulation;
    [SerializeField] GameObject simSelectUI;

    private float simulationTime;
    private float currentSimulationTime;
    private bool simRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        StartSimulation.onClick.AddListener(SimulationStart);
    }


    private void SimulationStart()
    {
        simulationTime = int.Parse(TimeInSeconds.text);
        currentSimulationTime = 0;
        OnSimulationStart?.Invoke();
        simRunning = true;
        Simulation.SetActive(true);
        simSelectUI.SetActive(false);
    }

    private void Update()
    {
        if (!simRunning)
        {
            return;
        }
        currentSimulationTime += Time.deltaTime;
        if (currentSimulationTime >= simulationTime)
        {
            SimulationEnd();
        }
    }

    private void SimulationEnd()
    {
        OnSimulationStop?.Invoke();
        Simulation.SetActive(false);
    }
}
