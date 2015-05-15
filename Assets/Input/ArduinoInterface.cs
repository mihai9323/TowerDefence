using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;



public class ArduinoInterface : MonoBehaviour {
	public enum SendCode{
		None,
		Yellow,
		Red
	}
	//Setup parameters to connect to Arduino
	public static SerialPort sp;
	public static string strIn;        
	public string COM = "COM9";
	void Awake(){
		sp = new SerialPort(COM, 9600);
	}
	void Update()
	{
		if (sp.IsOpen ) {
			//Read incoming data
			strIn = sp.ReadLine ();
			if (strIn == "1") {
				Debug.Log(strIn);
				WebAPI.SendWave();
			}
		
			//You can also send data like this
			//sp.Write("1");
		}
		
	}

	public static void SendData(SendCode sendCode){
		if (sp.IsOpen)
			sp.Write (((int)sendCode).ToString());
		else
			OpenConnection ();
	}

	//Function connecting to Arduino
	public static void OpenConnection() 
	{
		if (sp != null) 
		{
			if (sp.IsOpen) 
			{
				sp.Close();

			}
			else 
			{
				sp.Open();  // opens the connection
				sp.ReadTimeout = 50;  // sets the timeout value before reporting error
			
			}
		}
		else 
		{
			if (sp.IsOpen)
			{
			
			}
			else 
			{
			
			}
		}
	}
	
	void OnApplicationQuit() 
	{
		sp.Close();
	}
}
