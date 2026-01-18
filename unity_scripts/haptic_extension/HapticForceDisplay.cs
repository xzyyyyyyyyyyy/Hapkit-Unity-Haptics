using TMPro;
using UnityEngine;

public class HapticForceDisplay : MonoBehaviour
{
    public SerialHapkitReader reader;
    public TMP_Text label;
    public string prefix = "Force: ";
    public string unit = " N";
    public int decimals = 2;

    private void Update()
    {
        if (reader == null || label == null) return;
        label.text = prefix + reader.LastForce.ToString("F" + decimals) + unit;
    }
}
