using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]
public class BlockMeter : MonoBehaviour
{
	public Texture2D blockTexture;
	public Texture2D halfTexture;
	public Texture2D emptyTexture;
	public float spacing = 1.0f;
	public bool verticalOrientation = false;
	public bool centerAlignment = false;
	public int countDirection = 1;
	public int rowsOrColumns = 1;
	
	public int maxBlocks = 15;
	private float blockCount = 1.0f;
	private int blocksPerRow;
	private Rect bar_rect;
	
	private float posX;
	private float posY;
	
	void Start()
	{
		posX = transform.position.x * Screen.width;
		posY = transform.position.y * Screen.height;
		
		SetMaxBlocks(maxBlocks);
	}
	
	public void SetBlocks(float input)
	{
		blockCount = input;
	}
	
	public void SetMaxBlocks(int num)
	{
		maxBlocks = Mathf.Max(0, num);
		rowsOrColumns = Mathf.Max(1, rowsOrColumns);
		blocksPerRow = Mathf.CeilToInt(maxBlocks / rowsOrColumns);
	}
	
	void OnGUI()
	{
		GUI.depth = -1;
	    var scale = transform.localScale;
		int i;
		Rect bar_rect;
		
		Texture2D thisTexture;
		float originx;
		float originy;
	
		posX = transform.position.x * Screen.width;
		posY = transform.position.y * Screen.height;
		
		if(centerAlignment)
		{
			if(!verticalOrientation)
			{
			    posX -= (blockTexture.width * spacing * scale.x * maxBlocks) * 0.6f;
			    posY += (blockTexture.height * scale.y) * 0.6f;
			}
		}
	
		if(!verticalOrientation)
		{
			bar_rect = new Rect(posX,
								Screen.height - posY,
								Screen.width,//(blockTexture.width * maxBlocks * scale.x * countDirection) + (spacing * maxBlocks),
								Screen.height);//blockTexture.height * scale.y);
		}
		else {
			bar_rect = new Rect(posX,
								Screen.height - posY,
								Screen.width,//blockTexture.width * scale.x,
								Screen.height);//(blockTexture.height * maxBlocks * scale.y * countDirection) + (spacing * maxBlocks));
		}
	
		if(!verticalOrientation)
		{
			GUI.BeginGroup(bar_rect);
			
			var totalBlockWidth = (blockTexture.width * scale.x) + (spacing * scale.x);
			var totalBlockHeight = blockTexture.height * scale.y;
			
			var row = -1;
			for(i = 1; i <= maxBlocks ; i++) {
				if(i <= blockCount) thisTexture = blockTexture;
				else if((i - 0.5f <= blockCount) && halfTexture != null) thisTexture = halfTexture;
				else thisTexture = emptyTexture;
				
				if((i-1) % blocksPerRow == 0){ row += 1; }
				originx = (((i-1)  % blocksPerRow)) * countDirection * (blockTexture.width * spacing * scale.x);
				originy = (row) * totalBlockHeight;
				
				GUI.DrawTexture(
					new Rect(originx, originy,thisTexture.width * scale.x, thisTexture.height * scale.y),
					thisTexture
				);
			}
			GUI.EndGroup();
		}
		else
		{
			GUI.BeginGroup(bar_rect);
			
			var col = -1;
			for(i = 1; i <= maxBlocks ; i++) {
				if(i <= blockCount) thisTexture = blockTexture;
				else if((i - 0.5f <= blockCount) && halfTexture != null) thisTexture = halfTexture;
				else thisTexture = emptyTexture;
				
				if((i-1) % blocksPerRow == 0){ col += 1; }
				originx = (col) * blockTexture.width * scale.x + (spacing * scale.x);
				originy = ((i-1)  % blocksPerRow) * countDirection * blockTexture.height * scale.y + (spacing * scale.y);
				
				GUI.DrawTexture(new Rect(originx,originy,blockTexture.width * scale.x, blockTexture.height * scale.y), thisTexture);
			}
			GUI.EndGroup();
	
		}
	}
}