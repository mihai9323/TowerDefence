using UnityEngine;
using System.Collections;
using Newtonsoft.Json;


public class WebAPI : MonoBehaviour {

	private const string host = "http://jocoi.hostreo.com/";
	private static WebAPI s_instance;
	private void Awake(){
		s_instance = this;
	}
	public static void SendMap(int[] map){
		string sMap = JsonConvert.SerializeObject (map);
		WWW www = new WWW (host+"updateMap.php?MAP="+sMap);
		s_instance.StartCoroutine(s_instance.UploadData(www));
	}
	public static void GetMap(OnDownloadMapResponse response){

		WWW www = new WWW (host+"getData.php?ID=0");
		s_instance.StartCoroutine(s_instance.DownloadData(www,response));
	}
	public static void SendWave(){
		WWW www = new WWW (host + "sendWave.php");
		s_instance.StartCoroutine (s_instance.UploadData (www));
	}
	public static void GetWave(OnGetWaveResponse response){
		WWW www = new WWW (host + "getWave.php");
		s_instance.StartCoroutine (s_instance.GetWaveData (www,response));
	}	

	private IEnumerator UploadData(WWW www){
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.data);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}  
		
	}

	public delegate void OnGetWaveResponse(bool newWave);
	public delegate void OnDownloadMapResponse(byte[] map);

	private IEnumerator DownloadData(WWW www,OnDownloadMapResponse response){
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			string data = www.data.Split(new char[1]{'|'})[0];
			byte[] map = JsonConvert.DeserializeObject<byte[]>(data);
			Debug.Log(data);
			response(map);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}  
		
	}
	private IEnumerator GetWaveData(WWW www,OnGetWaveResponse response){
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			string data = www.data.Split(new char[1]{'|'})[0];
			bool dataResponse= JsonConvert.DeserializeObject<bool>(data);

			response(dataResponse);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}  
		
	}
}

