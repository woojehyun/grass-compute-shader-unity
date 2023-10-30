//#define A

using System.Collections.Generic;
using UnityEngine;



public static class GrassFactory
{
    const float Height = 0.05f;
    const float Width = Height / 6;
    const int StepsCount = 4;
    const float StepHeight = Height / StepsCount;
    const float HalfWidth = Width / 2;

    private const float addX = 0.02f;
    private const float addZ = 0.05f;

    // Start is called before the first frame update
    public static Mesh GetGrassBladeMesh()
    {
        var mesh = new Mesh();

        // define the vertices
        mesh.vertices = new Vector3[] {
            // step 0
            new Vector3(-HalfWidth, 0, 0),
            new Vector3(HalfWidth, 0, 0),

            // step 1
            new Vector3(-HalfWidth * 0.9f, StepHeight, -0.005f),
            new Vector3(HalfWidth * 0.9f, StepHeight, -0.005f),

            // step 2
            new Vector3(-HalfWidth * 0.8f, 2 * StepHeight, -0.01f),
            new Vector3(HalfWidth * 0.8f, 2 * StepHeight, -0.01f),

            // step 3
            new Vector3(-HalfWidth * 0.6f, 3 * StepHeight, -0.025f),
            new Vector3(HalfWidth * 0.6f, 3 * StepHeight, -0.025f),

            // step 4
            new Vector3(0, 4 * StepHeight, -0.05f),
            
            // step 1-0
            new Vector3(-HalfWidth - addX, 0, -addZ),
            new Vector3(HalfWidth - addX, 0, -addZ),

            // step 1-1
            new Vector3(-HalfWidth * 0.9f - addX, StepHeight, 0.005f-addZ),
            new Vector3(HalfWidth * 0.9f - addX , StepHeight, 0.005f-addZ),

            // step 1-2
            new Vector3(-HalfWidth * 0.8f - addX, 2 * StepHeight, 0.01f-addZ),
            new Vector3(HalfWidth * 0.8f - addX , 2 * StepHeight, 0.01f-addZ),

            // step 1-3
            new Vector3(-HalfWidth * 0.6f - addX, 3 * StepHeight, 0.025f-addZ),
            new Vector3(HalfWidth  * 0.6f- addX , 3 * StepHeight, 0.025f-addZ),

            // step 1-4
            new Vector3( -addX, 4 * StepHeight, 0.05f-addZ),
            
            // step 2-0
            new Vector3(-HalfWidth + addX, 0, 0 + addZ),
            new Vector3(HalfWidth + addX, 0, 0 + addZ),

            // step 2-1
            new Vector3(-HalfWidth * 0.9f + addX, StepHeight, 0.005f + addZ),
            new Vector3(HalfWidth * 0.9f + addX , StepHeight, 0.005f + addZ),

            // step 2-2
            new Vector3(-HalfWidth * 0.8f + addX, 2 * StepHeight, 0.01f + addZ),
            new Vector3(HalfWidth * 0.8f + addX , 2 * StepHeight, 0.01f + addZ),

            // step 2-3
            new Vector3(-HalfWidth * 0.6f + addX, 3 * StepHeight, 0.025f + addZ),
            new Vector3(HalfWidth * 0.6f + addX , 3 * StepHeight, 0.025f + addZ),

            // step 2-4
            new Vector3( addX, 4 * StepHeight, 0.05f + addZ)
        };

        // define the normals
        Vector3[] normalsArray = new Vector3[mesh.vertices.Length];
        System.Array.Fill(normalsArray, new Vector3(0, 0, -1));
        mesh.normals = normalsArray;

        mesh.uv = new Vector2[] {
            // step 0
            new Vector2(0, 0),
            new Vector2(1, 0),

            // step 1
            new Vector2(0, 0.25f),
            new Vector2(1, 0.25f),

            // step 2
            new Vector2(0, 0.5f),
            new Vector2(1, 0.5f),

            // step 3
            new Vector2(0, 0.75f),
            new Vector2(1, 0.75f),

            // step 4
            new Vector2(0.5f, 1f),
            
            // step 1-0
            new Vector2(0, 0),
            new Vector2(1, 0),

            // step 1-1
            new Vector2(0, 0.25f),
            new Vector2(1, 0.25f),

            // step 1-2
            new Vector2(0, 0.5f),
            new Vector2(1, 0.5f),

            // step 1-3
            new Vector2(0, 0.75f),
            new Vector2(1, 0.75f),

            // step 1-4
            new Vector2(0.5f, 1f),
            
            // step 2-0
            new Vector2(0, 0),
            new Vector2(1, 0),

            // step 2-1
            new Vector2(0, 0.25f),
            new Vector2(1, 0.25f),

            // step 2-2
            new Vector2(0, 0.5f),
            new Vector2(1, 0.5f),

            // step 2-3
            new Vector2(0, 0.75f),
            new Vector2(1, 0.75f),

            // step 2-4
            new Vector2(0.5f, 1f),
        };

        mesh.SetIndices(
            // counter clock wise so the normals make sense
            indices: new int[]{
                // step 0
                0,1,2,
                2,1,3,

                // step 1
                2,3,4,
                4,3,5,

                // step 2
                4,5,6,
                6,5,7,

                // step 3
                6,7,8,
                
                // step 1-0
                9,10,11,
                11,10,12,

                // step 1-1
                11,12,13,
                13,12,14,

                // step 1-2
                13,14,15,
                15,14,16,

                // step 1-3
                15,16,17,
                
                // step 2-0
                9 + 9,10 + 9,11 + 9,
                11 + 9,10 + 9,12 + 9,

                // step 2-1
                11 + 9,12 + 9,13 + 9,
                13 + 9,12 + 9,14 + 9,

                // step 2-2
                13 + 9,14 + 9,15 + 9,
                15 + 9,14 + 9,16 + 9,

                // step 2-3
                15 + 9,16 + 9,17 + 9,
            },
            topology: MeshTopology.Triangles,
            submesh: 0
        );

        return mesh;
    }
    
    public static void RaycastGrassBlades(
        Transform transform,
        MeshFilter meshFilter,
        float maxExtent,
        float density,
        out Bounds bounds,
        out List<GrassBlade> grassBlades,
        out List<Vector3> allPosition

    )
    {
#if A        
        
        var meshBounds = meshFilter.sharedMesh.bounds;
        
        bounds = new Bounds(
            transform.position,
            new Vector3(
                System.Math.Min(meshBounds.extents.x * transform.localScale.x, maxExtent) * 2,
                meshBounds.extents.y * 2,
                System.Math.Min(meshBounds.extents.z * transform.localScale.z, maxExtent) * 2
            )
        );
        
        var grassBladesCountX = bounds.extents.x * 2 * density;
        var grassBladesCountY = bounds.extents.y * 2 * density;
        
        var grassBladesCount = (int)(grassBladesCountX * grassBladesCountY);
        
        grassBlades = new List<GrassBlade>();
        allPosition = new List<Vector3>();
        
        for (int i = 0; i < grassBladesCount; ++i)
        {
            
            var localPos = new Vector3(
                x: Random.Range(-bounds.extents.x, bounds.extents.x),
                y: 0,
                z: Random.Range(-bounds.extents.z, bounds.extents.z)
            );
        
            var origin = localPos  + (transform.up * 20); //transform.TransformPoint(localPos) + (transform.up * 20);
        
            allPosition.Add(origin);
            
            RaycastHit hit;
            var didHit = Physics.Raycast(
                origin: origin,
                direction: -transform.up,
                hitInfo: out hit
            );
        
            if (didHit)
            {
                GrassBlade grassBlade = new GrassBlade();
        
                localPos.y = hit.point.y;

                grassBlade.position = localPos;//transform.TransformPoint(localPos);
        
                grassBlade.rotationY = Random.Range((float)-System.Math.PI, (float)System.Math.PI);
        
                grassBlades.Add(grassBlade);
            }
        }

        #else

        var meshBounds = meshFilter.sharedMesh.bounds;
    
        bounds = new Bounds(
            transform.position,
            new Vector3(
                System.Math.Min(meshBounds.extents.x * transform.localScale.x, maxExtent) * 2 + 3,
                meshBounds.extents.y * 4,
                System.Math.Min(meshBounds.extents.y * transform.localScale.y, maxExtent) * 2 + 3
            )
        );
    
        var grassBladesCountX = bounds.extents.x * 2 * density;
        var grassBladesCountY = bounds.extents.z * 2 * density;
    
        var grassBladesCount = (int)(grassBladesCountX * grassBladesCountY);
    
        grassBlades = new List<GrassBlade>();
        allPosition = new List<Vector3>();
        
        for (int i = 0; i < grassBladesCount; ++i)
        {

            var position = new Vector3(
                x: Random.Range(-bounds.extents.x, bounds.extents.x),
                y: 0,
                z: Random.Range(-bounds.extents.z, bounds.extents.z)
            );

            position += transform.position;
            var origin = position + Vector3.up * 20; //transform.TransformPoint(localPos) + (transform.up * 20);

            allPosition.Add(origin);
            
            RaycastHit hit;
            var didHit = Physics.Raycast(
                origin: origin,
                direction: Vector3.down,
                hitInfo: out hit,
                100f,
                layerMask: (1 << LayerMask.NameToLayer("GrassCollider"))
            );

            if (didHit)
            {
                GrassBlade grassBlade = new GrassBlade();

                position.y = hit.point.y;

                grassBlade.position = position;//transform.TransformPoint(localPos);

                grassBlade.rotationY = Random.Range((float)-System.Math.PI, (float)System.Math.PI);

                grassBlades.Add(grassBlade);
            }
        }
#endif
    }
}
