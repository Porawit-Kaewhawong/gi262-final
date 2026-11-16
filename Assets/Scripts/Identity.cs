using UnityEngine;

/// <summary>
/// Root base class for all game entities (Characters and Interactables).
/// </summary>
public class Identity : MonoBehaviour
{
    [Header("Identity Info")]
    public string Name = "Entity";
    public int positionX;
    public int positionY;

    protected virtual void Awake()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name == "Entity")
        {
            Name = gameObject.name;
        }
        SyncGridPosition();
    }

    public void SyncGridPosition()
    {
        positionX = Mathf.RoundToInt(transform.position.x);
        positionY = Mathf.RoundToInt(transform.position.y);
    }

    public void PrintInfo()
    {
        SyncGridPosition();
        Debug.Log($"[Identity] Name: {Name} | Position: ({positionX}, {positionY})");
    }
}
