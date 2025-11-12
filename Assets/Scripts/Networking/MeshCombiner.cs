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

    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] instances = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            var meshFilter = meshFilters[i];

            instances[i] = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            meshFilter.gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(instances);
        gameObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        gameObject.SetActive(true);
    }
}