using UnityEngine;
using System.Collections;

public class WebCam : MonoBehaviour {

	public Color whiteBalance;
	public enum ColorType{
		White,
		Gray,
		Black, 
		Red, 
		Green,
		Blue
	}
	public enum DetectionType{
		none,
		WhiteBalance,
		Normalized,
		Hue
	}
	public DetectionType detectionType;
	private static WebCamTexture webCamTexture;
	[Range(0,45)]
	public float colorThreshold;
	[Range(0,5)]
	public int noiseReduction;
	[Range(1,12)]
	public int framesPerSecond;
	[Range(1,20)]
	public int resolutionMod;

	public string cameraName;
	private void Start(){
		if (string.IsNullOrEmpty (cameraName)) {
			webCamTexture = new WebCamTexture ();
		} else {
			webCamTexture = new WebCamTexture (cameraName);
		}


	
		//this.gameObject.renderer.material.mainTexture = webCamTexture;
		webCamTexture.Play ();

		Tick ();
		StartCoroutine (Refresh ());
	}

	private IEnumerator Refresh(){
		while(true){
			yield return new WaitForSeconds(1f/framesPerSecond);
			Tick();
		}
	}

	void Tick ()
	{
		if (webCamTexture.didUpdateThisFrame && detectionType != DetectionType.none)
			DetectColor ();
		else
			if (detectionType == DetectionType.none)
				this.gameObject.renderer.material.mainTexture = webCamTexture;
	}

	private void DetectColor(){
		Color[] pixels = webCamTexture.GetPixels ();
		Color[] newPixels = new Color[pixels.Length/((resolutionMod)*(resolutionMod))];
		int c = 0;
		for (int y = 0; y< webCamTexture.height/resolutionMod; y++) {
			for(int x= 0; x<webCamTexture.width/resolutionMod; x++){
				Color pixel = pixels[pos1D(x*resolutionMod,y*resolutionMod,webCamTexture.width)];
				Color adjustedColor = ApplyColorBalance(pixel);
				switch(detectionType){
					case DetectionType.Hue: pixel = HueDetection(adjustedColor); break;
					case DetectionType.Normalized: pixel = NormalizedDetection(adjustedColor); break;
					case DetectionType.WhiteBalance: pixel = adjustedColor; break;
				}
				newPixels[pos1D(x,y,webCamTexture.width/(resolutionMod))] = pixel;
			}
		}

		Texture2D texture = new Texture2D (webCamTexture.width/(resolutionMod), webCamTexture.height/(resolutionMod));
		if(noiseReduction!=0 && detectionType!=DetectionType.WhiteBalance)newPixels = RemoveNoise (newPixels, texture.width, texture.height, noiseReduction*2+1);
		texture.SetPixels (newPixels);
		texture.Apply ();
		this.gameObject.renderer.material.mainTexture = texture;

	}
	private Color NormalizedDetection(Color pixel){
		Vector3 vc = new Vector3(pixel.r*whiteBalance.r,pixel.g*whiteBalance.g,pixel.b*whiteBalance.b);
		vc.Normalize();
		Color adjustedColor = new Color(vc.x,vc.y,vc.z);

		float max = Mathf.Max(new float[3]{adjustedColor.r,adjustedColor.g,adjustedColor.b});
		return
			max == adjustedColor.r? Color.red:
			max == adjustedColor.g? Color.green:
			max == adjustedColor.b? Color.blue: Color.white;

	}
	private Color HueDetection(Color pixel){
		HSBColor hsbColor = new HSBColor (pixel);
		if (hsbColor.h == 0) {
			if(hsbColor.b <.3f) return Color.black;
			else if(hsbColor.b <.6f) return Color.gray;
			else return Color.white;
		}else
		if (hsbColor.h < (60f-colorThreshold) / 360 || hsbColor.h > (300f+colorThreshold) / 360) {
			return Color.red;
		} else if (hsbColor.h >= (60f+colorThreshold) / 360 && hsbColor.h < (180f-colorThreshold) / 360) {
			return Color.green;
		} else if (hsbColor.h >= (180f+colorThreshold) / 360 && hsbColor.h < (300f-colorThreshold) / 360) {
			return Color.blue;
		} else 
			return Color.white;

	}
	private Color[] RemoveNoise(Color[] pixels,int w, int h, int kSize){
		Color[] resultPixels = new Color[pixels.Length];
		for (int i = 0; i<resultPixels.Length; i++) {
			resultPixels[i] = Color.cyan;
		}
		int desiredResult = (kSize-1) * (kSize-1);
		for (int y = kSize/2; y<h-kSize/2; y++) {
			for(int x = kSize/2; x<w-kSize/2; x++){
				int Rsum = 0;
				int Gsum = 0;
				int Bsum = 0;
				for(int kx = -kSize/2; kx<kSize/2; kx++){
					for(int ky = -kSize/2; ky<kSize/2; ky++){
						if(pixels[pos1D(x+kx,y+ky,w)] == Color.red) Rsum ++;
						if(pixels[pos1D(x+kx,y+ky,w)] == Color.green) Gsum ++;
						if(pixels[pos1D(x+kx,y+ky,w)] == Color.blue) Bsum ++;
					}
				}
				if(Rsum < desiredResult-1 && Gsum < desiredResult-1 && Bsum < desiredResult-1){
					resultPixels[pos1D(x,y,w)] = Color.black;
				}else resultPixels[pos1D(x,y,w)] = pixels[pos1D(x,y,w)];
			}
		}
		return resultPixels;
	}
	private int pos1D(int x, int y, int width){
		return width * y + x;
	}
	private void OnMouseUp(){
		RaycastHit hit;
		if(detectionType == DetectionType.WhiteBalance && Physics.Raycast(Camera.main.ScreenPointToRay (Input.mousePosition),out hit)){
			Vector3 point = transform.InverseTransformPoint(hit.point) + Vector3.one/2;
			WhiteBalanceChange(getPixelAtPoint(point));
			Debug.Log(point);
		}
	}
	private Color getPixelAtPoint(Vector2 point){
		return webCamTexture.GetPixel ((int)(point.x * (float)webCamTexture.width), (int)(point.y * (float)webCamTexture.height));
	}
	private void WhiteBalanceChange(Color c){
		float r = 1f / Mathf.Max (c.r,0.001f);
		float g = 1f / Mathf.Max (c.g,0.001f);
		float b = 1f / Mathf.Max (c.b,0.001f);
		Vector3 v = new Vector3 (r, g, b);//.normalized;

		whiteBalance = new Color (v.x, v.y, v.z);
	}
	private Color ApplyColorBalance(Color originalPixel){
		return new Color (originalPixel.r * whiteBalance.r, originalPixel.g * whiteBalance.g, originalPixel.b * whiteBalance.b);
	}
}


