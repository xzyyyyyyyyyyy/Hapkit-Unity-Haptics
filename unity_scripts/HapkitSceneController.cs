using UnityEngine;

public class HapkitSceneController : MonoBehaviour
{
    public enum AxisSource { X, Y, Z, NegX, NegY, NegZ }

    [Header("Refs")]
    public SerialHapkitReader reader;
    public Transform ball; // 球体对象

    [Header("Geometry (match Processing)")]
    public float dWallPos = 130f;
    public float zWallThick = 10f;
    public float circleRad = 20f;
    public Vector2[] circleCenter = new Vector2[]
    {
        new Vector2(-20, -20),
        new Vector2( 20,  20),
    };

    [Header("Scaling")]
    public float scale = 0.1f; // Processing 中单位是 mm，这里可按需要缩放
    public Vector3 axisScale = Vector3.one;
    public float maxAbsPosition = 2000f; // 防止异常数据把物体拉飞（单位：原始坐标）

    [Header("Axis Mapping")]
    public AxisSource unityX = AxisSource.X;
    public AxisSource unityY = AxisSource.Z; // Unity 默认 Y 为上
    public AxisSource unityZ = AxisSource.Y;

    [Header("Constraints")]
    public bool useConstraints = true;

    private void Update()
    {
        if (reader == null || ball == null) return;
        if (!reader.HasValidData) return;
        if (!float.IsFinite(scale) || scale == 0f) return;

        float x = reader.rawX / 10f;
        float y = reader.rawY / 10f;
        float z = reader.rawZ / 10f;

        if (reader.signX == 0) x = -x;
        if (reader.signY == 0) y = -y;

        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(z)) return;
        if (Mathf.Abs(x) > maxAbsPosition || Mathf.Abs(y) > maxAbsPosition || Mathf.Abs(z) > maxAbsPosition) return;

        if (useConstraints)
        {
            bool inHole = IsInsideHole(x, y);

            if (!inHole)
            {
                if (z <= dWallPos + circleRad / 2f && z >= dWallPos + zWallThick - circleRad / 2f)
                {
                    z = dWallPos + circleRad / 2f;
                }
                else if (z >= dWallPos - zWallThick - circleRad / 2f && z <= dWallPos - circleRad / 2f)
                {
                    z = dWallPos - circleRad / 2f;
                }
            }
        }

        Vector3 mapped = new Vector3(
            GetAxisValue(unityX, x, y, z),
            GetAxisValue(unityY, x, y, z),
            GetAxisValue(unityZ, x, y, z)
        );

        ball.localPosition = Vector3.Scale(mapped * scale, axisScale);
    }

    private float GetAxisValue(AxisSource source, float x, float y, float z)
    {
        switch (source)
        {
            case AxisSource.X: return x;
            case AxisSource.Y: return y;
            case AxisSource.Z: return z;
            case AxisSource.NegX: return -x;
            case AxisSource.NegY: return -y;
            case AxisSource.NegZ: return -z;
            default: return 0f;
        }
    }

    private bool IsInsideHole(float x, float y)
    {
        float r = circleRad / 2f;
        float r2 = r * r;

        foreach (var c in circleCenter)
        {
            float dx = x - c.x;
            float dy = y - c.y;
            if (dx * dx + dy * dy <= r2)
                return true;
        }
        return false;
    }
}
