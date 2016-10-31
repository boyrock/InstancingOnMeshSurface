using UnityEngine;
using System.Collections;

public class Tetragon : MonoBehaviour {

    [SerializeField]
    float _rotationSpeed;

    Vector3 _rotateAxis;
	// Use this for initialization
	void Start () {
        //_rotateAxis = Vector3.up; //Random.insideUnitSphere;
        _rotateAxis = Random.insideUnitSphere;
    }
	
	// Update is called once per frame
	void Update () {

        this.transform.Rotate(_rotateAxis * _rotationSpeed);
	}
}
