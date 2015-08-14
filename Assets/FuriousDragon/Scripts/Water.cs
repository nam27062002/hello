using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour 
{
	public int NumVertsPerRow;
	public int NumVertsPerCol;
	public int CellSpacing;
	
	private Transform m_transform = null;
	private int _NumCellsPerRow;
	private int _NumCellsPerCol;
	private int _Width;
	private int _Depth;
	private int _NumVertices;
	private int _NumTriangles;
	
	private Mesh _Mesh;
	private Vector3[] _Vertices;
	private Vector2[] _UV;
	private Vector2[] _UV2;
	private int[] _Indices;
	private Color[] _Colours;
	
	protected Vector3 _Position;
	
	// Make copy of
	public Water copyFrom = null;
	
	void Awake()
	{
		_NumCellsPerRow = NumVertsPerRow - 1;
		_NumCellsPerCol = NumVertsPerCol - 1;
		_Width = _NumCellsPerRow * CellSpacing;
		_Depth = _NumCellsPerCol * CellSpacing;
		_NumVertices = NumVertsPerRow * NumVertsPerCol;
		_NumTriangles = _NumCellsPerRow * _NumCellsPerCol * 2;
		
		
		_Vertices = new Vector3[_NumVertices];
		_Indices = new int[_NumTriangles * 3];
		_UV = new Vector2[_NumVertices];
		_UV2 = new Vector2[_NumVertices];
		_Colours = new Color[_NumVertices];
		
		int StartX = -(_Width / 2);
//		int StartZ = _Depth / 2;
		int StartZ = -3;
		int EndX = _Width / 2;
		//int EndZ = -(_Depth / 2);
		int EndZ = _Depth;
		
		//Debug.Log("WATER--------------------> StartZ = " + StartZ + " EndZ = " + EndZ);
		
		float fUI = ((float)(_NumCellsPerRow) * 0.5f) / (float) _NumCellsPerRow;
		float fVI = ((float)(_NumCellsPerCol) * 0.5f) / (float) _NumCellsPerCol;
				
		int i = 0;
		float fDepth = 0.0f;
		float fPerc;
					
		
		float fZ = 0.0f;
		
		float fWaterStep = 0.0f;

		for(int z = StartZ; z <= EndZ; z += CellSpacing)
		{
			int j = 0;
			
			for(int x = StartX; x <= EndX; x += CellSpacing)
			{ 
				int iIndex = i * NumVertsPerRow + j;
								
				float fX = (float)x;
				fZ = (float)z;
				
				_Vertices[iIndex].x = fX;				
				float fDamp;
				
				_Colours[iIndex].g = 1.0f;
				
				if(Mathf.Abs(fZ) < 1.0f && Mathf.Abs(fZ) >= 0.0f)
				{
					_Colours[iIndex].g = 0.0f;
					
				}
				
				
				_Colours[iIndex].r = Mathf.Sin(fWaterStep);
				fWaterStep += 0.01f + Random.Range(0.01f,0.02f);
				_Colours[iIndex].b = 1.0f;
				
				_Vertices[iIndex].y = 1.0f;
				
				_Vertices[iIndex].z = fZ;
				
				//Debug.Log("Water X = " + fX + " Z = " + fZ);
				
				if(z > fZ)
				{
					fZ = z;
				}
								
				_UV[iIndex].x = j * fUI;
				_UV[iIndex].y = i * fVI;
				_UV2[iIndex].x = j * fUI;
				_UV2[iIndex].y = i * fVI;
				
				++j;
			}
			
			fWaterStep += 0.3f + Random.Range(0.1f,1.4f);
			
			++i;
		}
		
		//Debug.Log("------> Z = " + fZ);
		
		int iBaseIndex = 0;
		
		for(i = 0; i < _NumCellsPerCol; ++i)
		{
			for(int j = 0; j < _NumCellsPerRow; ++j)
			{
				
				_Indices[iBaseIndex] = i * NumVertsPerRow + j;
				_Indices[iBaseIndex + 1] = i * NumVertsPerRow + j + 1;
				_Indices[iBaseIndex + 2] = (i + 1) * NumVertsPerRow + j;
				
				_Indices[iBaseIndex + 3] = (i + 1) * NumVertsPerRow + j;
				_Indices[iBaseIndex + 4] = i * NumVertsPerRow + j + 1;
				_Indices[iBaseIndex + 5] = (i + 1) * NumVertsPerRow + j + 1;
				
 				iBaseIndex += 6;
			}
		}
		
	}

	// Use this for initialization
	void Start () 
	{
		m_transform = transform;
		_Position = m_transform.position;
	
		_Mesh = GetComponent<MeshFilter>().mesh;
		_Mesh.Clear();
		
		if( copyFrom == null )
		{
			
			_Mesh.vertices = _Vertices;
			_Mesh.triangles = _Indices;
			_Mesh.colors = _Colours;
			_Mesh.uv = _UV;
			_Mesh.uv2 = _UV;
			_Position = m_transform.position;
		}
		else
		{		
			_Mesh.vertices = copyFrom._Vertices;
			_Mesh.triangles = copyFrom._Indices;
			_Mesh.colors = copyFrom._Colours;
			_Mesh.uv = copyFrom._UV;
			_Mesh.uv2 = copyFrom._UV;
		}
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	//	if(App.paused)
		//	return;
				
		/*if(Camera.main != null)
		{
			float size = _Width * Mathf.Abs( m_transform.localScale.x );
			
			float diff = Camera.main.transform.position.x - _Position.x;
			while( diff <= -size )
			{
				_Position.x -= size * 2;
				diff = Camera.main.transform.position.x - _Position.x;
			}
			while( diff >= size )
			{
				_Position.x += size * 2;
				diff = Camera.main.transform.position.x - _Position.x;
			}
			m_transform.position = _Position;

		}*/
	}
}
