using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class Droppable : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
	
	public void OnPointerEnter( PointerEventData eventData ) {
		Debug.Log ("Enter");

		// Highlight the border
		Image  im = this.transform.GetComponent<Image> ();
		im.color = Color.white;

		if (eventData.pointerDrag != null) {
			Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
			if (d != null) {
				d.mCurrentParent = this.gameObject.transform;
			}
		}
	}

	public void OnPointerExit( PointerEventData eventData ) {
		Debug.Log ("Exit");

		Image  im = this.transform.GetComponent<Image> ();
		im.color = Color.clear;

		if (eventData.pointerDrag != null) {
			Draggable d = eventData.pointerDrag.GetComponent<Draggable> ();
			if (d != null) {
				d.mCurrentParent = null;
			}
		}
	}

	public void OnDrop( PointerEventData eventData ) {
		Debug.Log ("Drop");
	}

}
