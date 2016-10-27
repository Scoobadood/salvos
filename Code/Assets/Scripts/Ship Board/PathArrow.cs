using UnityEngine;
using System.Collections;

public class PathArrow : MonoBehaviour {
	public Material	upArrowMaterial;
	public Material	upRightArrowMaterial;
	public Material	rightArrowMaterial;
	public Material	downRightArrowMaterial;
	public Material	downArrowMaterial;
	public Material	downLeftArrowMaterial;
	public Material	leftArrowMaterial;
	public Material	upLeftArrowMaterial;


	public enum CursorState {
		UpArrow,
		UpRightArrow,
		RightArrow,
		DownRightArrow,
		DownArrow,
		DownLeftArrow,
		LeftArrow,
		UpLeftArrow,
	};

	// Current state
	private CursorState mState;

	// Use this for initialization
	void Start () {
		mState = CursorState.UpArrow;
	}

	public void SetState( CursorState newState ) {
		mState = newState;
		Material material = null;

		switch( newState ) {
		case CursorState.UpArrow:
			material = upRightArrowMaterial;
			break;

		case CursorState.UpRightArrow:
			material = upRightArrowMaterial;
			break;
		case CursorState.RightArrow:
			material = rightArrowMaterial;
			break;
		case CursorState.DownRightArrow:
			material = downRightArrowMaterial;
			break;
		case CursorState.DownArrow:
			material = downArrowMaterial;
			break;
		case CursorState.DownLeftArrow:
			material = downLeftArrowMaterial;
			break;
		case CursorState.LeftArrow:
			material = leftArrowMaterial;
			break;
		case CursorState.UpLeftArrow:
			material = upLeftArrowMaterial;
			break;
		}

		gameObject.GetComponent<Renderer> ().material = material;
	}
}
