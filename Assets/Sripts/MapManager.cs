using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.Tf2;
using System;
using System.Net.Mail;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject robot;
    [SerializeField] private GameObject mapPrefab;
   // private Renderer mapRenderer;
    public SpriteRenderer mapRendSprite;
    private Texture2D mapTexture;
    [SerializeField] TMP_Text mapStatusText;

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
        foreach (TransformStampedMsg transform in tfMsg.transforms)
        {
            string frameId = transform.child_frame_id;
            string childFrameId = transform.child_frame_id;

        //  if (frameId == "odom")
            {

                if ( childFrameId == "base_footprint")
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

                    Debug.Log($"TF: {frameId} -> {childFrameId}");
                    Debug.Log($"  Position: {Rposition}");
                  //  Debug.Log($"  Rotation: {Rrotation.eulerAngles}");
                }
            }
        }
    }
    #endregion

    #region Map Size

    void ReceiveMap(OccupancyGridMsg message)
    {
        // Convert OccupancyGrid message to a Texture2D
        mapTexture = CreateTextureFromMap(message);
        // Update the existing map texture
        UpdateMapTexture(mapTexture);
        mapStatusText.text = "Map received and displayed.";
    }

    Texture2D CreateTextureFromMap(OccupancyGridMsg mapMessage)
    {
        int width = (int)mapMessage.info.width;
        int height = (int)mapMessage.info.height;

        sbyte[] sbyteData = mapMessage.data;
        byte[] data = Array.ConvertAll(sbyteData, b => (byte)b);

        // Initialize or update the texture
        if (mapTexture == null || mapTexture.width != width || mapTexture.height != height)
        {
            mapTexture = new Texture2D(width, height);
        }

        Color[] colors = new Color[width * height];

        for (int i = 0; i < colors.Length; i++)
        {
            byte value = data[i];
            colors[i] = value == 0 ? Color.white : (value == 100 ? Color.black : Color.gray);
        }

        mapTexture.SetPixels(colors);
        mapTexture.Apply();

        return mapTexture;
    }

    void UpdateMapTexture(Texture2D texture)
    {
        if (mapRendSprite != null)
        {
            Sprite mapSprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), // pivot at center
                100f); // pixels per unit, adjust as needed

            mapRendSprite.sprite = mapSprite;
        }
    }



    #endregion



}
