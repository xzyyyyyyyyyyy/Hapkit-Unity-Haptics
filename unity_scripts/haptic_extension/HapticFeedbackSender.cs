using System;
using UnityEngine;

public class HapticFeedbackSender : MonoBehaviour
{
    [Header("Serial")]
    public SerialHapkitReader reader;

    [Header("Feedback Params")]
    public bool useConstraints = true;
    public float wallPos = 130f;
    public float wallThick = 10f;
    public float holeRadius = 20f;
    public float stiffness = 150f;
    public float damping = 0f;
    public float maxForce = 4.5f;

    [Header("Send")]
    public bool sendOnStart = true;
    public bool sendContinuously = true;
    public float sendInterval = 0.2f;

    private float _lastSendTime;

    private const byte Header1 = 0xAA;
    private const byte Header2 = 0x55;

    private void Start()
    {
        if (sendOnStart)
        {
            SendParams();
            _lastSendTime = Time.time;
        }
    }

    private void Update()
    {
        if (!sendContinuously) return;
        if (Time.time - _lastSendTime >= sendInterval)
        {
            SendParams();
            _lastSendTime = Time.time;
        }
    }

    [ContextMenu("Send Params Now")]
    public void SendParams()
    {
        if (reader == null) return;

        byte flags = (byte)(useConstraints ? 0x01 : 0x00);

        byte[] payload = new byte[1 + 6 * 4];
        payload[0] = flags;

        WriteFloat(payload, 1, wallPos);
        WriteFloat(payload, 5, wallThick);
        WriteFloat(payload, 9, holeRadius);
        WriteFloat(payload, 13, stiffness);
        WriteFloat(payload, 17, damping);
        WriteFloat(payload, 21, maxForce);

        byte length = (byte)payload.Length;
        byte[] packet = new byte[3 + payload.Length];
        packet[0] = Header1;
        packet[1] = Header2;
        packet[2] = length;
        Buffer.BlockCopy(payload, 0, packet, 3, payload.Length);

        reader.TrySend(packet);
    }

    private void WriteFloat(byte[] buffer, int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Buffer.BlockCopy(bytes, 0, buffer, offset, 4);
    }
}
