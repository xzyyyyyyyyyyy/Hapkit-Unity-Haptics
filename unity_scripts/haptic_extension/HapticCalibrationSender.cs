using UnityEngine;

public class HapticCalibrationSender : MonoBehaviour
{
    public SerialHapkitReader reader;

    [ContextMenu("Calibrate Now")]
    public void Calibrate()
    {
        if (reader == null) return;
        reader.TrySend(new byte[] { (byte)'C' });
    }
}
