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

    // Start is called before the first frame update
    void Start()
    {
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
        string fullFolderPath = Path.Combine(Application.persistentDataPath, folderPath);

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        string filepath = fullFolderPath + TestName.text + ".csv";

        using (StreamWriter writer = new StreamWriter(filepath))
        {
            for (int i = 0; i < frameTime.Count; i++)
            {
                writer.WriteLine(frameTime[i]);
            }
        }
        Debug.Log($"Saved {frameTime.Count} frame times to {filepath}");
    }

}
