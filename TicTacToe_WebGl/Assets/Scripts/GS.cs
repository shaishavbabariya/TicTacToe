using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GS : MonoBehaviour {

	public static GS Inst;

	[SerializeField] Sprite[] UProps;
	[SerializeField] Sprite[] OProps;

	internal Sprite UserProp;
	internal Sprite OppProp;

	void Awake(){
		Inst = this;
		UserProp = UProps [0];
		OppProp = OProps  [0];
	}
	// Use this for initialization
	void Start () {
	
	}
}
