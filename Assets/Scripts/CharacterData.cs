using UnityEngine;

/// <summary>
/// ScriptableObject that holds data for one selectable character.
/// Create via: Right-click in Project > Create > FTC Game > Character Data
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "FTC Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName = "Robot";
    public Sprite previewSprite;              // Portrait shown in selection panel

    [Header("Prefab")]
    public GameObject playerPrefab;           // The actual player prefab to spawn

    [Header("Description")]
    [TextArea(2, 4)]
    public string description = "";
}
