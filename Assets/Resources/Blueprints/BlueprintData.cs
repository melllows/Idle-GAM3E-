using UnityEngine;

[CreateAssetMenu(fileName = "NewBlueprint", menuName = "Blueprint System/Blueprint Data")]
public class BlueprintData : ScriptableObject
{
    public string blueprintName;
    public Sprite blueprintSprite;
    public Vector2Int size = Vector2Int.one;
    public GameObject finalPrefab;

    [Header("Pathfinding")]
    public bool isWalkable = true; 
}
