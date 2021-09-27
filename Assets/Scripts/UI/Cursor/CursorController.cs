using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorUser {
    public string name;
    public enum Type { Normal, Pointer, Dragger, EditorEraser, EditorLinkLogicEntities };
    public Type type = Type.Normal;

    public CursorUser(string name, Type type) {
        this.name = name;
        this.type = type;
    }
}

public class CursorController : MonoBehaviour {

    private static CursorController Singletron;

    public static List<CursorUser> Users = new List<CursorUser>();

    private static Texture2D Pointer;
    private static Texture2D Dragger;
    private static Texture2D EditorEraser;
    private static Texture2D EditorLinkLogicEntities;

    private void Awake() {
        SceneManager.activeSceneChanged += SceneChanged;

        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(this);

        Pointer = Resources.Load("pointer", typeof(Texture2D)) as Texture2D;
        Dragger = Resources.Load("dragger", typeof(Texture2D)) as Texture2D;
        EditorEraser = Resources.Load("editorEraser", typeof(Texture2D)) as Texture2D;
        EditorLinkLogicEntities = Resources.Load("linkLogicEntities", typeof(Texture2D)) as Texture2D;
    }

    private void SceneChanged(Scene current, Scene next) {
        if (next.buildIndex == 0) {
            AddUser("mainmenu");
        } else {
            RemoveUser("mainmenu");
        }
    }

    public static void AddUser(string name, CursorUser.Type type = CursorUser.Type.Normal) {
        CursorUser user = GetUser(name);
        if(user == null) {
            Users.Add(new CursorUser(name, type));
        } else {
            user.type = type;
        }

        SetCursor();
    }

    public static CursorUser GetUser(string name) {
        foreach(CursorUser user in Users) {
            if (user.name == name) return user;
        }
        return null;
    }

    public static void RemoveUser(string name) {
        Users.Remove(GetUser(name));

        SetCursor();
    }

    public static void RemoveAllUsers() {
        Users.Clear();
    }

    private static void SetCursor() {
        if (Users.Count == 0) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Cursor.SetCursor(null, new Vector2(10, 7), CursorMode.Auto);
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            bool alreadySetCursor = false;

            foreach (CursorUser user in Users) {
                switch (user.type) {
                    case CursorUser.Type.Normal:
                        if (!alreadySetCursor) Cursor.SetCursor(null, new Vector2(10, 7), CursorMode.Auto);
                        break;
                    case CursorUser.Type.Pointer:
                        alreadySetCursor = true;
                        Cursor.SetCursor(Pointer, new Vector2(10, 7), CursorMode.Auto);
                        break;
                    case CursorUser.Type.Dragger:
                        alreadySetCursor = true;
                        Cursor.SetCursor(Dragger, new Vector2(10, 7), CursorMode.Auto);
                        break;
                    case CursorUser.Type.EditorEraser:
                        alreadySetCursor = true;
                        Cursor.SetCursor(EditorEraser, new Vector2(10, 7), CursorMode.Auto);
                        break;
                    case CursorUser.Type.EditorLinkLogicEntities:
                        alreadySetCursor = true;
                        Cursor.SetCursor(EditorLinkLogicEntities, new Vector2(16, 16), CursorMode.Auto);
                        break;
                }
            }
        }
    }

    public static void ListAllUsers() {
        print("All cursor users:");
        foreach (CursorUser user in Users) {
            print(user.name);
        }
    }
}
