using System.Collections.Generic;
using UnityEngine;

public class LevelEditorLinesController : MonoBehaviour {
    private static LevelEditorLinesController Singletron;

    private List<LevelEditorLineController> lines = new List<LevelEditorLineController>();

    public LevelEditorLineController template;

    private void Awake() {
        Singletron = this;
    }

    public static LevelEditorLineController GetLine(Transform origin) {
        foreach(LevelEditorLineController line in Singletron.lines) {
            if (line.origin == origin) return line;
        }
        return null;
    }

    public static void CreateLine(Transform origin, Transform target) {
        LevelEditorLineController newLine = Instantiate(Singletron.template, Vector3.zero, Quaternion.identity);
        newLine.transform.parent = Singletron.transform;
        newLine.Setup(origin, target);

        Singletron.lines.Add(newLine);
    }

    public static void CreateOrEditLine(Transform origin, Transform target) {
        LevelEditorLineController existingLine = GetLine(origin);
        if (existingLine == null) {
            CreateLine(origin, target);
        } else {
            //origin will always be the same so no point reassigning it to itself
            existingLine.target = target;
        }
    }

    /// <summary>
    /// Sets the new target for all lines with the old target
    /// </summary>
    /// <param name="oldTarget"></param>
    /// <param name="newTarget"></param>
    public static void UpdateTarget(Transform oldTarget, Transform newTarget) {
        foreach (LevelEditorLineController line in Singletron.lines) {
            if (line.target == oldTarget) line.target = newTarget;
        }
    }

    public static void DestroyAllLinesWithTarget(Transform target) {
        for(int i = 0; i < Singletron.lines.Count; i++) {
            LevelEditorLineController line = Singletron.lines[i];

            if (line.target == target) {
                Singletron.lines.Remove(line);
                Destroy(line.gameObject);
            }
        }
    }

    public static void DestroyAllLinesWithSource(Transform target) {
        for (int i = 0; i < Singletron.lines.Count; i++) {
            LevelEditorLineController line = Singletron.lines[i];

            if (line.origin == target) {
                Singletron.lines.Remove(line);
                Destroy(line.gameObject);
            }
        }
    }

    public static void DestroyAllLines() {
        foreach(Transform line in Singletron.template.transform.parent) {
            if (line.gameObject.activeSelf) Destroy(line.gameObject);
        }

        Singletron.lines.Clear();
    }
}
