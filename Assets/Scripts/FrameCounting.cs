using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrameCounting : MonoBehaviour
{
    [SerializeField] TMP_InputField TestName;
    [SerializeField] string filepath = "/SimulationData/";
    private List<float> frameTime = new List<float>();
    private float timeSinceStart = 0;
    bool isCounting = false;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceStart = 0;
        filepath = Application.dataPath + filepath + TestName.text +".csv";
        SimulationManager.OnSimulationStart += StartCounting;
        SimulationManager.OnSimulationStop += EndCounting;
    }

    private void OnDisable()
    {
        SimulationManager.OnSimulationStart -= StartCounting;
        SimulationManager.OnSimulationStop -= EndCounting;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCounting) { return;}
        frameTime.Add(Time.deltaTime);
    }

    private void StartCounting()
    {
        isCounting = true;
    }

    private void EndCounting()
    {
        isCounting = false;
        SaveDataInFile();
    }

    private void SaveDataInFile()
    {

    }

}
