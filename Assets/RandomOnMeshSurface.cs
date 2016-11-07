using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class RandomOnMeshSurface : MonoBehaviour
{
    Mesh _baked;
    Mesh _mesh;
    float[] _triangleSizes;

    [SerializeField]
    public Mesh _instancingMesh;
    [SerializeField]
    Material _mat;
    [SerializeField]
    int _countInMaxSizeSingleTriangle;
    [SerializeField]
    int _instancingDensity;
    [SerializeField]
    ComputeShader _coreComputeShader;
    [SerializeField]
    Vector3 _accel;
    [SerializeField]
    float scale;
    [SerializeField]
    int limitLoopCount;

    List<InstanceData> _dataList;
    InstanceData[] _dataArr;


    Quaternion _initRotation;
    Vector3 _initPosition;

    Vector3[] _vertices;
    int[] _triangles;


    ComputeBuffer _vertextBuffer;
    ComputeBuffer _triangleBuffer;
    ComputeBuffer _instanceDataBuffer;

    const int BLOCK_SIZE = 8;

    int _k_index_main;

    SkinnedMeshRenderer _skin;

    Matrix4x4?[] _matrixList;



    void Awake()
    {
        _k_index_main = _coreComputeShader.FindKernel("Update");
    }

    // Use this for initialization
    void Start()
    {
        _baked = new Mesh();
        _initRotation = this.transform.rotation;
        _initPosition = this.transform.position;


        _dataList = new List<InstanceData>();

        MeshFilter mf = this.GetComponent<MeshFilter>();
        if (mf != null)
        {
            _mesh = mf.mesh;
        }
        else
        {
            _skin = this.GetComponent<SkinnedMeshRenderer>();
            if (_skin != null)
                _mesh = _skin.sharedMesh;
        }

        //for (int i = 0; i < _mesh.triangles.Length; i++)
        //{
        //    Debug.Log("_mesh.triangles : " + _mesh.triangles[i]);
        //}

        int triangle_count = _mesh.triangles.Length / 3;

        int pointCount = 0;

        _triangleSizes = new float[triangle_count];
        for (int i = 0; i < triangle_count; i++)
        {
            _triangleSizes[i] = GetTriangleSize(i);
        }

        var minTriangleSize = _triangleSizes.Min();
        var maxTriangleSize = _triangleSizes.Max();

        //Debug.Log("triangle_count : " + triangle_count);

        for (int i = 0; i < triangle_count; i++)
        {
            var size = _triangleSizes[i];

            int instancingCount = Mathf.FloorToInt(_countInMaxSizeSingleTriangle * Mathf.Clamp01((size / maxTriangleSize) * _instancingDensity));

            if(instancingCount == 0)
            {
                if (Random.value <= 0.05f)
                    instancingCount = 1;
            }

            for (int j = 0; j < instancingCount; j++)
            {
                var d = NewInstancingData(i);
                if(d.HasValue)
                {
                    _dataList.Add(d.Value);
                }
            }
        }

        _matrixList = new Matrix4x4?[_dataList.Count];
        _dataArr = _dataList.ToArray();

        _instanceDataBuffer = new ComputeBuffer(_dataList.Count, Marshal.SizeOf(typeof(InstanceData)));
        _vertextBuffer = new ComputeBuffer(_mesh.vertices.Length, Marshal.SizeOf(typeof(Vector3)));
        _triangleBuffer = new ComputeBuffer(_mesh.triangles.Length, sizeof(int));

    }


    //float Normalized(float min, float max, float v)
    //{
    //    return (v - min) / (max - min);
    //}

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

    InstanceData? NewInstancingData(int index)
    {
        //Vector3 p1 = _mesh.vertices[_mesh.triangles[(index * 3) + 0]];
        //Vector3 p2 = _mesh.vertices[_mesh.triangles[(index * 3) + 1]];
        //Vector3 p3 = _mesh.vertices[_mesh.triangles[(index * 3) + 2]];

        //var normalize = GetNormal(p1, p2, p3);
        //if (normalize.z < -0.25f)
            //return null;

        InstanceData data = new InstanceData();
        data.IsAlive = false;
        data.trianleIndex = index;
        data.angle = Random.Range(0f, 360f);
        float u = Random.value;
        float v = Random.value;
        data.xy = new Vector2(u, v);
        data.speed = Random.Range(0.7f, 1f);

        return data;
    }

    //Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    //{
    //    Vector3 side1 = b - a;
    //    Vector3 side2 = c - a;
    //    return Vector3.Cross(side1, side2).normalized;
    //}

    //Vector3 GetRandomPositionBetween(Vector3 from, Vector3 to, float t)
    //{
    //    Vector3 position;
    //    var direction = from - to;
    //    position = to + direction.normalized * Mathf.Lerp(0, direction.magnitude, t);
    //    return position;
    //}

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
        if(Input.GetKeyDown(KeyCode.A))
        {
            AwakeFlowers();
        }

        if (_dataArr == null)
            return;

        if (_dataArr.Length == 0)
            return;

        if (_block == null)
            _block = new MaterialPropertyBlock();

        int num_active_blocks = (_dataArr.Length / BLOCK_SIZE) + _dataArr.Length % BLOCK_SIZE;

        if (_skin != null)
        {
            _skin.BakeMesh(_baked);

            _vertices = _baked.vertices;
            _triangles = _baked.triangles;
        }
        else
        {
            _vertices = _mesh.vertices;
            _triangles = _mesh.triangles;
        }


        _instanceDataBuffer.SetData(_dataArr);
        _vertextBuffer.SetData(_vertices);
        _triangleBuffer.SetData(_triangles);

        _coreComputeShader.SetBuffer(_k_index_main, "data", _instanceDataBuffer);
        _coreComputeShader.SetBuffer(_k_index_main, "vertices", _vertextBuffer);
        _coreComputeShader.SetBuffer(_k_index_main, "triangles", _triangleBuffer);

        _coreComputeShader.Dispatch(_k_index_main, num_active_blocks, 1, 1);

        _instanceDataBuffer.GetData(_dataArr);

        for (int i = 0; i < _dataArr.Length; i++)
        {
            var data = _dataArr[i];

            if (data.IsAlive == false)
                continue;

            _block.SetFloat("_Scale", _scale);

            _dataArr[i].LifeTime += Time.deltaTime * _speed;
            var t = _dataArr[i].LifeTime * data.speed;

            _block.SetFloat("_AnimTex_T", t);

            var position = transform.TransformPoint((data.position) * scale);

            var rotation = Quaternion.LookRotation(data.normalized);
            rotation = rotation * Quaternion.Euler(r);

            Quaternion angle = Quaternion.AngleAxis(data.angle, Vector3.up);
            rotation = rotation * angle;

            rotation = this.transform.rotation * rotation;

            if (t >= 29f)
            {
                var mtx = _matrixList[i];
                if (mtx.HasValue == false)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
                    _matrixList[i] = matrix;
                }

                _dataArr[i].velocity += _accel;
                _dataArr[i].position1 += _dataArr[i].velocity;
                _block.SetVector("_Velocity", _dataArr[i].position1);
                _block.SetMatrix("_Position", _matrixList[i].Value);
            }

            Graphics.DrawMesh(_instancingMesh, position, rotation, _mat, 0, null, 0, _block, false, false);

            if (t >= _mat.GetVector("_AnimTex_AnimEnd").x)
            {
                _dataArr[i].LifeTime = 0;
                _dataArr[i].velocity = Vector3.zero;
                _dataArr[i].position1 = Vector3.zero;
                _dataArr[i].xy = new Vector2(Random.value, Random.value);
                _dataArr[i].angle = Random.Range(0f, 360f);
                _dataArr[i].speed = Random.Range(0.7f, 1f);
                _dataArr[i].LoopCount += 1;
                _matrixList[i] = null;

                if(_dataArr[i].LoopCount == limitLoopCount)
                {
                    _dataArr[i].IsAlive = false;
                    _dataArr[i].LoopCount = 0;
                }
            }
        }
    }

    public void AwakeFlowers()
    {
        for (int i = 0; i < _dataArr.Length; i++)
        {
            _dataArr[i].IsAlive = true;
        }
    }

    //public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angles)
    //{
    //    Vector3 dir = point - pivot; // get point direction relative to pivot
    //    dir = angles * dir; // rotate it
    //    point = dir + pivot; // calculate rotated point
    //    return point; // return it
    //}

    void OnDisable()
    {
        if(_instanceDataBuffer != null)
            _instanceDataBuffer.Release();
        if (_vertextBuffer != null)
            _vertextBuffer.Release();
        if (_triangleBuffer != null)
            _triangleBuffer.Release();
    }
    public struct InstanceData
    {
        public int trianleIndex { get; set; }
        public Vector3 position { get; set; }
        public Vector3 normalized { get; set; }
        public Vector3 rotation { get; set; }
        public float speed { get; set; }
        public Vector2 xy { get; set; }
        public float angle { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 position1 { get; set; }
        public bool IsAlive { get; set; }
        public float LifeTime { get; set; }
        public int LoopCount { get; set; }
    }
}
