using UnityEngine;

public class SceneNodeData : ScriptableObject
{
    [SerializeField] private long currentId = 0;

    public long GetNextId()
    {
        return currentId++;
    }

    public long CurrentId { get => currentId; }
}
