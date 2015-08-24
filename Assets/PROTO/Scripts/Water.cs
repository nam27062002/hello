using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour 
{
	private int CellSpacing;
	
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

	
	void Awake()
	{
		BoxCollider bounds = GetComponent<BoxCollider>();
		
		CellSpacing = (int)Mathf.Max(1, Mathf.Max(bounds.size.x * 0.01f, bounds.size.z * 0.01f));

		int numVertsPerRow = (int)bounds.size.x / CellSpacing;
		int numVertsPerCol = (int)bounds.size.z / CellSpacing;

		MeshRenderer renderer = GetComponent<MeshRenderer>();
		renderer.material.SetFloat("_Width", bounds.size.y);
		renderer.material.SetFloat("_Near", bounds.size.y / 10f);
		renderer.material.SetFloat("_Far", bounds.size.y / 4f);
		renderer.material.SetFloat("_WaveAmplitude", CellSpacing / 2f);

		_NumCellsPerRow = numVertsPerRow - 1;
		_NumCellsPerCol = numVertsPerCol - 1;
		_Width = _NumCellsPerRow * CellSpacing;
		_Depth = _NumCellsPerCol * CellSpacing;
		_NumVertices = numVertsPerRow * numVertsPerCol;
		_NumTriangles = _NumCellsPerRow * _NumCellsPerCol * 2;
		
		
		_Vertices = new Vector3[_NumVertices];
		_Indices = new int[_NumTriangles * 3];
		_UV = new Vector2[_NumVertices];
		_UV2 = new Vector2[_NumVertices];
		_Colours = new Color[_NumVertices];
		
		int StartX = -(_Width / 2);
		int StartZ = -3;
		int EndX = _Width / 2;
		int EndZ = _Depth;

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
				int iIndex = i * numVertsPerRow + j;
								
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
		
		int iBaseIndex = 0;
		
		for(i = 0; i < _NumCellsPerCol; ++i)
		{
			for(int j = 0; j < _NumCellsPerRow; ++j)
			{
				
				_Indices[iBaseIndex] = i * numVertsPerRow + j;
				_Indices[iBaseIndex + 1] = i * numVertsPerRow + j + 1;
				_Indices[iBaseIndex + 2] = (i + 1) * numVertsPerRow + j;
				
				_Indices[iBaseIndex + 3] = (i + 1) * numVertsPerRow + j;
				_Indices[iBaseIndex + 4] = i * numVertsPerRow + j + 1;
				_Indices[iBaseIndex + 5] = (i + 1) * numVertsPerRow + j + 1;
				
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
			
		_Mesh.vertices = _Vertices;
		_Mesh.triangles = _Indices;
		_Mesh.colors = _Colours;
		_Mesh.uv = _UV;
		_Mesh.uv2 = _UV;
		_Position = m_transform.position;
	}
}
