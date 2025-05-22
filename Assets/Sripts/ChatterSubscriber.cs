using RosMessageTypes.Std;
using System.Diagnostics;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ChatterSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public TMP_Text chatterText; // UI Text element to display messages

    public TMP_Text Error;

    public TMP_Text outputText;

    private string launchFilePath = "/home/tech/simulation/src/curio_one/launch/rtabmap.launch.py";

    [SerializeField] TMP_InputField InputField;
    [SerializeField] GameObject mapNamePanel;


    void Start()
    {
        ros = ROSConnection.instance;
        ros.Subscribe<StringMsg>("/face_data", ReceiveMessage);

        ros = ROSConnection.GetOrCreateInstance();
      //  LogToConsole("ROSConnection initialized.");

        // Example: Subscribe to a topic
     //   ros.Subscribe<std_msgs.String>("chatter", OnROSMessageReceived);

    }

    void ReceiveMessage(StringMsg message)
    {
        chatterText.text = "face detected";

        if (message.data == "face detected")
        {
            // GoToInteractionPage();
            print("deteced");
        }
    }

    //public void GoToInteractionPage()
    //{
    //    SceneManager.LoadScene(2);
    //}
    //public void BactToHome()
    //{
    //    SceneManager.LoadScene(0);
    //}

    public void mapNamePanelEnable()
    {
        mapNamePanel.SetActive(true);
    }

    public void MapLoad()
    {
        mapNamePanel.SetActive(false);
        LaunchROS2File();
        SceneManager.LoadScene("Map");
    }
    void LaunchROS2File()
    {
        string mapName = InputField.text;
        print(mapName + " Map Name");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = $"-c \"source /opt/ros/humble/setup.bash && ros2 launch {launchFilePath}  use_sim_time:=true localization:=false database_path:=/home/tech/simulation/"+mapName+".db\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                AppendToOutput(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                AppendToOutput($"<color=red>{args.Data}</color>");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    void AppendToOutput(string message)
    {
        // Ensure UI update is on the main thread
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            outputText.text += message + "\n";
        });
    }
}