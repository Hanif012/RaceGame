using UnityEngine;

public class SpringBone2D : MonoBehaviour
{
    public Transform child;             // The next bone (must be set manually)
    public float stiffness = 0.2f;      // How much the bone wants to return to its rest state
    public float drag = 0.1f;           // Damping
    public Vector2 boneAxis = new Vector2(0, 1); // Direction the bone extends

    private Vector3 prevTipPos;
    private Vector3 currTipPos;
    private Quaternion initialLocalRotation;

    void Start()
    {
        initialLocalRotation = transform.localRotation;

        Vector3 worldTip = transform.TransformPoint(boneAxis);
        prevTipPos = worldTip;
        currTipPos = worldTip;
    }

    void LateUpdate()
    {
        transform.localRotation = initialLocalRotation;

        Vector3 worldTip = transform.TransformPoint(boneAxis);
        Vector3 velocity = (currTipPos - prevTipPos) * (1f - drag);
        Vector3 force = (worldTip - currTipPos) * stiffness;

        prevTipPos = currTipPos;
        currTipPos += velocity + force;

        Vector3 direction = currTipPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

