using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class RandomOnMeshSurface : MonoBehaviour
{
    Mesh _mesh;
    float[] _triangleSizes;

    [SerializeField]
    public Mesh _instancingMesh;

    [SerializeField]
    int _instancingMaxCount;

    List<TransformData> _positions;

    [SerializeField]
    Material _mat;

    Quaternion _initRotation;
    Vector3 _initPosition;

    Vector3[] _initVertices;


    // Use this for initialization
    void Start()
    {
        _initRotation = this.transform.rotation;
        _initPosition = this.transform.position;


        _positions = new List<TransformData>();

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

        _initVertices = _mesh.vertices;

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

            float t = 1;
            if (minTriangleSize != maxTriangleSize)
                t = Normalized(minTriangleSize, maxTriangleSize, size);

            int roopCount = (int)Mathf.Lerp(15, 15, t);

            for (int j = 0; j < roopCount; j++)
            {
                if (_instancingMaxCount > 0 && pointCount >= _instancingMaxCount)
                    break;


                TransformData data = new TransformData();

                var position = GetPointOnMesh(i, data);

                data.trianleIndex = i;
                data.position = position.point;
                data.normalized = position.normal;

                var rotation = Quaternion.LookRotation(position.normal);
                rotation = rotation * Quaternion.Euler(r);

                Quaternion angle = Quaternion.AngleAxis(Random.Range(0,360f), Vector3.up);
                rotation = rotation * angle;
                data.rotation = rotation;

                data.speed = Random.Range(0.7f, 1f);

                pointCount++;

                _positions.Add(data);
            }
        }
        int index = Random.Range(0, (_mesh.triangles.Length / 3));
    }

    float Normalized(float min, float max, float v)
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

    RaycastHit GetPointOnMesh(int index, TransformData data)
    {
        RaycastHit hit = new RaycastHit();

        Vector3 p1 = _mesh.vertices[_mesh.triangles[(index * 3) + 0]];
        Vector3 p2 = _mesh.vertices[_mesh.triangles[(index * 3) + 1]];
        Vector3 p3 = _mesh.vertices[_mesh.triangles[(index * 3) + 2]];

        float r = Random.value;
        float s = Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }

        float a = 1 - r - s;
        float b = r;
        float c = s;

        Vector3 pointOnMesh = a * p1 + b * p2 + c * p3;

        //and then turn them back to a Vector3
        //Vector3 pointOnMesh = p1 + r * (p2 - p1) + s * (p3 - p1);

        data.xy = new Vector2(r, s);

        hit.point = transform.TransformPoint(pointOnMesh);
        hit.normal = GetNormal(p1, p2, p3);

        return hit;
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

    Vector3 GetRandomPositionBetween(Vector3 from, Vector3 to, float t)
    {
        Vector3 position;
        var direction = from - to;
        position = to + direction.normalized * Mathf.Lerp(0, direction.magnitude, t);
        return position;
    }

    float _t;
    [SerializeField]
    float _speed;

    MaterialPropertyBlock _block;

    [SerializeField]
    Vector3 r;

    [SerializeField]
    float _scale;

    // Update is called once per frame
    void Update()
    {
        if (_block == null)
            _block = new MaterialPropertyBlock();


        _t += Time.deltaTime * _speed;

        for (int i = 0; i < _positions.Count; i++)
        {
            var transform = _positions[i];

            _mat.SetFloat("_Scale", _scale);
            _mat.SetFloat("_AnimTex_T", _t * transform.speed);

            var t = RotatePointAroundPivot(transform.position + this.transform.position - _initPosition, this.transform.position, this.transform.rotation * Quaternion.Inverse(_initRotation));
            var rotation = this.transform.rotation * transform.rotation;

            Graphics.DrawMesh(_instancingMesh, t, rotation, _mat, 0, null, 0, _block, false, false);
        }

    }
    
    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = angles * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    public struct TransformData
    {
        public int trianleIndex { get; set; }
        public Vector3 position { get; set; }
        public Vector3 normalized { get; set; }
        public Quaternion rotation { get; set; }
        public float speed { get; set; }
        public Vector2 xy { get; set; }
    }
}
