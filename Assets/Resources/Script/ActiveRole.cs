using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveRole : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
	public string Name;
	public int ID;
	public void OnPointerEnter(PointerEventData e) {
		if (Name != null && Name != "Empty")
			Prepare.main.ShowInfo(true, Name);
	}

	public void OnPointerClick(PointerEventData e) {
		Prepare.main.RemoveUnit(ID);
	}
}
