#if UNITY_EDITOR

using UnityEditor;

using UnityEngine;
using System.Text;
using System.IO;

// This script combines the meshes from children into a single new Mesh.
// The combined mesh is assigned to the MeshFilter of this GameObject.
// The original meshes are not destroyed, but their GameObjects are deactivated.

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
    [MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
    public static void SaveMeshInPlace(MenuCommand menuCommand)
    {
        MeshFilter mf = menuCommand.context as MeshFilter;
        Mesh m = mf.sharedMesh;
        SaveMesh(m, m.name, false, true);
    }

    [MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
    public static void SaveMeshNewInstanceItem(MenuCommand menuCommand)
    {
        MeshFilter mf = menuCommand.context as MeshFilter;
        Mesh m = mf.sharedMesh;
        SaveMesh(m, m.name, true, true);
    }

    public static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
    {
        string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);

        Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

        if (optimizeMesh)
            MeshUtility.Optimize(meshToSave);

        AssetDatabase.CreateAsset(meshToSave, path);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("GameObject/Export to OBJ")]
    static void ExportToOBJ()
    {

        GameObject obj = Selection.activeObject as GameObject;
        if (obj == null)
        {
            Debug.Log("No object selected.");
            return;
        }

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.Log("No mesh found in selected GameObject.");
            return;
        }

        MeshCombiner meshCombiner = obj.GetComponent<MeshCombiner>();
        if (meshFilter == null)
        {
            Debug.Log("No mesh combiner found in selected GameObject.");
            return;
        }

        meshCombiner.combine();

        string path = EditorUtility.SaveFilePanel("Export OBJ", "", obj.name, "obj");
        if (string.IsNullOrEmpty(path)) return;

        Mesh mesh = meshFilter.sharedMesh;
        StringBuilder sb = new StringBuilder();

        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        foreach (Vector3 v in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }

        for (int material = 0; material < mesh.subMeshCount; material++)
        {
            sb.Append(string.Format("\ng {0}\n", obj.name)); // Group name
            int[] triangles = mesh.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }

        StreamWriter writer = new StreamWriter(path);
        writer.Write(sb.ToString());
        writer.Close();

        Debug.Log("Mesh exported to: " + path);
    }

    public static void ExportToOBJ(Mesh mesh, string path)
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh provided.");
            return;
        }

        if (string.IsNullOrEmpty(path)) 
            path = EditorUtility.SaveFilePanel("Export OBJ", "", "combinedMesh", "obj");
        if (string.IsNullOrEmpty(path)) return;

        StringBuilder sb = new StringBuilder();

        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        foreach (Vector3 v in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }

        for (int material = 0; material < mesh.subMeshCount; material++)
        {
            sb.Append(string.Format("\ng {0}\n", "obj.name")); // Group name
            int[] triangles = mesh.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }

        StreamWriter writer = new StreamWriter(path);
        writer.Write(sb.ToString());
        writer.Close();

        Debug.Log("Mesh exported to: " + path);
    }

    // Function to check if a given layer index is part of a LayerMask
    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        // Shift 1 by the layer index to get a bitmask for that specific layer
        // Then perform a bitwise AND with the targetLayerMask
        // If the result is non-zero, the layer is included in the mask
        return ((1 << layer) & layerMask) != 0;
    }

    public LayerMask layersToGenerateMeshFrom;
    public bool makeChildrenInactive;

    public static Mesh combine(GameObject obj, LayerMask layersToGenerateMeshFrom, bool makeChildrenInactive)
    {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] instances = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            // Check if the specific layer is in the LayerMask
            if (!IsInLayerMask(meshFilter.gameObject.layer, layersToGenerateMeshFrom))
            {
                continue;
            }

            instances[i] = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };
            if (makeChildrenInactive)
                meshFilter.gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(instances);
        //gameObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        Debug.Log("Combined " + meshFilters.Length + " meshes into one.");
        return combinedMesh;
    }

    public void combine()
    {
        combine(this.gameObject, layersToGenerateMeshFrom, makeChildrenInactive);
    }

    void Start()
    {
        
    }
}

#endif