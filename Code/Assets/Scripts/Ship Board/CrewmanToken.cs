using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrewmanToken : MonoBehaviour {
	/** Action points */
	private int mActionPoints;

	/** Max Action points */
	private int mMaxActionPoints;

	public CrewmanToken( ) {
		mMaxActionPoints = 10;
		mActionPoints = mMaxActionPoints;
	}

	/** Action points remaining */
	public int GetActionPoints( ) {
		return mActionPoints;
	}


	/// <summary>
	/// Unhighlight this instance.
	/// </summary>
	public void Unhighlight ()
	{
		gameObject.GetComponent<Renderer> ().material.color = Color.white;
	}

	/// <summary>
	/// Highlights the selected player.
	/// </summary>
	public  void Highlight ()
	{
		gameObject.GetComponent<Renderer> ().material.color = Color.blue;
	}


}
