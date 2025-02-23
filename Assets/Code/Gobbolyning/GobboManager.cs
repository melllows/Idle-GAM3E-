using System.Collections.Generic;
using UnityEngine;

public class GobboManager : MonoBehaviour
{
    public static GobboManager Instance;
    public List<Gobblyns> gobbos = new();

    void Awake() => Instance = this;

    public void RegisterGobbo(Gobblyns gobbo)
    {
        Debugger.Instance.Log("Registering gobbo");
        if (!gobbos.Contains(gobbo)) gobbos.Add(gobbo);
    }

    public void RemoveGobbo(Gobblyns gobbo)
    {
        gobbos.Remove(gobbo);
    }

    public Gobblyns GetClosestIdleGobbo(Vector3 taskPosition)
    {
        Gobblyns closestGobbo = null;
        float closestDistance = float.MaxValue;

        foreach (var gobbo in gobbos)
        {
            if (!gobbo.isBusy)
            {
                float distance = Vector3.Distance(gobbo.transform.position, taskPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGobbo = gobbo;
                }
            }
        }

        return closestGobbo;
    }
}
