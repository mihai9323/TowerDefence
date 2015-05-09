using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	private EnemyManager myManager;
	public float health{
		set{
			_health = value;
			if(_health<0){
				Board.stats.AddKill();
				Destroy(this.gameObject);
			}
		}
		get{
			return _health;
		}
	}

	private float _health;

	public float speed;
	public Tile currentTile{
		get{
			return Board.GameBoard[(int)this.transform.position.x, (int)this.transform.position.z];
		}
	}

	public void Configure(EnemyManager enemyManager, float health,float speed){
		myManager = enemyManager;
		myManager.StopAllEvent += DestroyObject;
		myManager.MapChangedEvent += CalculatePath;
		this.speed = speed;
		this.health = health;
	}
	private void OnDestroy(){

		myManager.StopAllEvent -= DestroyObject;
		myManager.MapChangedEvent -= CalculatePath;
	}
	public void CalculatePath(Tile finalTile){

		Tile[] path = AStar.CalculatePath (currentTile, finalTile).ToArray();
		if(path.Length >=0)StartCoroutine (MoveOnPath (path));
		else{
			Destroy(this.gameObject,.5f);
		}
	}

	public void DestroyObject(){
		Destroy (this.gameObject);
	}

	private IEnumerator MoveOnPath(Tile[] path){
		foreach (Tile tile in path) {
			if(tile != currentTile){
				float ct = 0;
				Vector3 initialPosition = transform.position;
				while(ct<1){
					transform.position = Vector3.Lerp(initialPosition, tile.transform.position,ct);
					ct+= speed * Time.deltaTime;
					yield return null;
				}
			}
		}
		Board.stats.RemoveLife ();
		Destroy (this.gameObject, .5f);
	}

}
