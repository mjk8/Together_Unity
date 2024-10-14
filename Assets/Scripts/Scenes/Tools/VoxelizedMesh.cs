using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelizedMesh : MonoBehaviour
{
    public List<Vector3Int> gridPoints = new List<Vector3Int>();
    public float HalfSize = 0.1f;
    public Vector3 LocalOrigin; //로컬좌표 기준 바운드상자의 꼭짓점 시작점 (바운드상자 첫 시작 꼭짓점 좌표인듯?)
    
    

    private void Start()
    {
        VoxelizeMesh(gameObject.GetComponent<MeshFilter>());
    }

    //https://bronsonzgeb.com/index.php/2021/05/15/simple-mesh-voxelization-in-unity/
    public Vector3 PointToPosition(Vector3Int point) //해당 로컬 복셀중점의 좌표를 월드좌표로 변환
    {
        float size = HalfSize * 2f;
        Vector3 pos = new Vector3(HalfSize + point.x * size, HalfSize + point.y * size, HalfSize + point.z * size);
        return LocalOrigin + transform.TransformPoint(pos);
    }
    
    public static void VoxelizeMesh(MeshFilter meshFilter)
    {
        if (!meshFilter.TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
        }

        if (!meshFilter.TryGetComponent(out VoxelizedMesh voxelizedMesh))
        {
            voxelizedMesh = meshFilter.gameObject.AddComponent<VoxelizedMesh>();
        }

        //bounds.extents : 경계 상자의 중점에서 꼭짓점까지의 크기벡터(x,y,z벡터)
        Bounds bounds = meshCollider.bounds; //월드좌표 기준 메시콜라이더를 모두 포함하는 경계 상자
        Vector3 minExtents = bounds.center - bounds.extents; //월드좌표에서 경계 상자의 한 모서리에 위치한 점의 좌표(기준 시작점 좌표일듯...아마 꼭짓점)
        float halfSize = voxelizedMesh.HalfSize; //복셀의 한변의 절반 크기
        Vector3 count = bounds.extents / halfSize;

        int xMax = Mathf.CeilToInt(count.x);
        int yMax = Mathf.CeilToInt(count.y);
        int zMax = Mathf.CeilToInt(count.z);

        voxelizedMesh.gridPoints.Clear();
        voxelizedMesh.LocalOrigin = voxelizedMesh.transform.InverseTransformPoint(minExtents);

        for (int x = 0; x < xMax; ++x)
        {
            for (int z = 0; z < zMax; ++z)
            {
                for (int y = 0; y < yMax; ++y)
                {
                    Vector3 pos = voxelizedMesh.PointToPosition(new Vector3Int(x, y, z));
                    if (Physics.CheckBox(pos, new Vector3(halfSize, halfSize, halfSize)))
                    {
                        voxelizedMesh.gridPoints.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }
}
