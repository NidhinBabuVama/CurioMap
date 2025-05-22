using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.Tf2;
using System;
using System.Diagnostics;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;



public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject robot;
    [SerializeField] private GameObject mapPrefab;
   // private Renderer mapRenderer;
    public SpriteRenderer mapRendSprite;
    private Texture2D mapTexture;
    [SerializeField] TMP_Text mapStatusText;

    float mapResolution;
    float mapWidth;
    float mapHeight;
    float lastMapOriginX;
    float lastMapOriginY;

    private Process rosProcess;



    ROSConnection ros;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ros = ROSConnection.instance;
      
        ros.Subscribe<TFMessageMsg>("/tf", ReceiveTF);

        ros.Subscribe<OccupancyGridMsg>("map", ReceiveMap);



        // Initialize the mapPrefab and its renderer
        if (mapPrefab != null)
        {
            GameObject mapObject = Instantiate(mapPrefab);
            mapRendSprite = mapObject.GetComponent<SpriteRenderer>();
        }

    }


    #region Robot Position Update

    void ReceiveTF(TFMessageMsg tfMsg)
    {
      //  print("received TF");
        foreach (TransformStampedMsg transform in tfMsg.transforms)
        {
            string frameId = transform.child_frame_id;
            string childFrameId = transform.child_frame_id;
          //  Debug.Log($"TF: {frameId} -> {childFrameId}");
            //  if (frameId == "odom")
            {

                if ( childFrameId == "base_link")
                {
                    Vector3 Rposition = new Vector3(
                        (float)transform.transform.translation.x,
                        (float)transform.transform.translation.y,
                        (float)transform.transform.translation.z
                    );
                    robot.transform.position = Rposition;

                    //  print(robot.transform.position);

                    Quaternion Rrotation = new Quaternion(
                        (float)transform.transform.rotation.x,
                        (float)transform.transform.rotation.y,
                        (float)transform.transform.rotation.z,
                        (float)transform.transform.rotation.w
                    );
                    robot.transform.rotation = Rrotation;

                  //  Debug.Log($"TF: {frameId} -> {childFrameId}");
                  //  Debug.Log($"  Position: {Rposition}");
                  //  Debug.Log($"  Rotation: {Rrotation.eulerAngles}");
                }
            }
        }
    }
    #endregion


    #region Map Size

    void ReceiveMap(OccupancyGridMsg message)
    {
        // Store values for positioning
        mapResolution = (float)message.info.resolution;
        mapWidth = message.info.width;
        mapHeight = message.info.height;
        lastMapOriginX = (float)message.info.origin.position.x;
        lastMapOriginY = (float)message.info.origin.position.y;

        // Create and display map
        mapTexture = CreateTextureFromMap(message);
        UpdateMapTexture(mapTexture);

        mapStatusText.text = "Map received and displayed.";
    }


    Texture2D CreateTextureFromMap(OccupancyGridMsg mapMessage)
    {
        int width = (int)mapMessage.info.width;
        int height = (int)mapMessage.info.height;
        sbyte[] sbyteData = mapMessage.data;

        if (mapTexture == null || mapTexture.width != width || mapTexture.height != height)
        {
            mapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        Color[] colors = new Color[width * height];

        for (int i = 0; i < sbyteData.Length; i++)
        {
            int value = sbyteData[i];
            colors[i] = value == 0 ? Color.white :
                        value == 100 ? Color.black :
                        Color.gray;
        }

        mapTexture.SetPixels(colors);
        mapTexture.Apply();
        return mapTexture;
    }


    void UpdateMapTexture(Texture2D texture)
    {
        if (mapRendSprite != null)
        {
            float pixelsPerUnit = 1f / mapResolution;

            Sprite mapSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),  // center pivot
                pixelsPerUnit
            );

            mapRendSprite.sprite = mapSprite;

            // Calculate center position based on origin + half dimensions
            Vector3 worldPosition = new Vector3(
                lastMapOriginX + mapWidth * mapResolution / 2f,
                lastMapOriginY + mapHeight * mapResolution / 2f,
                0f
            );

            mapRendSprite.transform.position = worldPosition;
            mapRendSprite.transform.localScale = Vector3.one;
        }
    }


    #endregion

    #region Stop Backend Script
    public void StopAndBacktoMain()
    {
        StopROS2Process();
        SceneManager.LoadScene("FaceDectecting");

    }
    public void StopROS2Process()
    {
        //  string mapName = InputField.text;
        //  print(mapName + " Map Name");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            //Arguments = "-c \"pkill -f 'rtabmap'\"",
            Arguments = $"-c \"/home/tech/simulation/src/curio_one/sh_files/stop_slam_toolbox.sh\"",

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

    private void AppendToOutput(string message)
    {
        if (mapStatusText != null)
        {
            mapStatusText.text += message + "\n";
        }
        else
        {
            UnityEngine.Debug.Log(message);
        }
    }


    public void launchNav()
    {
        print("launched");
        string rosSetup = "source /opt/ros/humble/setup.bash";
        string launchnavigation = "ros2 launch curio_one navigation_launch.py use_sim_time:=true";


        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = $"-c \"{rosSetup}&&{launchnavigation}\"",
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
    #endregion


}
