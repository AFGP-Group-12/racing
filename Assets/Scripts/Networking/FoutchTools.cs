using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

public class FoutchTools : EditorWindow
{
    [MenuItem("Tools/Foutch Tools")]
    public static void ShowWindow()
    {
        GetWindow<FoutchTools>("Foutch Tools");
    }

    private void OnGUI()
    {
        GUILayout.Label("Foutch Tools", EditorStyles.boldLabel);
        PrintPathMarkers_Button();
        GenerateServerSideMeshes_Foldout();
    }

    private static void PrintPathMarkers_Button()
    {
        if (!GUILayout.Button("Print Path Markers")) { return; }
        
        List<BotGoalMarkerScript> markers = getPathMarkers();
        markers.Sort((a, b) => a.PathNumber.CompareTo(b.PathNumber));
        string output = "Path Markers:\n{";
        foreach (BotGoalMarkerScript marker in markers)
        {
            output += "vec3" + marker.transform.position.ToString("F3");
            if (marker != markers[markers.Count - 1])
                output += ", ";
        }
        output += "}";
        Debug.Log(output);
    }

    private static bool generateServerSideMeshes_FoldoutShowing = false;
    private static GameObject meshParent = null;
    private static void GenerateServerSideMeshes_Foldout()
    {
        generateServerSideMeshes_FoldoutShowing = EditorGUILayout.Foldout(generateServerSideMeshes_FoldoutShowing, "Generate Server-Side Meshes");
        if (!generateServerSideMeshes_FoldoutShowing) return;

        meshParent = EditorGUILayout.ObjectField(meshParent, typeof(GameObject), true) as GameObject;


        if (GUILayout.Button("Generate Server-Side Meshes"))
        {
            if (meshParent == null)
            {
                Debug.LogError("Please assign a parent object containing the meshes to generate.");
                return;
            }
            Debug.Log("Generating server-side meshes...");
            
            GenerateMeshForLayer(meshParent, 3, "Pogo");
            GenerateMeshForLayer(meshParent, 8, "Grapple");
            GenerateMeshForLayer(meshParent, 9, "Ground");
            GenerateMeshForLayer(meshParent, 10, "Wall");
            GenerateMeshForLayer(meshParent, 11, "Bounds");
            GenerateMeshForLayer(meshParent, 12, "Obstacle");
        }
    }

    private static void GenerateMeshForLayer(GameObject parent, int layer, string layerName)
    {
        Mesh combinedMesh = MeshCombiner.combine(parent, 1 << layer, false);
        string sceneName = SceneManager.GetActiveScene().name.ToString();
        MeshCombiner.ExportToOBJ(combinedMesh, "Assets/Meshes/" + sceneName + "_Layer_" + layerName + "_Mesh.obj");
    }

    private static List<BotGoalMarkerScript> getPathMarkers()
    {
        return new List<BotGoalMarkerScript>(Object.FindObjectsByType<BotGoalMarkerScript>(FindObjectsSortMode.None));
    }
}