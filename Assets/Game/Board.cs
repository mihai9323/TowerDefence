using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	[SerializeField] private Tile p_groundTile,p_pathTile;
	[SerializeField] private Stats _stats;
	public Tower tower1,tower2;
	public static Tile[,] GameBoard;
	public EnemyManager enemyManager;
	public static bool built;
	public static int xSize, ySize;
	public static Tile startTile;
	public static Tile endTile;
	public static Stats stats;
	public enum Map
	{
		ReadFromCamera,
		Debug1,
		Debug2,
		Debug3
	}

	private bool[,] debugMap1 = new bool[7,5]{
		{false,false,true,false,false},
		{false,false,true,false,false},
		{false,false,true,true,false},
		{false,false,false,true,false},
		{false,true,true,true,false},
		{false,true,false,false,false},
		{false,true,false,false,false}
	};
	private bool[,] debugMap2 = new bool[7,5]{
		{false,false,true,false,false},
		{false,false,true,false,false},
		{false,false,true,true,false},
		{false,false,false,true,false},
		{false,false,true,true,false},
		{false,false,true,false,false},
		{false,false,true,false,false}
	};
	private bool[,] debugMap3 = new bool[7,5]{
		{true,false,false,false,false},
		{true,false,false,true,true},
		{true,false,true,true,false},
		{true,false,true,false,false},
		{true,false,true,false,false},
		{true,false,true,false,false},
		{true,true,true,false,false}

	};

	[SerializeField] Map mapSource;
	private Map prevMap;

	private void Awake(){
		stats = _stats;
		built = false;
	}
	private void Start(){
		ChooseMap ();
	}

	public void BuildMap(bool[,] map){
		string errorMessage;
		Coords2D startTileCoords, endTileCoords;
		if (ValidateData (out errorMessage, map, out startTileCoords, out endTileCoords)) {
			DestroyPreviousMap();
			xSize = map.GetLength (0);
			ySize = map.GetLength (1);
			GameBoard = new Tile[xSize, ySize];
			for (byte y = 0; y<map.GetLength(1); y++) {
				for (byte x = 0; x<map.GetLength(0); x++) {
					if (map [x, y]) {
						GameBoard [x, y] = AddBlock (p_pathTile, x, y);
						if(startTileCoords.x == x && startTileCoords.y == y){
							startTile = GameBoard[x,y];
							Debug.Log("Start["+x+"]["+y+"]");
						}else if(endTileCoords.x == x && endTileCoords.y == y){
							endTile = GameBoard[x,y];
							Debug.Log("End["+x+"]["+y+"]");
						}
					} else {
						GameBoard [x, y] = AddBlock (p_groundTile, x, y);
					}
				}
			}
			built = true;
			enemyManager.MapChanged();
		} else {
			Debug.LogWarning(errorMessage);
		}
	}
	private void Update(){
		if (prevMap != mapSource) {
			ChooseMap();
		}
	}
	private void ChooseMap(){
		prevMap = mapSource;
		if (mapSource != Map.ReadFromCamera) {
			switch(mapSource){
			case Map.Debug1: BuildMap (debugMap1); PlaceTowers (0, 1, 3, 2); break;
			case Map.Debug2: BuildMap (debugMap2); PlaceTowers (0, 1, 3, 2); break;
			case Map.Debug3: BuildMap (debugMap3); PlaceTowers (1, 2, 4, 3); break;
			}


			
		} else {
			
		}
	}
	private bool ValidateData(out string message, bool[,] map, out Coords2D start, out Coords2D finish){
		bool startFound = false;
		bool finishFound = false;
		start = finish = new Coords2D (255, 255);

		for (int y = 0; y<map.GetLength(1); y++) {
			for (int x = 0; x<map.GetLength(0); x++) {
				if (map [x, y]) {
					byte sum = 0;
					if(x>0 && map[x-1,y]) sum++; 
					if(x<map.GetLength(0)-1 && map[x+1,y]) sum++;
					if(y>0 && map[x,y-1]) sum++;
					if(y<map.GetLength(1)-1 && map[x,y+1]) sum++;
					if(sum == 0){
						message = "not conected tile found!";
						return false;
					}else if(sum == 1){
						if(!startFound){
							startFound = true;
							start = new Coords2D(x,y);
						}else if(!finishFound){
							finishFound = true;
							finish = new Coords2D(x,y);
						}else{
							message = "multiple possible starts or finishes found";
							return false;
						}
					}else if(sum == 3 || sum == 4){
						message = "intersection found";
						return false;
					}
				}
			}
		}
		if (!startFound || !finishFound) {
			message = "start or finish missing possible loop";
			return false;
		}
		message = "all is fine";
		return true;
	}

	public void PlaceTowers(int x1, int y1, int x2, int y2){
		if (built) {
			tower1.transform.position = new Vector3 (x1, 0, y1);
			tower2.transform.position = new Vector3 (x2, 0, y2);


		} else {
			Debug.LogWarning("Map not built yet");
		}
	}

	private Tile AddBlock(Tile prefab, int x, int y){
		Tile t = Instantiate (prefab, new Vector3 (x, 0, y), Quaternion.identity) as Tile;
		t.transform.parent = this.transform;
		t.Configure (x, y);
		return t;
	}
	private void DestroyPreviousMap(){
		if (GameBoard != null) {
			enemyManager.StopAll();
			for(int y = 0; y<ySize; y++){
				for(int x = 0; x<xSize; x++){
					Destroy(GameBoard[x,y].gameObject);
				}
			}
		}

	}
}
[System.Serializable]
public class Coords2D{
	public int x,y;
	public Coords2D(int x,int y){
		this.x = x;
		this.y = y;
	}
	public bool Set{
		get{
			return x != 0 || y != 0;
		}
	}
}