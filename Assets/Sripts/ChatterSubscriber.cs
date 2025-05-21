using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using UnityEngine.SceneManagement;
using TMPro;

public class ChatterSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public TMP_Text chatterText; // UI Text element to display messages

    void Start()
    {
        ros = ROSConnection.instance;
        ros.Subscribe<StringMsg>("/face_data", ReceiveMessage);
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
}