using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class Stats : MonoBehaviour {

	[SerializeField] Text T_lifesLeft;
	[SerializeField] Text T_kills;

	int kills;
	[SerializeField]int lifes;

	private void Start(){
		kills = 0;
		ShowLifes ();
		ShowKills ();
	}

	public void RemoveLife(){
		lifes --;
		if (lifes <= 0) {
			Debug.Log ("Game lost!");
			Application.LoadLevel(Application.loadedLevel);
		} else {
			ShowLifes();
		}
	}
	public void AddKill(){
		kills++;
		ShowKills ();
	}

	private void ShowKills(){
		T_kills.text = "Kills: " + kills;
	}
	private void ShowLifes(){
		T_lifesLeft.text = "Lifes left: " + lifes;
	}
}
