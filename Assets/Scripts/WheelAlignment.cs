using UnityEngine;
using System.Collections;

public class WheelAlignment : MonoBehaviour
{
	public WheelCollider CorrespondingCollider;
	public GameObject SlipPrefab;
	public GameObject ParticlePrefab;

	float RotationValue = 0.0f;

	void Update()
	{
		if (!CorrespondingCollider)
			return;

		RaycastHit hit;
		Vector3 ColliderCenterPoint = CorrespondingCollider.transform.TransformPoint(CorrespondingCollider.center);

		if (Physics.Raycast(ColliderCenterPoint, -CorrespondingCollider.transform.up, out hit, CorrespondingCollider.suspensionDistance + CorrespondingCollider.radius))
		{
			transform.position = hit.point + (CorrespondingCollider.transform.up * CorrespondingCollider.radius);
		}
		else
		{
			transform.position = ColliderCenterPoint - (CorrespondingCollider.transform.up * CorrespondingCollider.suspensionDistance);
		}

		transform.rotation = CorrespondingCollider.transform.rotation * Quaternion.Euler(RotationValue, CorrespondingCollider.steerAngle, 90);
		RotationValue += CorrespondingCollider.rpm * (360 / 60) * Time.deltaTime;

		WheelHit CorrespondingGroundHit;
		CorrespondingCollider.GetGroundHit(out CorrespondingGroundHit);

		float cghSideSlip = Mathf.Abs(CorrespondingGroundHit.sidewaysSlip);

		// Do we create the skidmarks?
		if (SlipPrefab)
		{
			if (cghSideSlip > 1.0)
			{
				Instantiate(SlipPrefab, CorrespondingGroundHit.point + new Vector3(0, 0.1f, 0), SlipPrefab.transform.rotation);
			}
		}

		// do we have a particle effect to start?
		if (ParticlePrefab)
		{
			if (cghSideSlip > 1.0)
			{
				ParticlePrefab.particleSystem.Play();
			}
			else
			{
				ParticlePrefab.particleSystem.Stop();
			}
		}
	}
}

