using UnityEngine;
using UnityEngine.Events;

public class AiDebugger : MonoBehaviour
{
    public UnityEvent onRaceStart;

    [ContextMenu("StartRace")]
    void Start()
    {
        onRaceStart.Invoke();
    }
}
