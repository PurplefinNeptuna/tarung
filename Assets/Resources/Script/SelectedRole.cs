using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedRole : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler{
	public string Name;
	public void OnPointerEnter(PointerEventData e) {
		Prepare.main.ShowInfo(true, Name);
	}

	public void OnPointerClick(PointerEventData e) {
		Prepare.main.SelectedUnit(Name);
	}
}
