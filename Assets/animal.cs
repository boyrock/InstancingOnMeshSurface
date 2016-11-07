using UnityEngine;
using System.Collections;

public class animal : MonoBehaviour {

    [SerializeField]
    float _walkSpeed;

    [SerializeField]
    float _widthSize;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        this.transform.localPosition += Vector3.left * Time.deltaTime * _walkSpeed;

        if(Camera.main.WorldToViewportPoint(this.transform.position).x + _widthSize <= 0)
        {
            var pos = this.transform.localPosition;
            this.transform.localPosition = new Vector3(7f, pos.y, pos.z);
        }
	}
}
