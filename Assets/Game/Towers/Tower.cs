using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour {
	[SerializeField] float attackRange;
	[SerializeField] float attackSpeed;
	[SerializeField] Bullet bullet_prefab;
	[SerializeField] Transform bulletSpawn;
	private void Start(){
		StartCoroutine (AIRefresh ());
	}

	private IEnumerator AIRefresh(){
		while (true) {
			GameObject[] enemies = GameObject.FindGameObjectsWithTag("ENEMY");
			foreach(GameObject gObj in enemies){
				if(Vector3.SqrMagnitude(gObj.transform.position - this.transform.position)< Mathf.Pow(attackRange,2)){
					Bullet bullet = Instantiate(bullet_prefab, bulletSpawn.transform.position, Quaternion.identity) as Bullet;
					bullet.Shoot(gObj.transform.position, 1);
					yield return new WaitForSeconds(1f/attackSpeed);
					break;
				}
			}


			yield return new WaitForSeconds(.2f);
		}
	}
}
