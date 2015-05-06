using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	[SerializeField]private float projectileSpeed;
	private Vector3 targetPosition;

	public void Shoot(Vector3 targetPosition, float speed = -1){
		if (speed != -1)
			projectileSpeed = speed;
		this.targetPosition = targetPosition;
		StartCoroutine (shootAtTarget());
	}
	private IEnumerator shootAtTarget(){
		Vector3 initPos = transform.position;
		float ct = 0;
		float distance = Vector3.Distance (this.transform.position, targetPosition);
		while (ct<1) {
			this.transform.position = Vector3.Lerp(initPos,targetPosition,ct);
			ct+= Time.deltaTime * projectileSpeed/distance;
			yield return null;
		}
		Destroy (this.gameObject);
	}
	 
	private void OnCollisionEnter(Collision col){
		if (col.collider.tag == "ENEMY") {
			Enemy enemy = col.gameObject.GetComponent<Enemy> ();
			if(enemy!=null)enemy.health -= 1;
			Destroy (this.gameObject);
		} else {
			Destroy(this.gameObject);
		}
	}
}
