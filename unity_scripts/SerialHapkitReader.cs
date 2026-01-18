using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialHapkitReader : MonoBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM8";   // 运行时可动态修改
    [SerializeField] private int baudRate = 115200;
    [SerializeField] private int readTimeoutMs = 50;
    [SerializeField] private bool sendRequest = true;
    [SerializeField] private int requestIntervalMs = 20;
    [SerializeField] private bool acceptRawFrames = true;

    public bool IsOpen => _port != null && _port.IsOpen;

    // 最新原始数据（单位：0.1）
    public int rawX;
    public int rawY;
    public int rawZ;
    public int signX; // 1 表示正，0 表示负
    public int signY;
    public bool HasValidData { get; private set; }
    public float LastForce { get; private set; }

    private SerialPort _port;
    private Thread _readThread;
    private volatile bool _running;
    private readonly byte[] _buffer = new byte[256];
    private long _lastRequestTicks;
    private readonly object _portLock = new object();

    private const byte POS_H1 = 0xFE;
    private const byte POS_H2 = 0xEF;
    private const byte FORCE_H1 = 0xCC;
    private const byte FORCE_H2 = 0x33;

    private byte _pktState = 0;
    private byte _pktLen = 0;
    private byte _pktType = 0;
    private byte _pktIndex = 0;
    private readonly byte[] _pktBuf = new byte[32];
    private int _rawIndex = 0;
    private readonly byte[] _rawBuf = new byte[14];

    public event Action OnDataUpdated;

    void OnDisable()
    {
        ClosePort();
    }

    void OnDestroy()
    {
        ClosePort();
    }

    void OnApplicationQuit()
    {
        ClosePort();
    }

    public void SetPort(string newPort)
    {
        portName = newPort;
        if (string.IsNullOrWhiteSpace(portName))
        {
            ClosePort();
            return;
        }
        if (IsOpen)
        {
            ClosePort();
            OpenPort();
        }
        else
        {
            OpenPort();
        }
    }

    public void OpenPort()
    {
        if (IsOpen) return;
        if (string.IsNullOrWhiteSpace(portName)) return;

        if (_port != null)
        {
            try
            {
                if (_port.IsOpen) _port.Close();
            }
            catch { }
            _port.Dispose();
            _port = null;
        }

        _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = readTimeoutMs
        };

        try
        {
            _port.Open();
            _running = true;
            _readThread = new Thread(ReadLoop)
            {
                IsBackground = true
            };
            _readThread.Start();

            if (sendRequest)
            {
                TrySend(new byte[] { (byte)'A' });
                _lastRequestTicks = DateTime.UtcNow.Ticks;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Serial open failed: {e.Message}");
        }
    }

    public void ClosePort()
    {
        _running = false;
        try
        {
            _readThread?.Join(200);
        }
        catch { }

        if (_port != null)
        {
            if (_port.IsOpen) _port.Close();
            _port.Dispose();
            _port = null;
        }
    }

    private void ReadLoop()
    {
        while (_running)
        {
            try
            {
                if (_port == null || !_port.IsOpen)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (sendRequest)
                {
                    long nowTicks = DateTime.UtcNow.Ticks;
                    long elapsedMs = (nowTicks - _lastRequestTicks) / TimeSpan.TicksPerMillisecond;
                    if (elapsedMs >= requestIntervalMs && _port.BytesToRead < 14)
                    {
                        TrySend(new byte[] { (byte)'A' });
                        _lastRequestTicks = nowTicks;
                    }
                }

                int available = _port.BytesToRead;
                if (available <= 0)
                {
                    Thread.Sleep(2);
                    continue;
                }

                int toRead = Math.Min(available, _buffer.Length);
                int read = _port.Read(_buffer, 0, toRead);
                for (int i = 0; i < read; i++)
                {
                    ParseByte(_buffer[i]);
                }
            }
            catch (TimeoutException) { }
            catch (Exception)
            {
                Thread.Sleep(10);
            }
        }
    }

    public bool TrySend(byte[] data)
    {
        if (data == null || data.Length == 0) return false;
        if (!IsOpen) return false;
        lock (_portLock)
        {
            if (_port == null || !_port.IsOpen) return false;
            try
            {
                _port.Write(data, 0, data.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private void ParseByte(byte b)
    {
        if (_pktState == 0)
        {
            if (b == POS_H1) { _pktState = 1; _pktType = 1; return; }
            if (b == FORCE_H1) { _pktState = 1; _pktType = 2; return; }

            if (acceptRawFrames)
            {
                _rawBuf[_rawIndex++] = b;
                if (_rawIndex >= 14)
                {
                    ApplyPositionRaw(_rawBuf);
                    _rawIndex = 0;
                }
            }
            return;
        }

        if (_pktState == 1)
        {
            if ((_pktType == 1 && b == POS_H2) || (_pktType == 2 && b == FORCE_H2))
            {
                _pktState = 2;
                return;
            }
            _pktState = 0;
            _pktType = 0;
            return;
        }

        if (_pktState == 2)
        {
            _pktLen = b;
            if (_pktLen == 0 || _pktLen > _pktBuf.Length)
            {
                _pktState = 0;
                _pktType = 0;
                return;
            }
            _pktIndex = 0;
            _pktState = 3;
            return;
        }

        if (_pktState == 3)
        {
            _pktBuf[_pktIndex++] = b;
            if (_pktIndex >= _pktLen)
            {
                if (_pktType == 1 && _pktLen == 14)
                {
                    ApplyPositionRaw(_pktBuf);
                }
                else if (_pktType == 2 && _pktLen == 4)
                {
                    LastForce = BitConverter.ToSingle(_pktBuf, 0);
                }

                _pktState = 0;
                _pktType = 0;
            }
        }
    }

    private void ApplyPositionRaw(byte[] buf)
    {
        rawX = BitConverter.ToInt32(buf, 0);
        rawY = BitConverter.ToInt32(buf, 4);
        rawZ = BitConverter.ToInt32(buf, 8);
        signX = buf[12];
        signY = buf[13];

        HasValidData = true;
        OnDataUpdated?.Invoke();
    }
}
