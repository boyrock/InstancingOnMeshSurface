using UnityEngine;
using System.Collections;
using System.Linq;

public class RandomOnMeshSurface : MonoBehaviour
{
    Mesh _mesh;
    float[] _triangleSizes;

    [SerializeField]
    public GameObject _instancingObjectPrefab;
    [SerializeField]
    int _instancingMaxCount;

    // Use this for initialization
    void Start()
    {

        MeshFilter mf = this.GetComponent<MeshFilter>();
        if (mf != null)
        {
            _mesh = mf.mesh;
        }
        else
        {
            SkinnedMeshRenderer smr = this.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                _mesh = smr.sharedMesh;
        }

        int triangle_count = _mesh.triangles.Length / 3;

        int pointCount = 0;

        _triangleSizes = new float[triangle_count];
        for (int i = 0; i < triangle_count; i++)
        {
            _triangleSizes[i] = GetTriangleSize(i);
        }

        var minTriangleSize = _triangleSizes.Min();
        var maxTriangleSize = _triangleSizes.Max();

        for (int i = 0; i < triangle_count; i++)
        {
            if (_instancingMaxCount > 0 && pointCount >= _instancingMaxCount)
                break;

            var size = _triangleSizes[i];
            var t = Clamp01(minTriangleSize, maxTriangleSize, size);
            int roopCount = (int)Mathf.Lerp(20, 50, t);

            for (int j = 0; j < roopCount; j++)
            {
                if (_instancingMaxCount > 0 && pointCount >= _instancingMaxCount)
                    break;

                var position = GetPointOnMesh(i);

                var flower = Instantiate<GameObject>(_instancingObjectPrefab);
                flower.transform.SetParent(this.transform);
                flower.transform.position = position.point;
                flower.transform.rotation = Quaternion.LookRotation(position.normal);

                var normalize = position.normal;
                flower.transform.Rotate(normalize + Vector3.left * 270);
                flower.transform.Rotate(transform.rotation.eulerAngles, Space.World);

                //Debug.DrawRay(position.point, flower.transform.rotation.eulerAngles.normalized, Color.red, 1000);

                pointCount++;
            }
        }
        int index = Random.Range(0, (_mesh.triangles.Length / 3));
    }

    float Clamp01(float min, float max, float v)
    {
        return (v - min) / (max - min);
    }

    float GetTriangleSize(int index)
    {
        float size = 0;

        Vector3 p1 = _mesh.vertices[_mesh.triangles[(index * 3) + 0]];
        Vector3 p2 = _mesh.vertices[_mesh.triangles[(index * 3) + 1]];
        Vector3 p3 = _mesh.vertices[_mesh.triangles[(index * 3) + 2]];

        var lowerBase = p2 - p1;
        var middle_position = p1 + lowerBase.normalized * (lowerBase.sqrMagnitude / 2f);

        var height = p3 - middle_position;

        size = (lowerBase.sqrMagnitude * height.sqrMagnitude) / 2f;

        return size;
    }

    RaycastHit GetPointOnMesh(int index)
    {
        RaycastHit hit = new RaycastHit();

        Vector3 p1 = _mesh.vertices[_mesh.triangles[(index * 3) + 0]];
        Vector3 p2 = _mesh.vertices[_mesh.triangles[(index * 3) + 1]];
        Vector3 p3 = _mesh.vertices[_mesh.triangles[(index * 3) + 2]];

        var m = GetRandomPositionBetween(p1, p2);
        var m1 = GetRandomPositionBetween(m, p3);

        hit.point = transform.TransformPoint(m1);
        hit.normal = GetNormal(p1, p2, p3);

        return hit;
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

    Vector3 GetRandomPositionBetween(Vector3 from, Vector3 to)
    {
        Vector3 position;
        var direction = from - to;
        position = to + direction.normalized * Mathf.Lerp(0, direction.magnitude, Random.value);
        return position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
