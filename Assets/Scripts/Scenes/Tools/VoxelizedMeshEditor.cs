
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelizedMesh))]
public class VoxelizedMeshEditor : Editor
{
    void OnSceneGUI()
    {
        VoxelizedMesh voxelizedMesh = target as VoxelizedMesh;

        Handles.color = Color.green;
        float size = voxelizedMesh.HalfSize * 2f;

        foreach (Vector3Int gridPoint in voxelizedMesh.gridPoints)
        {
            Vector3 worldPos = voxelizedMesh.PointToPosition(gridPoint);
            Handles.DrawWireCube(worldPos, new Vector3(size, size, size));
        }

        Handles.color = Color.red;
        if (voxelizedMesh.TryGetComponent(out MeshCollider meshCollider))
        {
            Bounds bounds = meshCollider.bounds;
            Handles.DrawWireCube(bounds.center, bounds.extents * 2);
        }
    }
}

#endif