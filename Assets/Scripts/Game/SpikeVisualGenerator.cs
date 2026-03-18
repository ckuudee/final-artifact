using UnityEngine;

/// <summary>
/// Procedurally replaces the default cube visual with a spiky log shape.
/// Attach alongside SpikeController; runs once in Awake.
/// </summary>
public class SpikeVisualGenerator : MonoBehaviour
{
    [SerializeField] private Color baseColor = new Color(0.45f, 0.25f, 0.1f);
    [SerializeField] private Color spikeColor = new Color(0.7f, 0.55f, 0.2f);
    [SerializeField] private int spikesPerFace = 3;
    [SerializeField] private float spikeHeight = 0.35f;
    [SerializeField] private float spikeBaseRadius = 0.12f;

    private void Awake()
    {
        Vector3 originalScale = transform.localScale;

        MeshRenderer existingRenderer = GetComponent<MeshRenderer>();
        MeshFilter existingFilter = GetComponent<MeshFilter>();

        if (existingRenderer != null)
        {
            existingRenderer.material = CreateMaterial(baseColor);
        }

        GenerateSpikes(originalScale);
    }

    private void GenerateSpikes(Vector3 parentScale)
    {
        float halfX = 0.5f;
        float halfY = 0.5f;
        float halfZ = 0.5f;

        AddSpikesOnFace(Vector3.up, Vector3.right, Vector3.forward, halfX, halfZ, halfY, parentScale);
        AddSpikesOnFace(Vector3.down, Vector3.right, Vector3.forward, halfX, halfZ, -halfY, parentScale);
        AddSpikesOnFace(Vector3.forward, Vector3.right, Vector3.up, halfX, halfY, halfZ, parentScale);
        AddSpikesOnFace(Vector3.back, Vector3.right, Vector3.up, halfX, halfY, -halfZ, parentScale);
        AddSpikesOnFace(Vector3.right, Vector3.forward, Vector3.up, halfZ, halfY, halfX, parentScale);
        AddSpikesOnFace(Vector3.left, Vector3.forward, Vector3.up, halfZ, halfY, -halfX, parentScale);
    }

    private void AddSpikesOnFace(Vector3 normal, Vector3 tangent, Vector3 bitangent,
        float tangentHalf, float bitangentHalf, float normalOffset, Vector3 parentScale)
    {
        int cols = Mathf.Max(1, spikesPerFace);
        int rows = Mathf.Max(1, Mathf.RoundToInt(spikesPerFace * (bitangentHalf / tangentHalf)));

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                float u = cols == 1 ? 0f : Mathf.Lerp(-tangentHalf * 0.75f, tangentHalf * 0.75f, (float)c / (cols - 1));
                float v = rows == 1 ? 0f : Mathf.Lerp(-bitangentHalf * 0.75f, bitangentHalf * 0.75f, (float)r / (rows - 1));

                u += Random.Range(-0.05f, 0.05f);
                v += Random.Range(-0.05f, 0.05f);

                Vector3 localBase = normal * normalOffset + tangent * u + bitangent * v;
                CreateSpikeObject(localBase, normal, parentScale);
            }
        }
    }

    private void CreateSpikeObject(Vector3 localPosition, Vector3 direction, Vector3 parentScale)
    {
        GameObject spike = new GameObject("Spike");
        spike.transform.SetParent(transform, false);
        spike.transform.localPosition = localPosition;
        spike.transform.localRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);

        float scaleX = parentScale.x > 0.001f ? 1f / parentScale.x : 1f;
        float scaleY = parentScale.y > 0.001f ? 1f / parentScale.y : 1f;
        float scaleZ = parentScale.z > 0.001f ? 1f / parentScale.z : 1f;
        float uniformInverse = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));

        float baseR = spikeBaseRadius * uniformInverse;
        float height = spikeHeight * uniformInverse;

        MeshFilter mf = spike.AddComponent<MeshFilter>();
        MeshRenderer mr = spike.AddComponent<MeshRenderer>();
        mf.mesh = CreateConeMesh(baseR, height, 6);
        mr.material = CreateMaterial(spikeColor);
    }

    private static Mesh CreateConeMesh(float radius, float height, int segments)
    {
        Mesh mesh = new Mesh();
        int vertCount = segments + 2;
        Vector3[] verts = new Vector3[vertCount];
        int[] tris = new int[segments * 6];

        verts[0] = Vector3.zero;
        verts[segments + 1] = new Vector3(0f, height, 0f);

        for (int i = 0; i < segments; i++)
        {
            float angle = (2f * Mathf.PI * i) / segments;
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            tris[i * 6] = 0;
            tris[i * 6 + 1] = next + 1;
            tris[i * 6 + 2] = i + 1;

            tris[i * 6 + 3] = segments + 1;
            tris[i * 6 + 4] = i + 1;
            tris[i * 6 + 5] = next + 1;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }
}
