using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProductTester : MonoBehaviour {

    Vector3 valueVector = Vector3.up;
    TextMesh textObj;

	// Use this for initialization
	void Start () {
        textObj = GetComponent<TextMesh>();
    }
	
	// Update is called once per frame
	void Update () {

        const float moveScale = 0.5f;

        if (Input.GetKey(KeyCode.I)) valueVector.y += moveScale * Time.deltaTime;
        if (Input.GetKey(KeyCode.K)) valueVector.y -= moveScale * Time.deltaTime;

        if (Input.GetKey(KeyCode.J)) valueVector.x += moveScale * Time.deltaTime;
        if (Input.GetKey(KeyCode.L)) valueVector.x -= moveScale * Time.deltaTime;


        Debug.DrawLine(transform.position, transform.position + valueVector, Color.green);
        Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.blue);

        textObj.text = Vector3.Dot(Vector3.up, valueVector).ToString() + "\n"
            + Vector3.up + "\n" + valueVector;

    }
}
