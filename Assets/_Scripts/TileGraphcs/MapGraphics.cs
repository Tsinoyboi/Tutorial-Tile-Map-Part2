﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using TileData;

namespace TileGraphics
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class MapGraphics : MonoBehaviour
    {
        public bool mapCentered = true;
        private MapData mapData;

        public int tileCountX = 10;
        public int tileCountZ = 10;

        public float tileSize = 1f;
        public float randomHeight = 1f;
        public float perlinScale = 3f;

        private Vector3 mapOffset;
        private int vertexCountX;
        private int vertexCountZ;

        //private Transform mapHolder;

        Vector3 tileCountReciprocal;
        Vector3 vertexCountReciprocal;
        
        public Texture2D terrainTiles;
        public int tileResolution = 16;


        void Awake ()
        {

            Initialize();
            BuildTexture();
            BuildMesh();

        }

        public void Initialize ()
        {
            Debug.ClearDeveloperConsole();

            mapData = new MapData(tileCountX, tileCountZ, perlinScale);

            vertexCountX = tileCountX + 1;
            vertexCountZ = tileCountZ + 1;

            vertexCountReciprocal.x = 1f / (float)vertexCountX;
            vertexCountReciprocal.z = 1f / (float)vertexCountZ;

            //terrainTiles.

        }

        public void BuildMesh ()
        {
            mapOffset = Vector3.zero;
            if (mapCentered) { mapOffset = new Vector3(tileCountX, 0f, tileCountZ) * tileSize * 0.5f; }

            int numTiles = tileCountX * tileCountZ;
            int numTriangles = numTiles * 2;

            int numVerts = vertexCountX * vertexCountZ;

            // generate Mesh Data
            Vector3[] vertecies = new Vector3[numVerts];
            int[] triangles = new int[numTriangles * 6];
            Vector3[] normals = new Vector3[numVerts];
            Vector2[] uv = new Vector2[numVerts];

            for (int x = 0; x < vertexCountX; x++)
            {
                for (int z = 0; z < vertexCountZ; z++)
                {

                    int vertexCountIndex = z + x * vertexCountZ;
                    //Debug.Log(vertexCountIndex);
                    vertecies[vertexCountIndex] = new Vector3(x, (mapData.CustomPerlinNoise(x, z) - 0.5f) * randomHeight, z) * tileSize - mapOffset;
                    normals[vertexCountIndex] = Vector3.up;
                    uv[vertexCountIndex] = new Vector2(x * mapData.MapDimensionsReciprocal.x, z * mapData.MapDimensionsReciprocal.z);

                    //Debug.Log(Mathf.PerlinNoise(x * vectorCountReciprocalX, z * vectorCountReciprocalY));
                    //Debug.Log(x + ", " + z + ": " + x * mapData.mapDimensionsReciprocal.x + ", " + z * mapData.mapDimensionsReciprocal.z);
                    //Debug.Log(x + ", " + z + ": " + new Vector2(x * mapData.mapDimensionsReciprocal.x, z * mapData.mapDimensionsReciprocal.z));
                }
            }

            for (int x = 0; x < tileCountX; x++)
            {
                for (int z = 0; z < tileCountZ; z++)
                {
                    int tileCoordIndex = 6 * (z + x * tileCountZ);
                    int vertexCoord = (z + x * vertexCountZ);

                    triangles[tileCoordIndex + 0] = vertexCoord + 0;
                    triangles[tileCoordIndex + 1] = vertexCoord + 1;
                    triangles[tileCoordIndex + 2] = vertexCoord + 1 + vertexCountZ;

                    triangles[tileCoordIndex + 3] = vertexCoord + 0;
                    triangles[tileCoordIndex + 4] = vertexCoord + 1 + vertexCountZ;
                    triangles[tileCoordIndex + 5] = vertexCoord + vertexCountZ;
                }
            }



            // 0, 1, 3, 0, 3, 2

            // create new mesh and populate with the data

            Mesh mesh = new Mesh();
            mesh.Clear();

            mesh.name = "TileMesh";
            mesh.vertices = vertecies;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;

            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // Assign mesh to object components

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            MeshCollider meshCollider = GetComponent<MeshCollider>();

            //meshFilter..Clear();

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

        }

        public void BuildTexture ()
        {

            int tileResolution = terrainTiles.height;
            int textureWidth = tileCountX * tileResolution;
            int textureLength =  tileCountZ * tileResolution;

            Texture2D texture = new Texture2D(textureWidth, textureLength);

            Color[][] tilePixels = GetTiles();

            for (int z = 0; z < tileCountX; z++)
            {
                for (int x = 0; x < tileCountZ; x++)
                {
                    //int perlinShade = Mathf.Clamp(Mathf.FloorToInt(CustomPerlinNoise(x + 0.5f, z + 0.5f) * 5), 0, 3);
                    //texture.SetPixels(x * tileResolution, z * tileResolution, tileResolution, tileResolution, tilePixels[perlinShade]);
                    texture.SetPixels(x * tileResolution, z * tileResolution, tileResolution, tileResolution, tilePixels[mapData.GetTileAt(x, z).tileType]);
                }
            }
            texture.filterMode = FilterMode.Trilinear;
            texture.name = "Perlin Noise";

            texture.Apply();

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.sharedMaterial.mainTexture = texture;


        }

        Color[][] GetTiles ()
        {
            int numTilesPerRow = 1;
            int numRows = 1;

            float tileImageResolution = 1f / (float)tileResolution;

            numTilesPerRow = Mathf.FloorToInt(terrainTiles.width * tileImageResolution);
            numRows = Mathf.FloorToInt(terrainTiles.height * tileImageResolution);

            //Debug.Log(numTilesPerRow + ", " + numRows);

            Color[][] pixels = new Color[4][];

            //pixels[0] = terrainTiles.GetPixels(0, 0, 16, 16);
            //pixels[1] = terrainTiles.GetPixels(16, 0, 16, 16);
            //pixels[2] = terrainTiles.GetPixels(32, 0, 16, 16);
            //pixels[3] = terrainTiles.GetPixels(48, 0, 16, 16);

            for (int z = 0; z < numRows; z++)
            {
                for (int x = 0; x < numTilesPerRow; x++)
                {
                    int index = z * numTilesPerRow + x;
                    pixels[index] = terrainTiles.GetPixels(x * tileResolution, z * tileResolution, tileResolution, tileResolution);

                    //Debug.Log("(" + x + ", " + z + ")// pixels[" + z * numTilesPerRow + x + "] = ~~~GetPixels( " + x * tileResolution + ", " + z * tileResolution + ", " + tileResolution + ", " + tileResolution + ")");


                }
            }


            return pixels;
        }
        
        void MapSetup ()
        {
            //mapHolder = new GameObject("Map").transform;

            //instance.transform.SetParent(mapHolder);
        }


        //float CustomPerlinNoise (float x, float y)
        //{
            
        //    return Mathf.PerlinNoise((x + perlinOrg.x) * vertexCountReciprocal.x * perlinScale, (y + perlinOrg.z) * vertexCountReciprocal.z * perlinScale);
        //}
    }

}