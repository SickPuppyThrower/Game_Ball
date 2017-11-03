using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_Behaviour : MonoBehaviour {

    public Camera followCamera;
    public Rigidbody ballRigid;
    public Mesh debugTetherMesh; //DEBUG ONLY
    public float rollTorqueMultiplier = 1;
    public float jumpForce = 1;
    public float ropeRange = 5;
    float mouseYaw = 0;
    float mousePitch = 0.5f;
    Vector3 contactNormalAvg = Vector3.up;
    Vector3 upDirection = Vector3.up;
    bool inAir = false;
    float timeLeavingGround = 0;
    GameObject swingTether = null;


    private void OnCollisionExit(Collision collision)
    {
        inAir = true;
        timeLeavingGround = Time.time;
    }


    //Gets ground normals
    private void OnCollisionStay(Collision collision) {
        inAir = false;
        foreach (ContactPoint contact in collision.contacts) {
            contactNormalAvg += contact.normal;
        }
    }


    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
	}
	

	// Update is called once per frame
	void Update () {

        if (contactNormalAvg.magnitude != 0) contactNormalAvg.Normalize();

        //MOVEMENT SHIT------------------------------------------------------------------------------------------------

        //rolling
        Vector3 inputTorque = ((followCamera.transform.right * Input.GetAxis("Vertical")) + (followCamera.transform.forward * -Input.GetAxis("Horizontal"))) * rollTorqueMultiplier;

        ballRigid.AddTorque(inputTorque);

        //jumping
        if (Input.GetButtonDown("Jump") && inAir == false) {
            ballRigid.AddForce(contactNormalAvg * jumpForce, ForceMode.Impulse);
        }

        //CAMERA SHIT--------------------------------------------------------------------------------------------------

        //mouse thangs
        mouseYaw += Input.GetAxis("Mouse X")/100;

        if (mouseYaw < 0) mouseYaw = mouseYaw + 1;
        if (mouseYaw > 1) mouseYaw = mouseYaw - 1;

        mousePitch -= Input.GetAxis("Mouse Y")/100;

        if (mousePitch < 0) mousePitch = 0;
        if (mousePitch > 1) mousePitch = 1;

        //changes relative up direction based on ground normal
        if (inAir && (timeLeavingGround + 1 < Time.time)) {
            upDirection = Vector3.Lerp(upDirection, Vector3.up, 0.05f);
        }
        else upDirection = Vector3.Lerp(upDirection, contactNormalAvg, 0.1f);
        
        //camera orientation and offset
        //Vector3 cameraOffsetTarget = new Vector3(0, ((mousePitch * 0.5f) + 0.5f), -2);
        Vector3 cameraOffsetTarget = new Vector3(0, 2, -3);

        Quaternion cameraOrientation = Quaternion.FromToRotation(Vector3.up, upDirection) * Quaternion.Euler((mousePitch * 120) - 75, (mouseYaw * 360), 0);

        followCamera.transform.position = (cameraOrientation * cameraOffsetTarget)  + transform.position;
        followCamera.transform.rotation = cameraOrientation;

        Debug.Log(upDirection);



        //SHOOTING SPIDEY WEB------------------------------------------------------------------------------------------
        RaycastHit lookCastHit;
        RaycastHit ballCastHit;

        //WILL HAVE TO MAKE LOOKHITPOINT A CHILD OF HIT TARGET#################################################################################################
        bool lookCast = Physics.Raycast(followCamera.transform.position, followCamera.transform.forward, out lookCastHit, ropeRange * 2);
        bool ballCast = false;


        //CHANGE THIS, CAN ONLY BE ON WILST LOOKING AT TARGET
        if (Input.GetButtonDown("Fire1") && lookCast) {
            Vector3 ballRopeDirection = Vector3.Normalize(lookCastHit.point - transform.position);
            ballCast = Physics.Raycast(transform.position, ballRopeDirection, out ballCastHit, ropeRange * 2);

            if (ballCast) {
                swingTether = new GameObject("SwingTether");
                swingTether.AddComponent<MeshFilter>();
                swingTether.GetComponent<MeshFilter>().mesh = debugTetherMesh;
                swingTether.AddComponent<MeshRenderer>();
                swingTether.transform.position = ballCastHit.point;
            }
        }

        if (swingTether != null) {
            if (Input.GetButtonUp("Fire1")) Destroy(swingTether);
            Debug.Log(swingTether.transform.position);
            //Position of ball on next frame
            Vector3 ballPosPredict = (ballRigid.velocity * Time.deltaTime) + transform.position;
            //Distance ball might be from tether on next frame
            float btPredictDistance = Vector3.Distance(swingTether.transform.position, ballPosPredict);
            //Vector from ball to tether
            Vector3 ballToTether = swingTether.transform.position - transform.position;
            ballRigid.drag = 0;

            if (btPredictDistance > ropeRange) {
            //if (true) {
                //ballRigid.useGravity = false;
                //Vector3 swingAcc = (Vector3.Dot(ballRigid.velocity, ballRigid.velocity) / Mathf.Pow(ropeRange, 2)) * ballToTether;
                //Vector3 retargetGravAcc = (Vector3.Dot(Physics.gravity, Physics.gravity) / Mathf.Pow(ropeRange, 2)) * ballToTether;
                //ballRigid.velocity += (swingAcc + retargetGravAcc) * Time.deltaTime;

                
                float directionModifier = Vector3.Dot(ballRigid.velocity + Physics.gravity, -ballToTether.normalized);
                Vector3 swingAcc = ((btPredictDistance - ropeRange) * directionModifier) * ballToTether;
                

                ballRigid.velocity += swingAcc * Time.deltaTime;
                //ballRigid.AddForce(swingAcc, ForceMode.Acceleration);
            }
            else ballRigid.useGravity = true;

        }
        else { ballRigid.useGravity = true; ballRigid.drag = 1; }

        Debug.DrawLine(transform.position, transform.position + ballRigid.velocity, Color.cyan);
    }
}

