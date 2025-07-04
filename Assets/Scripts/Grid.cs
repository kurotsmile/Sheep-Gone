﻿using UnityEngine;

public class Grid : MonoBehaviour {
	
	public bool showGrid;
	public Shader lineShader;

	public float gridSizeX;
	public float gridSizeZ;
	float actualGridSizeX;
	float actualGridSizeZ;
	
	private float offsetY = 0f;
	
	private Material lineMaterial;
	
	private Color mainColor = new Color(125f,125f,125f,1f);

	void Start()
	{
		actualGridSizeX = gridSizeX;
		actualGridSizeZ = gridSizeZ;
	}

	void CreateLineMaterial()
	{		
		if( !lineMaterial ) {
			lineMaterial = new Material(this.lineShader);
			//lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
			 //                           "SubShader { Pass { " +
			  //                          " Blend SrcAlpha OneMinusSrcAlpha " +
			    //                        " ZWrite Off Cull Off Fog { Mode Off } " +
			      //                      " BindChannels {" +
			        //                    " Bind \"vertex\", vertex Bind \"color\", color }" +
			          //                  "} } }" );
			lineMaterial.hideFlags = HideFlags.None;
			lineMaterial.shader.hideFlags = HideFlags.None;}
	}
	
	void OnPostRender()
	{
		CreateLineMaterial();
		lineMaterial.SetPass( 0 );
		GL.Begin( GL.LINES );
		
		if(showGrid)
		{
			GL.Color(mainColor);
			
			//X axis lines
			for(float i = 0; i <= (gridSizeZ + 1); i += 2)
			{
				GL.Vertex3( -1, offsetY, -1 + i);
				GL.Vertex3( gridSizeX, offsetY, -1 + i);
			}
			
			//Z axis lines
			for(float i = 0; i <= (gridSizeX + 1); i += 2)
			{
				GL.Vertex3(-1 + i, offsetY, -1);
				GL.Vertex3( -1 + i, offsetY, gridSizeZ);
			}
		}		
		
		GL.End();
	}

	public void ChangeGridSize(bool x, float val)
	{
		if(x)
		{
			actualGridSizeX += val;
			iTween.ValueTo(gameObject, iTween.Hash("from", gridSizeX, "to", actualGridSizeX, "time", .3f, "onupdate", "ChangeX"));
		}
		else
		{
			actualGridSizeZ += val;
			iTween.ValueTo(gameObject, iTween.Hash("from", gridSizeZ, "to", actualGridSizeZ, "time", .3f, "onupdate", "ChangeZ"));
		}
	}

	//Called when loading an existing level into the level editor
	public void SetGridSize(float x, float y)
	{
		gridSizeX = x;
		gridSizeZ = y;
        actualGridSizeX = x;
        actualGridSizeZ = y;
	}

	void ChangeX(float val)
	{
		gridSizeX = val;
	}

	void ChangeZ(float val)
	{
		gridSizeZ = val;
	}
}