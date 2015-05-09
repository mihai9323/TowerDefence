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
	public static SerialPort sp = new SerialPort("COM9", 9600);
	public static string strIn;        
	
	void Update()
	{
		if (sp.IsOpen ) {
			//Read incoming data
			strIn = sp.ReadLine ();
			if (strIn == "1") {
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
