using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour {

	Light lightDirectional;

	 [System.Serializable]
	public class CycleInfo
	{
		public Color colour;
		public float strength;
		public float softness;
		public float fade;
	}

	/// <summary>
	/// Direction to rotate the sun / moon
	/// </summary>
	public Vector3 Rotational = new Vector3(1, 0, 0);
	private Vector3 defaultRotation;

	public float lightPower = 1.0f;

	public CycleInfo sunColour = new CycleInfo();
	public CycleInfo moonColour = new CycleInfo();

	public float rotationalAmount = 0.0f;

	private bool lightExists;
	private bool isSun;
	private float xAmount;

	void Start () {

		lightDirectional = GetComponent<Light>();

		if (lightDirectional != null)
		{
			lightExists = true;
			isSun = true;
			rotationalAmount = 180 / (GameParameters.halfCycle * 60);
			defaultRotation = transform.rotation.eulerAngles;

			lightDirectional.color = (isSun) ? sunColour.colour : moonColour.colour;
			lightDirectional.shadowSoftness = (isSun) ? sunColour.softness : moonColour.softness;
			lightDirectional.shadowSoftnessFade = (isSun) ? sunColour.fade : moonColour.fade;
		}
	}

	void Update () {
		if (HasLight())
		{
			lightDirectional.transform.Rotate(Rotational, rotationalAmount * Time.deltaTime, Space.World);
			xAmount = lightDirectional.transform.rotation.eulerAngles.x;
			GameParameters.timeOfDay = xAmount;

			lightDirectional.shadowStrength = lightPower * (xAmount / 180.0f) * ((isSun) ? sunColour.strength : moonColour.strength);

			if (xAmount >= 180.0f)
			{
				lightDirectional.transform.rotation = Quaternion.Euler(defaultRotation);
				isSun = !isSun;

				lightDirectional.color = (isSun) ? sunColour.colour : moonColour.colour;
				lightDirectional.shadowSoftness = (isSun) ? sunColour.softness : moonColour.softness;
				lightDirectional.shadowSoftnessFade = (isSun) ? sunColour.fade : moonColour.fade;
			}
		}
	}

	bool HasLight()
	{
		return lightExists;
	}

	// 360 degrees = 1 CycleLength;
}
