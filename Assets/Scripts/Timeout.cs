using UnityEngine;
using System.Collections;

public class Timeout : MonoBehaviour {

	public float alivetime = 1.0f;
	/// <summary>
	/// Will only remove once timeout has passed and it outside view
	/// </summary>
	public bool OnLeaveView = true;
	bool inView = false;

	// Update is called once per frame
	void Update () {
		alivetime -= Time.deltaTime;
		if (alivetime <= 0.0f)
		{
			if ((OnLeaveView && !inView) || !OnLeaveView)
			{
				Destroy(this.gameObject);
			}
		}
	}

	void OnBecameInvisible()
	{
		inView = false;
	}

	void OnBecameVisible()
	{
		inView = true;
	}
}
