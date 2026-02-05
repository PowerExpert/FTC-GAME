using UnityEngine;
using UnityEngine.XR;

public class headMovement : MonoBehaviour
{
    public Vector3 headPosition;
    public Quaternion headRotation;

    void Update()
    {
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        if (headDevice.isValid)
        {
            if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            {
                headPosition = position;
            }
            if (headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                headRotation = rotation;
            }
        }
        transform.localPosition = headPosition;
        transform.localRotation = headRotation;
    }
}
