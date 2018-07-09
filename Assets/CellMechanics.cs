using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CellMechanics : MonoBehaviour {

    public Vector3 scale;
    public Vector3 v;
    public Vector3 f;
    public Vector3 pos;
    public Vector3 orientation;

    private CellBehavior cellBehavior;
    public Rigidbody rb;
    // Use this for initialization
    void Start() {

        rb = GetComponent<Rigidbody>();


    }

    public void SetOrientation() {

        float theta = Utilities.GetRandomNumber(0, Mathf.PI * 2);
        float z = Utilities.GetRandomNumber(-1, 1);
        float temp = Mathf.Sqrt(1 - z * z);

        orientation = new Vector3(temp * Mathf.Cos(theta), temp * Mathf.Sin(theta), z);

    }

    // Update is called once per frame
    void Update() {

        rb.AddForce(f);

        //		this.v = this.v + this.f * Time.deltaTime;
        //		this.transform.position =  this.transform.position + this.v * Time.deltaTime; 

    }
}
