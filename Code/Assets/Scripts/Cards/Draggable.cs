using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler  {
	// Original parent
	Transform	mOriginalParent = null;

	// Current parent may be another dropzone
	public Transform	mCurrentParent = null;

	// Offset of the drag point from the origin
	private Vector2 mDragOffset;

	public void OnBeginDrag( PointerEventData eventData ) {
		Debug.Log ("Begin Drag");
		mOriginalParent = gameObject.transform.parent;
		mCurrentParent = mOriginalParent;

		mDragOffset.x = eventData.position.x - gameObject.transform.position.x;
		mDragOffset.y = eventData.position.y - gameObject.transform.position.y;

		gameObject.transform.parent = gameObject.transform.parent.parent;

		CanvasGroup cg = GetComponent<CanvasGroup> ();
		cg.blocksRaycasts = false;
	}

	public void OnDrag( PointerEventData eventData ) {
		gameObject.transform.position = eventData.position - mDragOffset;
	}

	public void OnEndDrag( PointerEventData eventData ) {
		Debug.Log ("End Drag");
		if (mCurrentParent != null) {
			gameObject.transform.parent = mCurrentParent;
		} else {
			gameObject.transform.parent = mOriginalParent;
		}

		CanvasGroup cg = GetComponent<CanvasGroup> ();
		cg.blocksRaycasts = true;
	}
}
