using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour {

	public delegate void MapChangedEventHandler(Tile newTarget);
	public delegate void StopAllEventHandler ();
	public event MapChangedEventHandler MapChangedEvent;
	public event StopAllEventHandler StopAllEvent;
	[SerializeField] float waveTime = 5.0f;
	int countPerWave = 3;
	[SerializeField] Enemy enemy_prefab;
	private int waveCount;

	public void Start(){
		StartCoroutine (SpawnMonsters ());
	}
	public void StopAll(){
		if (StopAllEvent != null) {
			StopAllEvent();
		}
	}
	private IEnumerator SpawnMonsters(){
		while (true) {
			WebAPI.GetWave(delegate(bool newWave) {
				if(newWave){
					StartCoroutine(AddMonsters(countPerWave));
					waveCount++;
				}

			});
			yield return new WaitForSeconds(1.0f);
				
		}
	}
	private IEnumerator AddMonsters(int number){
		for(int i = 0; i<number; i++){
			AddMonster();
			yield return new WaitForSeconds(10f/Mathf.Max(waveCount,1));
		}
		yield return new WaitForSeconds(waveTime);
	}
	private void AddMonster(){
		Enemy enemy = Instantiate (enemy_prefab) as Enemy;
		enemy.transform.position = new Vector3 (Board.startTile.x, 0, Board.startTile.y);
		enemy.Configure (this, 1+waveCount%2, (1+waveCount)*.05f );
		enemy.CalculatePath (Board.endTile);
	}
	public void MapChanged(){
		if (MapChangedEvent != null) {
			MapChangedEvent(Board.endTile);
		}
	}


}
