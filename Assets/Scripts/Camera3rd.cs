using UnityEngine;
using System.Collections;

public class Camera3rd : MonoBehaviour {

	public GameObject objectToTarget;

	public float damping = 0.5f;
	Vector3 offset;

	void Start()
	{
		offset = objectToTarget.transform.position - transform.position;
	}

	void LateUpdate()
	{
		float currentAngle = transform.eulerAngles.y;
		float desiredAngle = objectToTarget.transform.eulerAngles.y;
		float angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);

		Quaternion rotation = Quaternion.Euler(0, angle, 0);
		transform.position = objectToTarget.transform.position - (rotation * offset);

		transform.LookAt(objectToTarget.transform);
	}
}

