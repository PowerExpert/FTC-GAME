using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class AutoJoinMenuSetup : MonoBehaviour
{
    public List<CharacterData> characters;
    public PlayerInputManager inputManager;
    public timer timerScript;

    public void Start()
    {
        GetComponent<JoinScreenSetup>().characters = characters;
        GetComponent<JoinScreenSetup>().inputManager = inputManager;
        GetComponent<JoinScreenSetup>().timerScript = timerScript;
        GetComponent<JoinScreenSetup>().RunSetup();
    }
}