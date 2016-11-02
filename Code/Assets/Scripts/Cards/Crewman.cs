using UnityEngine;
using System.Collections;


/// <summary>
/// The Crewman is a member of the player's team. They have a class which determines the things they can carry 
/// as well as skills they can perform.
/// They also have a level which is basically how good they are. Levels run from 1 to 6 where 1 is the starting level and barely competent whicl 6 is an expert
/// 
/// 
/// </summary>
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
