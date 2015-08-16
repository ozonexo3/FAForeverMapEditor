using UnityEngine;
using System.Collections;

public class MarkerScript : MonoBehaviour {

	public	MarkersTypes	Typ;
	public	float			WaterLevel = 0;
	public	Vector2			MapSize = Vector2.zero;

	private	float			LastUpdateTime;

	void Start () {
		UpdateSnap();
	}

	void LateUpdate(){
		if(LastUpdateTime > Time.timeSinceLevelLoad) return;

		UpdateSnap();
		LastUpdateTime = Time.timeSinceLevelLoad + 0.5f;
	}

	public enum MarkersTypes{
		Army, Mex, Hydro
	}

	public void UpdateSnap(){
		//return;
		if(!Terrain.activeTerrain) return;

		Vector3 SnapMarkerPos = transform.position;
		SnapMarkerPos.x *= 10;
		SnapMarkerPos.x -= (SnapMarkerPos.x + 0.5f) % 1;
		SnapMarkerPos.x /= 10.0f;
		
		SnapMarkerPos.z *= 10;
		SnapMarkerPos.z -= (SnapMarkerPos.z + 0.5f) % 1;
		SnapMarkerPos.z /= 10.0f;

		SnapMarkerPos.x += 0.05f;
		SnapMarkerPos.z -= 0.05f;

		SnapMarkerPos.y = Terrain.activeTerrain.SampleHeight(SnapMarkerPos);

		if(MapLuaParser.Water) SnapMarkerPos.y = Mathf.Clamp(SnapMarkerPos.y, WaterLevel, 1024);
		
		transform.position = SnapMarkerPos;
	}
}
