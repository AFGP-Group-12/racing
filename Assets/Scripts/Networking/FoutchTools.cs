using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class FoutchTools : EditorWindow
{
    [MenuItem("Tools/Foutch Tools")]
    public static void ShowWindow()
    {
        GetWindow<FoutchTools>("Foutch Tools");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Print Path Markers"))
        {
            List<BotGoalMarkerScript> markers = getPathMarkers();
            markers.Sort((a, b) => a.PathNumber.CompareTo(b.PathNumber));
            string output = "Path Markers:\n[";
            foreach (BotGoalMarkerScript marker in markers)
            {
                output += "vec3" + marker.transform.position.ToString("F3");
                if (marker != markers[markers.Count - 1])
                    output += ", ";
            }
            output += "]";
            Debug.Log(output);
        }
    }

    public static List<BotGoalMarkerScript> getPathMarkers()
    {
        return new List<BotGoalMarkerScript>(Object.FindObjectsByType<BotGoalMarkerScript>(FindObjectsSortMode.None));
    }
}