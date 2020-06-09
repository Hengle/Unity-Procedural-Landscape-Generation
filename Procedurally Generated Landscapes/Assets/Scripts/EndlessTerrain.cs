﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewer;

    private static MapGenerator mapGenerator;
    public static Vector2 viewerPosition;
    public Material mapMaterial;


    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        foreach(var chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        //find the coordinate of the chunk the player is standing on
        int currentChunkCoordinateX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordinateY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoordinate = new Vector2(currentChunkCoordinateX + xOffset, currentChunkCoordinateY + yOffset);
                if (terrainChunkDict.ContainsKey(viewedChunkCoordinate))
                {
                    //update the terrain chunk
                    terrainChunkDict[viewedChunkCoordinate].UpdateTerrainChunk();
                    if (terrainChunkDict[viewedChunkCoordinate].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoordinate]);
                    }
                }
                else
                {
                    //instantiate new terrain chunk
                    terrainChunkDict.Add(viewedChunkCoordinate, new TerrainChunk(viewedChunkCoordinate, chunkSize, transform, mapMaterial));
                }
            }
        }
    }


    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 _coord, int _size, Transform _parent, Material _mat)
        {
            position = _coord * _size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            bounds = new Bounds(position, Vector2.one * _size);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = _mat;
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = _parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData _mapData)
        {
            mapGenerator.RequestMeshData(_mapData, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData _meshData)
        {
            meshFilter.mesh = _meshData.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            float viewerDistance = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistance <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool _visibility)
        {
            meshObject.SetActive(_visibility);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}