using UnityEngine;
using System.Collections;

public class Flower : MonoBehaviour {

    VertexAnimator _vertexAnimator;
    MaterialPropertyBlock _block;

    [SerializeField]
    Vector3 world_position;
    [SerializeField]
    Vector3 world_rotation;

    // Use this for initialization
    void Start () {

        _block = new MaterialPropertyBlock();
        _vertexAnimator = this.GetComponent<VertexAnimator>();
        _vertexAnimator.MaterialPropertyBlock = _block;
    }
	
	// Update is called once per frame
	void Update () {

        world_position = this.transform.position;
        world_rotation = this.transform.rotation.eulerAngles;

    }
}
