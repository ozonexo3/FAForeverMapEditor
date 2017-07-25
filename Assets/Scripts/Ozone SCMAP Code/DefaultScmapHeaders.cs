using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultScmapHeaders : MonoBehaviour {

	public static DefaultScmapHeaders Current;


	public GetGamedataFile.HeaderClass PreviewTextHeader;
	public GetGamedataFile.HeaderClass TextureMapHeader;
	public GetGamedataFile.HeaderClass TextureMap2Header;
	public GetGamedataFile.HeaderClass NormalmapHeader;
	public GetGamedataFile.HeaderClass WatermapHeader;

	void Awake () {
		Current = this;
	}
	
}
