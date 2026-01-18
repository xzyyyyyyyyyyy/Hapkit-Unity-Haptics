using UnityEngine;

public class HapkitWallVisualizer : MonoBehaviour
{
    [Header("Geometry (match Processing)")]
    public float dWallPos = 130f;
    public float zWallThick = 10f;
    public float circleRad = 20f;
    public Vector2[] circleCenter = new Vector2[]
    {
        new Vector2(-20, -20),
        new Vector2( 20,  20),
    };

    [Header("Rendering")]
    public float scale = 0.1f;
    public Color wallColor = new Color(0.2f, 0.8f, 1f, 0.25f);
    public Color holeColor = new Color(1f, 0.4f, 0.4f, 0.35f);
    public float wallSize = 320f; // 与 Processing cubeSize 接近

    private GameObject _wall;

    void Start()
    {
        CreateWall();
        CreateHoles();
    }

    private void CreateWall()
    {
        _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wall.name = "HapkitWall";
        _wall.transform.SetParent(transform, false);

        float zMid = dWallPos - zWallThick * 0.5f;
        _wall.transform.localPosition = new Vector3(0, 0, zMid) * scale;
        _wall.transform.localScale = new Vector3(wallSize, wallSize, zWallThick) * scale;

        var renderer = _wall.GetComponent<Renderer>();
        renderer.material = CreateMaterial(wallColor);
        Destroy(_wall.GetComponent<Collider>());
    }

    private void CreateHoles()
    {
        float holeDiameter = circleRad;
        for (int i = 0; i < circleCenter.Length; i++)
        {
            var hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hole.name = $"HapkitHole_{i}";
            hole.transform.SetParent(transform, false);

            float zMid = dWallPos - zWallThick * 0.5f;
            hole.transform.localPosition = new Vector3(circleCenter[i].x, circleCenter[i].y, zMid) * scale;
            hole.transform.localRotation = Quaternion.Euler(90, 0, 0);
            hole.transform.localScale = new Vector3(holeDiameter, zWallThick, holeDiameter) * scale;

            var renderer = hole.GetComponent<Renderer>();
            renderer.material = CreateMaterial(holeColor);
            Destroy(hole.GetComponent<Collider>());
        }
    }

    private Material CreateMaterial(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }
}
