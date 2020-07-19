using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAtlas : MonoBehaviour
{

    Mesh mesh;
    public Vector2 Tiling;
    public Vector2 Offset;
    Vector3[] vertices = new Vector3[4];
    Vector2[] uvs = new Vector2[4];

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices[0] = new Vector3(0, 0, 0); //top-left
        vertices[1] = new Vector3(1, 0, 0); //top-right
        vertices[2] = new Vector3(0, -1, 0); //bottom-left
        vertices[3] = new Vector3(1, -1, 0); //bottom-right

        mesh.vertices = vertices;

        int[] triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
        mesh.triangles = triangles;

        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);

        mesh.uv = uvs;
        
    }

    // Update is called once per frame
    void Update()
    {
             
        uvs[0] = new Vector2(0 + Offset.x, 1 + Offset.y);
        uvs[1] = new Vector2(1 + Offset.x, 1 + Offset.y);
        uvs[2] = new Vector2(0 + Offset.x, 0 + Offset.y);
        uvs[3] = new Vector2(1 + Offset.x, 0 + Offset.y);

        mesh.uv = uvs;
    }
}
