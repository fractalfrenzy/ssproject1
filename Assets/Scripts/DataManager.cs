﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class DataManager : SingletonMonoBehaviour<DataManager> {

	#region Attributes
	public Cloud cloud;
	public Plant plant;
	public float heightLoaded;
	public float cloudSizeLoaded;
	public float saturationLoaded;
	public List<Vector2> curvePointsLoaded;
	public List<int> segmentsLoaded;
	public int stemNextHeightLoaded;
	public List<byte> stemLineFlagsLoaded;
	public List<ushort> stemHeightsLoaded;
	public List<float> stemLengthsLoaded;
	public List<Vector2> stemCurvePointsLoaded;
	public List<float> flowerGrowthStatesLoaded;
	public float timeUntilStemDeathLoaded;
	#endregion

	#region Unity
	void Awake ()
	{
		im = ItemManager.Instance;
		filePath = Application.persistentDataPath + "/saplings.data";
		Debug.Log ("filePath: " + filePath);
		plantCurvePoints = new List<byte[]>();
		segments = new List<byte[]>();
		stemCurvePoints = new List<byte[]>();
		collectablesToStore = new List<ushort>();
		stemHeightsLoaded = new List<ushort>();
		stemLineFlagsLoaded = new List<byte>();
		data = new List<byte>();
	}
	
	void Start()
	{
		if (File.Exists(filePath))
		{
			Debug.Log("file exists. loading...");
			LoadData();
		}
		else
		{
			Debug.Log("no save file found.");
		}	
	}
	
	
	#endregion
	
	#region Actions
	public void Reset()
	{
		numberOfCurves = 0;
		plantCurvePoints = new List<byte[]>();
		stemCurvePoints = new List<byte[]>();
		segments = new List<byte[]>();
	}
	
	public void StoreCurve(Vector3[] curvePoints, ushort segment)
	{
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[0].x));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[0].y));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[1].x));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[1].y));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[2].x));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[2].y));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[3].x));
		plantCurvePoints.Add(BitConverter.GetBytes(curvePoints[3].y));
		segments.Add(BitConverter.GetBytes(segment));
		numberOfCurves++;
	}
	
	public void StoreStem(Vector3[] curvePoints)
	{
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[0].x));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[0].y));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[1].x));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[1].y));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[2].x));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[2].y));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[3].x));
		stemCurvePoints.Add(BitConverter.GetBytes(curvePoints[3].y));
	}
	
	public void RemoveStem(int stemIndex)
	{
		stemCurvePoints.RemoveRange(stemIndex*8, 8);
	}
	
	public void StoreCollectableIndex(ushort index)
	{
		Debug.Log ("storing collectable index: " + index);
		collectablesToStore.Add(index);
	}
	
	public void SaveData()
	{
		data.Clear();
		/*file version # (2 bytes)
		cloud size
		plant height (4 bytes), plant saturation (4 bytes), number of curves (2 bytes), curve segments (2 bytes each), curve control points (4 bytes each, 8 per curve),
		stemHeight (4 bytes), number of stems (2 bytes), stem plant line flag: 0 = current line, 1 = previous line, 2 = lower (1 byte each)
		individual stem's height (2 bytes each), stem length(4 bytes each), stem control points (4 bytes each, 8 per curve)
		flower growth state (4 bytes each, postive: flower size; negative: growth counter, 0: budded), time until stem death (4 bytes)
		inventory: 		powerups-- number of powerups(2 bytes); for each powerup: time remaining (4 bytes), quanity (2 bytes), piece quanities (2 bytes each)
					  	collectables: number stored (2 bytes); index (2 bytes), quantity (2 bytes), piece quantities (2 bytes each)
		*/
		
		byte[] fileVersionBytes = BitConverter.GetBytes(FILE_VERSION);
		data.Add(fileVersionBytes[0]);
		data.Add(fileVersionBytes[1]);
		
		byte[] cloudSize = BitConverter.GetBytes(cloud.Size);
		for(int b=0; b<4; b++)
			data.Add(cloudSize[b]);
		
		byte[] heightBytes = BitConverter.GetBytes(plant.Height);
		for(int b=0; b<4; b++)
			data.Add(heightBytes[b]);
		
		byte[] saturationBytes = BitConverter.GetBytes(plant.Saturation);
		for(int b=0; b<4; b++)
			data.Add(saturationBytes[b]);
			
		Debug.Log ("storing numberOfCurves as " + numberOfCurves);
		byte[] numberOfCurvesBytes = BitConverter.GetBytes(numberOfCurves);
		data.Add(numberOfCurvesBytes[0]);
		data.Add(numberOfCurvesBytes[1]);
		
		for(int i=0; i<numberOfCurves; i++)
		{
			for(int b=0; b<2; b++)
				data.Add(segments[i][b]);
		}

		for(int i=0; i <plantCurvePoints.Count; i++)
		{
			for(int b=0; b<4; b++)
				data.Add(plantCurvePoints[i][b]);
		}
		
		byte[] nextStemHeightBytes = BitConverter.GetBytes(plant.NextStemHeight);
		for(int b=0; b<4; b++)
			data.Add(nextStemHeightBytes[b]);
		
		float[] stemLengths = plant.StemLengths;
		uint numberOfStems = (uint)plant.StemLengths.Length;
		Debug.Log ("storing numberOfStems as " + numberOfStems);
		byte[] numberOfStemsBytes = BitConverter.GetBytes(numberOfStems);
		
		data.Add(numberOfStemsBytes[0]);
		data.Add(numberOfStemsBytes[1]);
		
		byte[] stemLineFlags = plant.StemLineFlags;
		ushort[] stemHeights = plant.StemHeights;
		
		for(int i=0; i<numberOfStems; i++)
		{
			data.Add(stemLineFlags[i]);
			
			byte[] stemHeight = BitConverter.GetBytes(stemHeights[i]);
			
			data.Add(stemHeight[0]);
			data.Add(stemHeight[1]);
		
			byte[] stemLengthsBytes = BitConverter.GetBytes(stemLengths[i]);
			for(int b=0; b<4; b++)
				data.Add(stemLengthsBytes[b]);
		}
		
		Debug.Log ("stemCurvePoints.Count: " + stemCurvePoints.Count);
		
		for(int i=0; i <stemCurvePoints.Count; i++)
		{
			for(int b=0; b<4; b++)
				data.Add(stemCurvePoints[i][b]);
		}
		
		float[] flowerGrowthStates = plant.FlowerGrowthStates;
		for(int i=0; i<numberOfStems; i++)
		{
			byte[] flowerGrowthStatesBytes = BitConverter.GetBytes(flowerGrowthStates[i]);
			for(int b=0; b<4; b++)
				data.Add(flowerGrowthStatesBytes[b]);
		}
		
		byte[] timeUntilStemDeathBytes = BitConverter.GetBytes(plant.TimeUntilStemDeath);
		for(int b=0; b<4; b++)
			data.Add(timeUntilStemDeathBytes[b]);
		
		ushort numberOfPowerups = (ushort)im.powerups.Count;
		byte[] numberOfPowerupsBytes = BitConverter.GetBytes(numberOfPowerups);
		data.Add(numberOfPowerupsBytes[0]);
		data.Add(numberOfPowerupsBytes[1]);
		Debug.Log ("storing numberOfPowerups as " + numberOfPowerups);
		for(int i=0; i <numberOfPowerups; i++)
		{
			byte[] timeRemainingBytes = BitConverter.GetBytes(im.powerups[i].powerupTimeRemaining);
			for(int b=0; b<4; b++)
				data.Add(timeRemainingBytes[b]);
			
			byte[] quantityBytes = BitConverter.GetBytes((ushort)im.powerups[i].inventory);
			data.Add(quantityBytes[0]);
			data.Add(quantityBytes[1]);
			
			int numberOfPieces = im.powerups[i].pieces.Length;
			for(int p=0; p<numberOfPieces; p++)
			{
				byte[] pieceQuantityBytes =  BitConverter.GetBytes((uint)im.powerups[i].pieces[p].inventory);
				data.Add(pieceQuantityBytes[0]);
				data.Add(pieceQuantityBytes[1]);
			}
		}
		
		//*collectables: number stored (2 bytes); for each: index (2 bytes), quantity (2 bytes), piece quantities (2 bytes each)
		ushort numOfCollectablesToStore = (ushort)collectablesToStore.Count;
		byte[] numOfCollectablesToStoreBytes = BitConverter.GetBytes(numOfCollectablesToStore);
		data.Add(numOfCollectablesToStoreBytes[0]);
		data.Add(numOfCollectablesToStoreBytes[1]);
		for(int i=0; i<numOfCollectablesToStore; i++)
		{
			ushort collectableIndex = collectablesToStore[i];
			byte[] indexBytes = BitConverter.GetBytes(collectableIndex);
			data.Add(indexBytes[0]);
			data.Add(indexBytes[1]);
			
			byte[] quantityBytes = BitConverter.GetBytes((ushort)im.collectables[collectableIndex].inventory);
			data.Add(quantityBytes[0]);
			data.Add(quantityBytes[1]);
			
			int numberOfPieces = im.collectables[collectableIndex].pieces.Length;
			for(int p=0; p<numberOfPieces; p++)
			{
				byte[] pieceQuantityBytes =  BitConverter.GetBytes((uint)im.collectables[collectableIndex].pieces[p].inventory);
				data.Add(pieceQuantityBytes[0]);
				data.Add(pieceQuantityBytes[1]);
			}
			
		}
		
		
		File.WriteAllBytes(filePath, data.ToArray());
		Debug.Log ("data.Count: " + data.Count);
	}
	#endregion
	
	#region Private
	private const ushort FILE_VERSION = 1;
	private ItemManager im;
	private string filePath;
//	private string data;
	private List<byte> data;
	private byte[] dataLoaded;
	private List<byte[]>plantCurvePoints;
	private List<byte[]>segments;
	private List<byte[]>stemCurvePoints;
	private List<byte[]>stemLengths;
	private ushort numberOfCurves = 0; //total number of curves
	private ushort numberOfCurvesLoaded = 0; //total number of curves
	private int nextStemHeight;
	private ushort numberOfStemsLoaded = 0;
	private List<ushort>collectablesToStore;
	
	private void LoadData()
	{
		int index = 0;		
		dataLoaded = File.ReadAllBytes(filePath);
		Debug.Log ("dataLoaded.Length: " + dataLoaded.Length);
		
		uint fileVersionLoaded = BitConverter.ToUInt16(dataLoaded, index);
		index += 2;
		
		if (fileVersionLoaded == FILE_VERSION)
			Debug.Log ("file version is good.");
		else
			Debug.LogError("file version has changed!");
		
		cloudSizeLoaded = BitConverter.ToSingle(dataLoaded, index);
		cloud.Size = cloudSizeLoaded;
		index += 4;
		
		heightLoaded = BitConverter.ToSingle(dataLoaded, index);
		index += 4;
		
		saturationLoaded = BitConverter.ToSingle(dataLoaded, index);
		index += 4;
		
		numberOfCurvesLoaded = BitConverter.ToUInt16(dataLoaded, index);
		index += 2;
		
		for(int i=0; i<numberOfCurvesLoaded; i++)
		{
			segmentsLoaded.Add(BitConverter.ToUInt16(dataLoaded, i*2 + index));
		}
		index += numberOfCurvesLoaded * 2;
		
		
		for(int i=0; i<numberOfCurvesLoaded*4; i++)
		{
			curvePointsLoaded.Add(new Vector2(BitConverter.ToSingle(dataLoaded, i*8 + index), BitConverter.ToSingle(dataLoaded, i*8 + 4 + index)));
		}
		
		index += numberOfCurvesLoaded * 32;
		
		
		stemNextHeightLoaded = BitConverter.ToInt32(dataLoaded, index);
		index += 4;
		Debug.Log ("stemHeightLoaded: " + stemNextHeightLoaded);
		
		numberOfStemsLoaded = BitConverter.ToUInt16(dataLoaded, index);
		Debug.Log ("numberOfStemsLoaded: " + numberOfStemsLoaded);
		index += 2;
		
		for(int i=0; i<numberOfStemsLoaded; i++)
		{
			stemLineFlagsLoaded.Add(dataLoaded[i*7 + index]);
			stemHeightsLoaded.Add(BitConverter.ToUInt16(dataLoaded, i*7 + 1 + index));
			stemLengthsLoaded.Add(BitConverter.ToSingle(dataLoaded, i*7 + 3 + index));
		}
		
		index += numberOfStemsLoaded * 7;
		
		
		for(int i=0; i<numberOfStemsLoaded*4; i++)
		{
			stemCurvePointsLoaded.Add(new Vector2(BitConverter.ToSingle(dataLoaded, i*8 + index), BitConverter.ToSingle(dataLoaded, i*8 + 4 + index)));
		}
		
		index += numberOfStemsLoaded * 32;
		
		for(int i=0; i<numberOfStemsLoaded; i++)
		{
			flowerGrowthStatesLoaded.Add(BitConverter.ToSingle(dataLoaded, i*4 + index));
		}
		
		index += numberOfStemsLoaded*4;
		
		timeUntilStemDeathLoaded = BitConverter.ToSingle(dataLoaded, index);
		index += 4;
		
		ushort numberOfPowerupsLoaded = BitConverter.ToUInt16(dataLoaded, index);
		Debug.Log ("numberOfPowerupsLoaded: " + numberOfPowerupsLoaded);
		index += 2;
		for(int i=0; i < numberOfPowerupsLoaded; i++)
		{
			Debug.Log ("i: " + i);
			im.powerups[i].powerupTimeRemaining = BitConverter.ToSingle(dataLoaded, i*6 + index);
			im.powerups[i].inventory = BitConverter.ToUInt16(dataLoaded, i*6 + 4 + index);
			int numberOfPieces = im.powerups[i].pieces.Length;
			for(int p=0; p<numberOfPieces; p++)
			{
				im.powerups[i].pieces[p].inventory = BitConverter.ToUInt16(dataLoaded, i*6 + 6 + index);
				index += 2;
			}
		}
		index += numberOfPowerupsLoaded*6;
		
		//*collectables: number stored (2 bytes); for each: index (2 bytes), quantity (2 bytes), piece quantities (2 bytes each)
		ushort numberOfCollectablesLoaded = BitConverter.ToUInt16(dataLoaded, index);
		index += 2;
		for(int i=0; i < numberOfCollectablesLoaded; i++)
		{
			int colIndex = (int)BitConverter.ToUInt16(dataLoaded, i*4 + index);
			im.collectables[colIndex].inventory = BitConverter.ToUInt16(dataLoaded, i*4 + 2 + index);
			int numberOfPieces = im.collectables[colIndex].pieces.Length;
			for(int p=0; p<numberOfPieces; p++)
			{
				im.collectables[colIndex].pieces[p].inventory = BitConverter.ToUInt16(dataLoaded, i*4 + 4 + index);
				index += 2;
			}
		}
	}
	
	#endregion
}
