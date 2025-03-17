using UnityEngine;

public class GameTask
{
    public Vector3 Position { get; private set; }
    public float TaskTime { get; private set; }
    public Crops targetCrop;

    private System.Action<Gobblyns> action;

    public GameTask(Vector3 position, float taskTime, System.Action<Gobblyns> action)
    {
        Position = position;
        TaskTime = taskTime;
        this.action = action;
    }

    public void Execute(Gobblyns gobbo)
    {
        action(gobbo);
    }
}
