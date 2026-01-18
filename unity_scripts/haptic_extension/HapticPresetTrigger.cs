using UnityEngine;

public class HapticPresetTrigger : MonoBehaviour
{
    [Header("Target")]
    public HapticFeedbackSender sender;

    [Header("Preset Params")]
    public bool useConstraints = true;
    public float wallPos = 130f;
    public float wallThick = 10f;
    public float holeRadius = 20f;
    public float stiffness = 150f;
    public float damping = 0f;
    public float maxForce = 4.5f;

    [Header("Behavior")]
    public bool applyOnStart = false;
    public bool applyOnEnter = true;
    public bool applyOnExit = false;

    private void Start()
    {
        if (applyOnStart)
        {
            Apply();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (applyOnEnter)
        {
            Apply();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (applyOnExit)
        {
            Apply();
        }
    }

    [ContextMenu("Apply Preset")]
    public void Apply()
    {
        if (sender == null) return;
        sender.useConstraints = useConstraints;
        sender.wallPos = wallPos;
        sender.wallThick = wallThick;
        sender.holeRadius = holeRadius;
        sender.stiffness = stiffness;
        sender.damping = damping;
        sender.maxForce = maxForce;
        sender.SendParams();
    }
}
