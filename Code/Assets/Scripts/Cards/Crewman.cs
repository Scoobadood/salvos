using UnityEngine;
using System.Collections;

public class Crewman : MonoBehaviour {
	private int		mMaxActionPoints;
	private int		mActionPoints;

	private int		mMaxRangedCombat;
	private int		mRangedCombat;

	private int		mMaxMeleeCombat;
	private int		mMeleeCombat;

	private int		mMaxHitPoints;
	private int 	mHitPoints;

	private int		mMaxEngineering;
	private int		mEngineering;

	private int		mMaxScience;
	private int		mScience;


	private Texture2D	mImage;
	private string 		mName;


	public GameObject	mCardPrefab;


}
