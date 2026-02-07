using UnityEngine;
using Unity.Netcode;

public class sdgfdsfsdf : MonoBehaviour
{
    public void Create()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
}
