using UnityEngine;
using System.Collections;

public class Quad : MonoBehaviour {

    [SerializeField]
    Vector3 offset;
    
    Mesh _mesh;
    Vector3[] _vertices;
    Vector3[] _vertices_c;

    // Use this for initialization
    void Start () {

        _mesh = this.GetComponent<MeshFilter>().mesh;
        _vertices = _mesh.vertices;
        _vertices_c = new Vector3[_vertices.Length];
    }
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < _vertices.Length; i++)
        {
            if (i == 0)
            {
                _vertices_c[i] = _vertices[i] + offset;
            }
            else
            {
                _vertices_c[i] = _vertices[i];
            }
        }

        _mesh.vertices =_vertices_c;

    }
}
