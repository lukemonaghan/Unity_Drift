using UnityEngine;
using System.Collections;

// ----------- CAR TUTORIAL-----------------

public class PlayerCar : MonoBehaviour
{
	[System.Serializable]
	public class WheelSlipValues
	{
		public WheelSlipValues(float minf, float maxf, float mins, float maxs)
		{
			MinForward = minf;
			Forward = MaxForward = maxf;
			MinSideways = mins;
			Sideways = MaxSideways = maxs;
		}
		public void Update()
		{
			Sideways = MaxSideways;
			Forward = MaxForward;
		}
		public float MinForward;
		public float MaxForward;
		public float MinSideways;
		public float MaxSideways;
	
		[HideInInspector]
		public float Forward;
		[HideInInspector]
		public float Sideways;
	}
	
	public WheelSlipValues FrontLeftWheelSlip = new WheelSlipValues(0.8f, 0.92f, 0.05f, 0.5f);
	public WheelSlipValues FrontRightWheelSlip = new WheelSlipValues(0.8f, 0.92f, 0.05f, 0.5f);
	public WheelSlipValues RearLeftWheelSlip = new WheelSlipValues(0.8f, 0.92f, 0.05f, 0.5f);
	public WheelSlipValues RearRightWheelSlip = new WheelSlipValues(0.8f, 0.92f, 0.05f, 0.5f);

	// Brake lights
	public GameObject BackLeftLight;
	public GameObject BackRightLight;

	// These variables allow the script to power the wheels of the car.
	public WheelCollider FrontLeftWheel;
	public WheelCollider FrontRightWheel;
	public WheelCollider RearLeftWheel;
	public WheelCollider RearRightWheel;

	// 4.31 2.71 1.88 1.41 1.13 0.93 Default gears
	public float[] GearRatio = new float[6] { 4.31f, 2.7f, 1.88f, 1.41f, 1.13f, 0.93f };
	public int CurrentGear = 0;

	public float BrakeTorque = 100.0f;
	public float EngineTorque = 500.0f;
	public float MaxEngineRPM = 8000.0f; 
	public float MinEngineRPM = 1000.0f;
	public float SteeringAngle = 15.0f;
	float SteeringAngleLocal;

	private float EngineRPM = 0.0f;

	public enum CarTypes { ALL,FRONT,REAR }

	public CarTypes cartype = CarTypes.REAR;
	public CarTypes handbrakeOn = CarTypes.ALL;

	bool DebugShowing = false;

	void Start()
	{
		rigidbody.centerOfMass = new Vector3(0.0f, 0.0f, 0.0f);

		FrontLeftWheelSlip.Update();
		FrontRightWheelSlip.Update();
		RearLeftWheelSlip.Update();
		RearRightWheelSlip.Update();

		UpdateStiffness(ref FrontLeftWheel  ,FrontLeftWheelSlip.Forward ,FrontLeftWheelSlip.Sideways);
		UpdateStiffness(ref FrontRightWheel ,FrontRightWheelSlip.Forward,FrontRightWheelSlip.Sideways);
		UpdateStiffness(ref RearLeftWheel   ,RearLeftWheelSlip.Forward	,RearLeftWheelSlip.Sideways);
		UpdateStiffness(ref RearRightWheel  ,RearRightWheelSlip.Forward, RearRightWheelSlip.Sideways);

	}

	void Update()
	{
		if (Input.GetKeyDown("r"))
		{
			transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
			transform.Translate(0, 2, 0);
		}

		audio.pitch = Mathf.Abs(EngineRPM / MaxEngineRPM) + 0.5f;
		if (audio.pitch > 2.0f)
		{
			audio.pitch = 2.0f;
		}

		float forward = Input.GetAxis("Vertical") + Input.GetAxis("Acceleration");
		float horizontal = Input.GetAxis("Horizontal");

		if (BackLeftLight && BackRightLight)
		{
			if (forward < 0.0f)
			{
				BackLeftLight.SetActive(true);
				BackRightLight.SetActive(true);
			}
			else
			{
				BackLeftLight.SetActive(false);
				BackRightLight.SetActive(false);
			}
		}

		if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button1))
		{
			if (handbrakeOn == CarTypes.ALL || handbrakeOn == CarTypes.FRONT)
			{
				FrontLeftWheel.brakeTorque += BrakeTorque;
				UpdateMinMax(ref FrontLeftWheelSlip, ref FrontLeftWheel, true);
				FrontRightWheel.brakeTorque += BrakeTorque;
				UpdateMinMax(ref FrontRightWheelSlip, ref FrontRightWheel, true);
			}
			if (handbrakeOn == CarTypes.ALL || handbrakeOn == CarTypes.REAR)
			{
				RearLeftWheel.brakeTorque += BrakeTorque;
				UpdateMinMax(ref RearLeftWheelSlip, ref RearLeftWheel, true);
				RearRightWheel.brakeTorque += BrakeTorque;
				UpdateMinMax(ref RearRightWheelSlip, ref RearRightWheel, true);
			}
		}
		else
		{
			FrontLeftWheel.brakeTorque = 0.0f;
			FrontRightWheel.brakeTorque = 0.0f;
			RearLeftWheel.brakeTorque = 0.0f;
			RearRightWheel.brakeTorque = 0.0f;

			UpdateMinMax(ref FrontLeftWheelSlip, ref FrontLeftWheel, false);
			UpdateMinMax(ref FrontRightWheelSlip, ref FrontRightWheel, false);
			UpdateMinMax(ref RearLeftWheelSlip, ref RearLeftWheel, false);
			UpdateMinMax(ref RearRightWheelSlip, ref RearRightWheel, false);
		}

		// Compute the engine RPM based on the average RPM of the two wheels, then call the shift gear function
		EngineRPM = (FrontLeftWheel.rpm + FrontRightWheel.rpm) / 2 * GearRatio[CurrentGear];
		if (EngineRPM <= 0.1f && EngineRPM >= -0.1f){ EngineRPM = 0.0f; }
		ShiftGears();

		// Front wheels
		if (cartype == CarTypes.ALL || cartype == CarTypes.FRONT)
		{
			FrontLeftWheel.motorTorque  = EngineTorque / GearRatio[CurrentGear] * forward;
			FrontRightWheel.motorTorque = EngineTorque / GearRatio[CurrentGear] * forward;
		}

		// Rear wheels
		if (cartype == CarTypes.ALL || cartype == CarTypes.REAR)
		{
			RearLeftWheel.motorTorque = EngineTorque / GearRatio[CurrentGear] * forward;
			RearRightWheel.motorTorque = EngineTorque / GearRatio[CurrentGear] * forward;
		}

		// the steer angle is an arbitrary value multiplied by the user input.
		SteeringAngleLocal = FrontRightWheel.steerAngle = FrontLeftWheel.steerAngle = (SteeringAngle * horizontal);

		if (Input.GetKeyDown(KeyCode.F1))
		{
			DebugShowing = !DebugShowing;
		}
	}

	void UpdateMinMax(ref WheelSlipValues slipvals,ref WheelCollider collider, bool ondown)
	{
		if (ondown)
		{
			if (slipvals.Forward > slipvals.MinForward)
			{
				slipvals.Forward = collider.forwardFriction.stiffness * 0.9f;
			}
			if (slipvals.Sideways > slipvals.MinSideways)
			{
				slipvals.Sideways = collider.sidewaysFriction.stiffness * 0.9f;
			}
		}
		else
		{
			if (slipvals.Forward < slipvals.MaxForward)
			{
				slipvals.Forward = collider.forwardFriction.stiffness * 1.1f;
			}
			if (slipvals.Sideways < slipvals.MaxSideways)
			{
				slipvals.Sideways = collider.sidewaysFriction.stiffness * 1.1f;
			}
		}
		UpdateStiffness(ref collider, slipvals.Forward, slipvals.Sideways);
	}

	public void UpdateStiffness(ref WheelCollider collider, float forward, float sideways)
	{
		if (collider)
		{
			if (forward != -1.0f)
			{
				WheelFrictionCurve w1 = new WheelFrictionCurve();
				w1.stiffness = forward;
				w1.extremumValue = collider.forwardFriction.extremumValue;
				w1.extremumSlip = collider.forwardFriction.extremumSlip;
				w1.asymptoteValue = collider.forwardFriction.asymptoteValue;
				w1.asymptoteSlip = collider.forwardFriction.asymptoteSlip;
				collider.forwardFriction = w1;
			}
			if (sideways != -1.0f)
			{
				WheelFrictionCurve w1 = new WheelFrictionCurve();
				w1.stiffness = sideways;
				w1.extremumValue = collider.sidewaysFriction.extremumValue;
				w1.extremumSlip = collider.sidewaysFriction.extremumSlip;
				w1.asymptoteValue = collider.sidewaysFriction.asymptoteValue;
				w1.asymptoteSlip = collider.sidewaysFriction.asymptoteSlip;
				collider.sidewaysFriction = w1;
			}
		}
	}

	void ShiftGears()
	{
		// this funciton shifts the gears of the vehcile, it loops through all the gears, checking which will make
		// the engine RPM fall within the desired range. The gear is then set to this "appropriate" value.
		int AppropriateGear = CurrentGear;

		if (EngineRPM >= MaxEngineRPM)
		{
			for (var i = 0; i < GearRatio.GetLength(0); i++)
			{
				if (FrontLeftWheel.rpm * GearRatio[i] < MaxEngineRPM)
				{
					AppropriateGear = i;
					break;
				}
			}

			CurrentGear = AppropriateGear;
		}

		if (EngineRPM <= MinEngineRPM)
		{
			AppropriateGear = CurrentGear;

			for (var j = GearRatio.GetLength(0) - 1; j >= 0; j--)
			{
				if (FrontLeftWheel.rpm * GearRatio[j] > MinEngineRPM)
				{
					AppropriateGear = j;
					break;
				}
			}

			CurrentGear = AppropriateGear;
		}
	}

	void OnGUI()
	{
		if (DebugShowing)
		{
			GUI.TextArea(new Rect(0,  0, 512, 24), "RPM : " + EngineRPM.ToString() + " Gear : " + CurrentGear.ToString() + " Drive : " + cartype.ToString() + " KM/h : " + Mathf.Round(transform.InverseTransformDirection(rigidbody.velocity).z).ToString());
			GUI.TextArea(new Rect(0, 24, 512, 24), "FL : " + FrontLeftWheel.brakeTorque.ToString() + " FR : " + FrontRightWheel.brakeTorque.ToString() + " RL : " + RearLeftWheel.brakeTorque.ToString() + " RR : " + RearRightWheel.brakeTorque.ToString());
			GUI.TextArea(new Rect(0, 48, 512, 24), "FLS : " + FrontLeftWheelSlip.Forward + " : " + FrontLeftWheelSlip.Sideways + " FRS : " + FrontRightWheelSlip.Forward + " : " + FrontRightWheelSlip.Sideways + " RLS : " + RearLeftWheelSlip.Forward + " : " + RearLeftWheelSlip.Sideways + " RRS : " + RearRightWheelSlip.Forward + " : " + RearRightWheelSlip.Sideways);
			GUI.TextArea(new Rect(0, 72, 512, 24), "COM : " + rigidbody.centerOfMass.ToString() + " SteeringAngle : " + SteeringAngleLocal.ToString() );
		}
	}
}

