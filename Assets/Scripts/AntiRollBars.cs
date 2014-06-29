using UnityEngine;
using System.Collections;

public class AntiRollBars : MonoBehaviour
{
	public WheelCollider wheelL;
	public WheelCollider wheelR;
	public float antiRollVal = 500.0f;

	void Update()
	{
		WheelHit hit;
	
		float travelL = 1.0f;
		float travelR = 1.0f;
	
		bool groundedL = wheelL.GetGroundHit(out hit);
		bool groundedR = wheelR.GetGroundHit(out hit);
	
		if (groundedL)
		{
			travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
		}
	
		if (groundedR)
		{
			travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
		}

		float antiRollForce = (travelL - travelR) * antiRollVal;
	
		if (groundedL)
		{
			rigidbody.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
		}
	
		if (groundedR)
		{
			rigidbody.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position);
		}
	}

}