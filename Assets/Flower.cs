using UnityEngine;
using System.Collections;

public class Flower : MonoBehaviour {

    VertexAnimator _vertexAnimator;
    MaterialPropertyBlock _block;
	// Use this for initialization
	void Start () {

        _block = new MaterialPropertyBlock();
        _vertexAnimator = this.GetComponent<VertexAnimator>();
        _vertexAnimator.MaterialPropertyBlock = _block;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
