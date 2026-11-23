using UnityEngine;

public class Identity : MonoBehaviour
{
    public string Name = "Entity";
    public int positionX;
    public int positionY;

    protected virtual void Awake()
    {
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
