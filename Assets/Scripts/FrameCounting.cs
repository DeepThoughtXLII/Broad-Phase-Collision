using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class FrameCounting : MonoBehaviour
{
    [SerializeField] TMP_InputField TestName;
    string folderPath = "SimulationData/";
    private List<float> frameTime = new List<float>(50000);
    bool isCounting = false;
    SimData simData;

    // Start is called before the first frame update
    void Start()
    {
        SimulationManager.Instance.OnSimulationStart += StartCounting;
        SimulationManager.Instance.OnSimulationStop += EndCounting;
        frameTime.Clear();
    }

    private void OnDisable()
    {
        SimulationManager.Instance.OnSimulationStart -= StartCounting;
        SimulationManager.Instance.OnSimulationStop -= EndCounting;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCounting) { return;}
        frameTime.Add(Time.deltaTime);
    }

    private void StartCounting(SimData simData)
    {
        isCounting = true;
        this.simData = simData;
    }

    private void EndCounting()
    {
        isCounting = false;
        SaveDataInFile();
    }

    private void SaveDataInFile()
    {
        string fullFolderPath = Path.Combine(Application.persistentDataPath, folderPath);

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        string filepath = fullFolderPath + TestName.text + ".csv";

        using (StreamWriter writer = new StreamWriter(filepath))
        {
            writer.WriteLine(simData.ToString());
            writer.WriteLine("FPS: " + (frameTime.Count/simData.simulationDuration));
            List<int> checks = simData.simulation.collisionCheckAmounts;
            for (int i = 0; i < frameTime.Count; i++)
            {
                writer.WriteLine(frameTime[i] +";"+ checks[i]);
            }
            writer.WriteLine("");
            //writer.WriteLine("collisionChecks: ");
            //for (int i = 0; i < collisionChecks.Count; i++)
            //{
            //}
        }
        Debug.Log($"Saved {frameTime.Count} frame times to {filepath}");
    }

}
