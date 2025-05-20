using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.Tf2;
using System;
using System.Net.Mail;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject robot;
    [SerializeField] private GameObject map;

    ROSConnection ros;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ros = ROSConnection.instance;
      
        ros.Subscribe<TFMessageMsg>("/tf", ReceiveTF);

        ros.Subscribe<OccupancyGridMsg>("map", ReceiveMap);

       


    }


    #region Robot Position Update

    void ReceiveTF(TFMessageMsg tfMsg)
    {
        foreach (TransformStampedMsg transform in tfMsg.transforms)
        {
            string frameId = transform.child_frame_id;
            string childFrameId = transform.child_frame_id;
            

            if (childFrameId == "base_footprint")
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

                //Debug.Log($"TF: {frameId} -> {childFrameId}");
                //Debug.Log($"  Position: {Rposition}");
                //Debug.Log($"  Rotation: {Rrotation.eulerAngles}");
            }
        }
    }
    #endregion
     
    #region Map Size

    void ReceiveMap(OccupancyGridMsg msg)
    {
        int width = (int)msg.info.width;
        int height = (int)msg.info.height;
        int reselution = (int)msg.info.resolution;

        map.transform.localScale = new Vector2( width, height );
        print("ReceivedMap");


    }

    #endregion



}
