using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		LowerLeftCorner,
		UpperRightCorner,
		AddSquares,
		WhiteBalance,
		RedPick,
		GreenPick,
		BluePick,
		Normalized,
		Hue
	}
	public bool submitData;
	public DetectionType detectionType;
	private static WebCamTexture webCamTexture;
	[Range(0,.15f)]
	public float redHueThreshold;
	[Range(0,1)]
	public float redIThreshold;


	[Range(0,.15f)]
	public float blueHueThreshold;
	[Range(0,1)]
	public float blueIThreshold;

	[Range(0,.15f)]
	public float greenHueThreshold;
	[Range(0,1)]
	public float greenIThreshold;
	[Range(0,5)]
	public int noiseReduction;
	[Range(1,12)]
	public int framesPerSecond;
	[Range(1,20)]
	public int resolutionMod;




	[Range(0,10)]
	public int checkChangeFrames;
	public List<Color[]> previousImages;
	public HSBColor redPicked, greenPicked, bluePicked;
	public string cameraName;
	public Coords2D lowerLeft, upperRight;

	public int minPixelCount = 10;
	public int usedHeight{
		get{
			return upperRight.y-lowerLeft.y;
		}
	}
	public int usedWidth{
		get{
			return upperRight.x-lowerLeft.x;
		}
	}
	private void Start(){
		previousImages = new List<Color[]> ();
		if (string.IsNullOrEmpty (cameraName)) {
			webCamTexture = new WebCamTexture ();
		} else {
			webCamTexture = new WebCamTexture (cameraName);
		}


	
		//this.gameObject.renderer.material.mainTexture = webCamTexture;
		webCamTexture.Play ();

		Tick ();
		StartCoroutine (Refresh ());
		StartCoroutine (UpdateBackend ());
	}

	private IEnumerator Refresh(){
		while(true){
			yield return new WaitForSeconds(1f/framesPerSecond);
			Tick();

		}
	}
	private IEnumerator UpdateBackend(){
		while (true) {
			yield return new WaitForSeconds(2f);
			if(submitData){
				int[] map = GetData ();
				if(ValidateData(map)){
					WebAPI.SendMap(GetData());
					ArduinoInterface.SendData(ArduinoInterface.SendCode.Yellow);
				}else{
					Debug.Log("map not valid");
					ArduinoInterface.SendData(ArduinoInterface.SendCode.Red);
				}


			}
		}
	}
	private bool ValidateData(int[] map){
		bool startFound = false;
		bool finishFound = false;
		int greenTowers=0, blueTowers=0;
		for (int y = 0; y<6; y++) {
			for (int x = 0; x<7; x++) {
				if(map[pos1D(x,y,7)] == 2) greenTowers++;
				if(map[pos1D(x,y,7)] == 3) blueTowers++;
				if(greenTowers>1 || blueTowers>1) return false;
				if (map [pos1D(x,y,7)]==1) {
					int sum = 0;
					if(x>0 && map[pos1D(x-1,y,7)]==1) sum++; 
					if(x<7-1 && map[pos1D(x+1,y,7)]==1) sum++;
					if(y>0 && map[pos1D(x,y-1,7)]==1) sum++;
					if(y<6-1 && map[pos1D (x,y+1,7)]==1) sum++;
					if(sum == 0){
					
						return false;
					}else if(sum == 1){
						if(!startFound){
							startFound = true;

						}else if(!finishFound){
							finishFound = true;

						}else{
						
							return false;
						}
					}else if(sum == 3 || sum == 4){
					

					}
				}
			}
		}
		if (!startFound || !finishFound) {
		
			return false;
		}

		return true;
	}
	public int[] GetData(){
		int[] bytes = new int[squares.Count];
		int c = 0;
		DetectColor ();
		foreach (Square sqr in squares) {
			Color color = sqr.GetMaxColor(minPixelCount);
			bytes[c] = (color == Color.red)?1:(color == Color.green)?2: (color == Color.blue)?3:0;
			c++;
		}
		return bytes;
	}
	void Tick ()
	{
		if (webCamTexture.didUpdateThisFrame) {
			if (detectionType != DetectionType.none && (upperRight.Set && lowerLeft.Set)) {

				DetectColor ();
			} else {
				this.gameObject.renderer.material.mainTexture = webCamTexture;
			}
		}


	}

	private void DetectColor(){
		Color[] pixels = webCamTexture.GetPixels ();
		Color[] newPixels = new Color[pixels.Length/((resolutionMod)*(resolutionMod))];
		int c = 0;
		ResetCounters ();
		for (int y = 0; y< webCamTexture.height/resolutionMod; y++) {
			for(int x= 0; x<webCamTexture.width/resolutionMod; x++){
				if(x*resolutionMod<lowerLeft.x || x*resolutionMod>upperRight.x || y*resolutionMod<lowerLeft.y || y*resolutionMod>upperRight.y) pixels[pos1D (x*resolutionMod,y*resolutionMod,webCamTexture.width)] = Color.white;

				Color pixel = pixels[pos1D(x*resolutionMod,y*resolutionMod,webCamTexture.width)];
				Color adjustedColor = ApplyColorBalance(pixel);
				switch(detectionType){
					case DetectionType.Hue: pixel = HueDetection(adjustedColor); break;
					case DetectionType.Normalized: pixel = NormalizedDetection(adjustedColor); break;
					case DetectionType.WhiteBalance: pixel = adjustedColor; break;
					case DetectionType.RedPick:
					case DetectionType.GreenPick:
					case DetectionType.BluePick: pixel = adjustedColor; break;
				}
				newPixels[pos1D(x,y,webCamTexture.width/(resolutionMod))] = pixel;

			}
		}

		Texture2D texture = new Texture2D (webCamTexture.width/(resolutionMod), webCamTexture.height/(resolutionMod));
		if(noiseReduction!=0 && (detectionType==DetectionType.Hue || detectionType == DetectionType.Normalized))newPixels = Closing (newPixels, texture.width, texture.height, noiseReduction*2+1);
		if (detectionType == DetectionType.Hue && checkChangeFrames>0) {
			if (previousImages.Count > checkChangeFrames) {
				previousImages.RemoveAt (0);
			}
			previousImages.Add (newPixels);
			foreach (Color[] image in previousImages) {
				newPixels = checkPixels (newPixels, image);
			}
		}
		if (minPixelCount > -1 && detectionType == DetectionType.Hue) {
			for (int y = 0; y<webCamTexture.height/resolutionMod; y++) {
				for (int x = 0; x<webCamTexture.width/resolutionMod; x++) {
					AddPixelToCounter(newPixels[pos1D(x,y,webCamTexture.width/resolutionMod)],x*resolutionMod,y*resolutionMod);
				}
			}
			for (int y = 0; y<webCamTexture.height/resolutionMod; y++) {
				for (int x = 0; x<webCamTexture.width/resolutionMod; x++) {
					newPixels [pos1D (x, y, webCamTexture.width / resolutionMod)] = Color.white;
					foreach (Square sqr in squares) {
						if (sqr.ContainsPoint (x*resolutionMod, y*resolutionMod)) {
							newPixels [pos1D (x, y, webCamTexture.width / resolutionMod)] = sqr.GetMaxColor (minPixelCount);
							break;
						}
					}
				}
			}
		}


		texture.SetPixels (newPixels);
		texture.Apply ();
		this.gameObject.renderer.material.mainTexture = texture;

	}
	private Color[] checkPixels(Color[] image1,Color[] image2){
		Color[] result = new Color[image1.Length];
		for (int i = 0; i<image1.Length; i++) {
			if(image1[i] == image2[i]) result[i] = image1[i];
			else result[i] = Color.white;
		}
		return result;
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
			if (hsbColor.b < .3f)
				return Color.white;
			else if (hsbColor.b < .6f)
				return Color.gray;
			else
				return Color.white;
		} else
		if (Mathf.Abs (redPicked.h - hsbColor.h) < redHueThreshold && Mathf.Abs(redPicked.s - hsbColor.s) < redIThreshold ) {
			return Color.red;
		} else if (Mathf.Abs (greenPicked.h - hsbColor.h) < greenHueThreshold && Mathf.Abs(greenPicked.s - hsbColor.s) < greenIThreshold ) {
			return Color.green;
		} else if (Mathf.Abs (bluePicked.h - hsbColor.h) < blueHueThreshold && Mathf.Abs(bluePicked.s - hsbColor.s) < blueIThreshold) {
			return Color.blue;
		} else
			return Color.white;

	}
	private Color[] Closing(Color[] pixels, int w, int h, int kSize){
		return Dilation (
					Erosion (pixels, w, h, kSize),
			w, h, kSize);
	}
	private Color[] Opening(Color[] pixels, int w, int h, int kSize){
		return Erosion (
			Dilation (pixels, w, h, kSize),
			w, h, kSize);
	}
	private Color[] Erosion(Color[] pixels,int w, int h, int kSize){
		Color[] resultPixels = new Color[pixels.Length];
		for (int i = 0; i<resultPixels.Length; i++) {
			resultPixels[i] = Color.white;
		}
		int desiredResult = (kSize) * (kSize);

			for (int y = kSize/2; y<h-kSize/2; y++) {
				for (int x = kSize/2; x<w-kSize/2; x++) {
					if (pixels [pos1D (x, y, w)] != Color.white && pixels [pos1D (x, y, w)] != Color.white) {
					int Rsum = 0;
					int Gsum = 0;
					int Bsum = 0;
					for (int ky = -kSize/2; ky<=kSize/2; ky++) {
						for (int kx = -kSize/2; kx<=kSize/2; kx++) {
							if (pixels [pos1D (x + kx, y + ky, w)] == Color.red)
								Rsum ++;
							else if (pixels [pos1D (x + kx, y + ky, w)] == Color.green)
								Gsum ++;
							else if (pixels [pos1D (x + kx, y + ky, w)] == Color.blue)
								Bsum ++;
							else{
								ky = 1000; // break both for loops
								break;
							}
						}
						if(ky==1000) break;
					}
					if (Rsum < desiredResult - 1 && Gsum < desiredResult - 1 && Bsum < desiredResult - 1) {
						resultPixels [pos1D (x, y, w)] = Color.white;
					} else
						resultPixels [pos1D (x, y, w)] = pixels [pos1D (x, y, w)];
				}
			}
		}
		return resultPixels;
	}
	private Color[] Dilation(Color[] pixels, int w, int h, int kSize){
		Color[] resultPixels = new Color[pixels.Length];
		for (int i = 0; i<resultPixels.Length; i++) {
			resultPixels[i] = Color.white;
		}
		int desiredResult = (kSize) * (kSize);
		for (int y = kSize/2; y<h-kSize/2; y++) {
			for(int x = kSize/2; x<w-kSize/2; x++){
				if (pixels [pos1D (x, y, w)] != Color.white && pixels [pos1D (x, y, w)] != Color.white) {
				int Rsum = 0;
				int Gsum = 0;
				int Bsum = 0;
				for(int ky = -kSize/2; ky<=kSize/2; ky++){
					for(int kx = -kSize/2; kx<=kSize/2; kx++){
						if(pixels[pos1D(x+kx,y+ky,w)] == Color.red) Rsum ++;
						if(pixels[pos1D(x+kx,y+ky,w)] == Color.green) Gsum ++;
						if(pixels[pos1D(x+kx,y+ky,w)] == Color.blue) Bsum ++;
					}
				}
				if(Rsum < 1 && Gsum < 1 && Bsum < 1){
					resultPixels[pos1D(x,y,w)] = Color.white;
				}else{
					int max= Mathf.Max(new int[3]{Rsum,Gsum,Bsum});
					if(max == Rsum){
						resultPixels[pos1D(x,y,w)] = Color.red;
					}else if(max == Gsum){
						resultPixels[pos1D(x,y,w)] = Color.green;
					}else resultPixels[pos1D(x,y,w)] = Color.blue;
				}
				}
			}
		}
		return resultPixels;
	}

	private int pos1D(int x, int y, int width){
		return width * y + x;
	}
	private void OnMouseUp(){
		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay (Input.mousePosition),out hit)){
			Vector3 point = transform.InverseTransformPoint(hit.point) + Vector3.one/2;
			if(detectionType == DetectionType.WhiteBalance){
				WhiteBalanceChange(getPixelAtPoint(point));
			}
			if(detectionType == DetectionType.RedPick){
				redPicked = (new HSBColor(ApplyColorBalance(getPixelAtPoint(point))));
			}
			if(detectionType == DetectionType.BluePick){
				bluePicked = (new HSBColor( ApplyColorBalance(getPixelAtPoint(point))));
			}
			if(detectionType == DetectionType.GreenPick){
				greenPicked = (new HSBColor( ApplyColorBalance(getPixelAtPoint(point))));
			}
			if(detectionType == DetectionType.LowerLeftCorner){
				lowerLeft = getCoords(point);
			}
			if(detectionType == DetectionType.UpperRightCorner){
				upperRight = getCoords(point);
			}
			if(detectionType == DetectionType.AddSquares){
				if(squares == null) squares = new List<Square>();
				if(additionStarted){
					squares[squares.Count-1].max =  getCoords(point);
				}else{
					squares.Add(new Square());
					squares[squares.Count-1].min =  getCoords(point);
				}
				additionStarted = !additionStarted;
			}

			Debug.Log(point);
		}
	}
	private bool additionStarted;

	private Color getPixelAtPoint(Vector2 point){
		return webCamTexture.GetPixel ((int)(point.x * (float)webCamTexture.width), (int)(point.y * (float)webCamTexture.height));
	}
	private Coords2D getCoords (Vector2 point){
		return new Coords2D ((int)(point.x * (float)webCamTexture.width),(int)(point.y * (float)webCamTexture.height));
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
	public List<Square> squares;
	private void AddPixelToCounter(Color pixelColor, int pX, int pY){
		foreach (Square sqr in squares) {
			if(sqr.ContainsPoint(pX,pY)){
				sqr.AddPixel(pixelColor);
				break;
			}
		}
	}
	private void ResetCounters(){
		foreach (Square sqr in squares) {
			sqr.Reset();
		}
	}

}
[System.Serializable]
public class Square{
	public Coords2D min, max;
	public int redPixels, greenPixels, bluePixels;
	public Square(Coords2D min, Coords2D max){
		this.min = min;
		this.max = max;
	}
	public Square(){

	}
	public void Reset(){
		redPixels = greenPixels = bluePixels = 0;
	}
	public void AddPixel(Color pixel){
		if (pixel == Color.red)
			redPixels++;
		if (pixel == Color.green)
			greenPixels++;
		if (pixel == Color.blue) {
			bluePixels++;
		}
	}
	public bool ContainsPoint(Coords2D point){
		return (min.x < point.x && point.x < max.x && min.y < point.y && point.y < max.y);
	}
	public bool ContainsPoint(int x, int y){
		return ContainsPoint(new Coords2D(x,y));
	}
	public Color GetMaxColor(int minNr = 0){
		int max = Mathf.Max (redPixels, greenPixels, bluePixels);
		if (max > minNr) {
			if (max == redPixels)
				return Color.red;
			if (max == greenPixels)
				return Color.green;
			if (max == bluePixels)
				return Color.blue;
		} 
		return Color.white;
	}
}


