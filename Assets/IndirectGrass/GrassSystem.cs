using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GrassSystem : MonoBehaviour
{
    [SerializeField]
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundInfo
    {
        public Vector3 _boundsCenter;
        public Vector3 _boundsExtents;
    }
    
    [Range(0, 1)]
    public float clipPosZ = 0;
    
    public Camera MainCamera;
    public Material Material;
    public ComputeShader ComputeShader;
    [Header("Grass")]
    public float Density = 1f;
    public float MaxExtent = 5f;

    [Header("Grass Age")]
    [Range(1, 10)]
    public int AgeNoiseColumns = 5;
    [Range(1, 10)]
    public int AgeNoiseRows = 5;
    public Color YoungGrassColor;
    public Color OldGrassColor;

    [Header("Wind")]
    public Vector3 WindDirection = new Vector3(0, 0, 1);
    [Range(0, 0.5f)]
    public float WindForce = 0.05f;
    [Range(0.1f, 15f)]
    public float WindVelocity = 2f;

    [Range(1, 10)]
    public int WindNoiseColumns = 5;
    [Range(1, 10)]
    public int WindNoiseRows = 5;

    [Range(0f, 10f)]
    public float fadeDis = 0f;

    private List<GrassBlade> _grassBlades;
    private ComputeBuffer _grassBladesBuffer;

    private int _kernelIndex;
    private uint _threadGroupsCountX;

    // for Graphics.DrawMeshInstancedIndirect
    private ComputeBuffer _argsBuffer;
    private Bounds _bounds;
    //private Mesh _mesh;
    private int _grassBladesCount;

    private bool _isInitialized = false;

    private uint[] _indicesCheck = null;
    private ComputeBuffer _indicesBuffer;

    private BoundInfo _boundInfo;
    private ComputeBuffer _boundInfoBuffer;

    private ComputeBuffer _OutputGrassBladesBuffer;

    private List<GrassBlade> list;

    public Mesh _mesh;
    
    private List<Vector3> _allPosition;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //_mesh = GrassFactory.GetGrassBladeMesh();

        while (MainCamera == null)
        {
            MainCamera = Camera.main;
            yield return null;
        }
            
        
        _boundInfo = new GrassSystem.BoundInfo()
        {
            _boundsCenter = _mesh.bounds.center,
            _boundsExtents = _mesh.bounds.extents
        };
        
        var o = new GameObject("grass", typeof(MeshFilter));
        var meshFilter = o.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = GrassFactory.GetGrassBladeMesh();

        InitializeGrassBlades();
        InitializeGrassBladesBuffer();
        InitializeIndirectArgsBuffer();
        InitializeThreadGroupsSize();
        SetOneTimeValues();

        _isInitialized = true;
    }

    private void InitializeGrassBlades()
    {
        GrassFactory.RaycastGrassBlades(
            transform: transform,
            meshFilter: GetComponent<MeshFilter>(),
            maxExtent: MaxExtent,
            density: Density,
            bounds: out _bounds,
            grassBlades: out _grassBlades,
            allPosition: out _allPosition
        );
    }

    private void InitializeGrassBladesBuffer()
    {
        var grassBladeMemorySize = (3 + 1 + 1) * sizeof(float);

        Debug.Log(_grassBlades.Count);

        if (_grassBlades.Count == 0)
            return;
        
        _grassBladesBuffer = new ComputeBuffer(
            count: _grassBlades.Count,
            stride: grassBladeMemorySize//_grassBlades.Length * grassBladeMemorySize
        );

        _indicesCheck = new uint[_grassBlades.Count];

        _indicesBuffer = new ComputeBuffer(
            count: _indicesCheck.Length,
            stride: sizeof(uint)
        );

        _boundInfoBuffer = new ComputeBuffer(
            count: 1,
            stride: 6 * 4
        );

        _OutputGrassBladesBuffer = new ComputeBuffer(
            count:_grassBlades.Count,
            stride:grassBladeMemorySize,
            type:ComputeBufferType.Append
            );
            
        
        _grassBladesBuffer.SetData(_grassBlades);
        
        _indicesBuffer.SetData(_indicesCheck);

        BoundInfo[] boundArray = new BoundInfo[1] { _boundInfo};
        
        _boundInfoBuffer.SetData(boundArray);

        _kernelIndex = ComputeShader.FindKernel("SimulateGrass");

        // this will let compute shader access the buffers
        ComputeShader.SetBuffer(_kernelIndex, "GrassBladesBuffer", _grassBladesBuffer);
        ComputeShader.SetBuffer(_kernelIndex, "IndicesBuffer", _indicesBuffer);
        ComputeShader.SetBuffer(_kernelIndex, "boundInfo", _boundInfoBuffer);

        // this will let the surface shader access the buffer
        //Material.SetBuffer("GrassBladesBuffer", _grassBladesBuffer);
    }

    private void InitializeIndirectArgsBuffer()
    {
        const int _argsCount = 5;

        _argsBuffer = new ComputeBuffer(
            count: 1,
            stride: _argsCount * sizeof(uint),
            type: ComputeBufferType.IndirectArguments
        );

        // for Graphics.DrawMeshInstancedIndirect
        // this will be used by the vertex/fragment shader
        // to get the instance_id and vertex_id
        var args = new int[_argsCount] {
            (int)_mesh.GetIndexCount(submesh: 0),       // indices of the mesh
            _grassBladesCount,                          // number of objects to render
            0,0,0                                       // unused args
        };

        _argsBuffer.SetData(args);
    }

    private void InitializeThreadGroupsSize()
    {
        // calculate amount of thread groups
        uint threadGroupSizeX;
        ComputeShader.GetKernelThreadGroupSizes(_kernelIndex, out threadGroupSizeX, out _, out _);
        _threadGroupsCountX = (uint)_grassBlades.Count / threadGroupSizeX;
    }

    private void SetOneTimeValues()
    {
        ComputeShader.SetVector("GrassSize", _bounds.size);
        ComputeShader.SetVector("GrassOrigin", transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isInitialized || _grassBlades.Count == 0)
        {
            return;
        }

        _OutputGrassBladesBuffer.SetCounterValue(0);
       
        ComputeShader.SetBuffer(_kernelIndex, "OutputGrassBladesBuffer", _OutputGrassBladesBuffer);
        ComputeShader.SetFloat("Time", Time.time);
        ComputeShader.SetFloat("fadeDis", fadeDis);
        ComputeShader.SetInt("WindNoiseColumns", WindNoiseColumns);
        ComputeShader.SetInt("WindNoiseRows", WindNoiseRows);
        ComputeShader.SetFloat("WindVelocity", WindVelocity);
        ComputeShader.SetVector("CameraPosition", MainCamera.transform.position);
        ComputeShader.SetMatrix("ViewMatrix", MainCamera.projectionMatrix * MainCamera.worldToCameraMatrix);


        ComputeShader.Dispatch(_kernelIndex, (int)_threadGroupsCountX, 1, 1);
        
        Material.SetBuffer("OutputGrassBladesBuffer", _OutputGrassBladesBuffer);
        ComputeBuffer.CopyCount(_OutputGrassBladesBuffer, _argsBuffer, sizeof(uint));
        
        Material.SetVector("WindDirection", WindDirection);
        Material.SetFloat("WindForce", WindForce);
        Material.SetColor("YoungGrassColor", YoungGrassColor);
        Material.SetColor("OldGrassColor", OldGrassColor);

        Graphics.DrawMeshInstancedIndirect(
            mesh: _mesh,
            submeshIndex: 0,
            material: Material,
            bounds: _bounds,
            bufferWithArgs: _argsBuffer
        );
        
        // for (int i = 0; i < _allPosition.Count; ++i)
        // {
        //     UnityEngine.Debug.DrawRay(_allPosition[i], Vector3.down * 30, Color.red);
        // }
        
        // for (int i = 0; i < _grassBlades.Count; ++i)
        // {
        //     UnityEngine.Debug.DrawRay(_grassBlades[i].position, Vector3.up * 30, Color.blue);
        // }
    }

    void OnDestroy()
    {
        if (_grassBladesBuffer != null)
        {
            _grassBladesBuffer.Release();
        }

        if (_argsBuffer != null)
        {
            _argsBuffer.Release();
        }
    }

    void DrawWindGizmo()
    {
        Gizmos.color = Color.white;
        var origin = transform.position + (Vector3.up * 2);
        Gizmos.DrawSphere(origin, 0.1f);
        Gizmos.DrawLine(origin, origin + WindDirection);
    }

    void OnDrawGizmosSelected()
    {
        DrawWindGizmo();

        if (!_isInitialized)
        {
            return;
        }

        // Gizmos.DrawSphere(new Vector3(1, 0, 1), 0.1f);
        // for (var i = 0; i < _grassBlades.Count; i++)
        // {
        //     // Debug.Log($"grassBlade.position {_grassBlades[i].position}");
        //     Gizmos.DrawSphere(_grassBlades[i].position, 0.01f);
        // }
    }
}
