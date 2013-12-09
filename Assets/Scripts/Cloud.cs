﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Cloud : MonoBehaviour {

	#region Attributes
	public Material raindropMaterial;
	public Transform raindropRoot;
	[Range(0, 1)]public float growthRate;
	[Range(0, 1)]public float rainRate;
	[Range(0, 5)]public float maxSize = 2f;
	[Range(0, 1)]public float darkestGrey = .25f;
	[Range(0, 100)]public float dropsPerFrameMax = .05f;
	[Range(0, 100)]public float dropsPerFrameMin = 1f;
	public float raindropTopEdge = 3.7f;
	public float raindropBottomEdge = -2.4f;
	public float raindropAreaMaxWidth = 3.5f;
	public float raindropAreaMinWidth = .5f;
	[Range(0, 1)]public float bottomRaindropThickness = .1f;
	[Range(0, 1)]public float topRaindropThickness = .5f;
	[Range(0, 1)]public float minRaindropLength = .1f;
	[Range(0, 3)]public float maxRaindropLength = 1f;
	[Range(0, 1)]public float raindropLengthVariety = .01f;
	[Range(0, 10)]public float raindropSpeed = 1f;
	[Range(0, 2)]public float startSize = 1f;
	#endregion

	#region Unity
	void Awake()
	{
		transform.localScale = new Vector3(startSize, startSize, 1);
		mainCam = Camera.main;
		spriteRenderer = GetComponent<SpriteRenderer>();
		raindrops = new List<VectorLine>();
		raindropTimers = new List<float>();
		raindropLengths = new List<float>();
	}
	
	void Update()
	{
		currentScale = transform.localScale;
		cloudPercentage = currentScale.x/maxSize;
		float cloudGrey = Mathf.Lerp(1, darkestGrey, cloudPercentage);
		spriteRenderer.color = new Color(cloudGrey, cloudGrey, cloudGrey);
		
		raining = false;
		#if UNITY_EDITOR
		if (Input.GetMouseButton(0))
		{
			ProcessTouch(Input.mousePosition);
		}
		else
		#else
		#if UNITY_ANDROID || UNITY_IPHONE
		if (Input.touches.Length > 0)
		{
			firstTouch = Input.touches[0];
			if (firstTouch.phase != TouchPhase.Ended && firstTouch.phase != TouchPhase.Canceled)
			{
				ProcessTouch(firstTouch.position);
			}
		}
		else
		#endif
		#endif
		{
			if (currentScale.x < maxSize)
			{
				float grow = Time.deltaTime * growthRate;
				ScaleCloud(grow);
			}
		}
		ProcessRain();
	}
	
	#endregion
	
	#region Private
	private const int MAX_RAIN_DROPS = 500;
	private Camera mainCam;
	private Vector3 currentScale;
	private bool raining;
	private SpriteRenderer spriteRenderer;
	private float cloudPercentage;
	private List<VectorLine> raindrops;
	private List<float> raindropTimers;
	private List<float> raindropLengths;
	Touch firstTouch;
	
	private void ProcessTouch(Vector2 coordinates)
	{
		if (currentScale.x > 0)
		{
			Ray ray = mainCam.ScreenPointToRay(coordinates);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				raining = true;
				float rain = -Time.deltaTime * rainRate;
				ScaleCloud(rain);
			}
		}
	}
	
	private void ScaleCloud(float scale)
	{
		transform.localScale = new Vector3(currentScale.x + scale, currentScale.y + scale, 1);
	}
	
	private void ProcessRain()
	{
		int raindropsCount = raindrops.Count;
		if (raining)
		{
			float rand = Random.value;
			float numberOfDrops = Mathf.Lerp(dropsPerFrameMin, dropsPerFrameMax, cloudPercentage);
			while (numberOfDrops > rand)
			{
				SpawnRainDrop();
				numberOfDrops--;
			}
		}
		float deltaTime = Time.deltaTime;
		for(int i=0; i < raindropsCount; i++)
		{
			float raindropTimer = raindropTimers[i] += deltaTime;
			float raindropTime = raindropLengths[i] / raindropSpeed;
			if(raindropTimer < raindropTime / 2)
			{
				float bottomDistance = deltaTime/raindropTime * raindropLengths[i];
				raindrops[i].points2[1].y -= bottomDistance/2;
				raindrops[i].points2[2].y -= bottomDistance;
				raindrops[i].Draw();
			}
			else if (raindropTimer < raindropTime)
			{
				float topDistance = deltaTime/raindropTime * raindropLengths[i];
				raindrops[i].points2[1].y -= topDistance/2;
				raindrops[i].points2[0].y -= topDistance;
				raindrops[i].Draw();
			}
			else
			{
				VectorLine line = raindrops[i];
				VectorLine.Destroy(ref line);
				raindrops.RemoveAt(i);
				raindropTimers.RemoveAt(i);
				raindropLengths.RemoveAt(i);
				raindropsCount--;
			}
		}
	}
	
	private void SpawnRainDrop()
	{
		float width = Mathf.Lerp(raindropAreaMinWidth, raindropAreaMaxWidth, cloudPercentage);
		float xCoor = Random.Range(-width, width);
		float yCoor = Random.Range(raindropBottomEdge, raindropTopEdge);
		float length = Mathf.Lerp(minRaindropLength, maxRaindropLength, cloudPercentage)
			* (1 + Random.Range(-raindropLengthVariety, raindropLengthVariety));
		Vector2 startPoint = new Vector2(xCoor, yCoor);
			   Vector2 endPoint = new Vector2(xCoor, yCoor);
		Vector2[] points = {startPoint, (startPoint + endPoint)/2, endPoint};
		VectorLine drop = new VectorLine("Rainddrop", points, raindropMaterial, bottomRaindropThickness, LineType.Continuous);
		drop.smoothWidth = true;
		drop.SetWidths(new float[]{topRaindropThickness, bottomRaindropThickness});
		drop.vectorObject.transform.parent = raindropRoot;
		Vector3 pos = drop.vectorObject.transform.localPosition;
		pos.y = 0;
		drop.vectorObject.transform.localPosition = pos;
		drop.Draw();
		raindrops.Add(drop);
		raindropTimers.Add(0);
		raindropLengths.Add(length);
	}
	#endregion
}