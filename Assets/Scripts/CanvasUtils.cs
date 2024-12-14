using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A support class with functions for UI
// This class can be referenced by any other script in the project
public class CanvasUtils : MonoBehaviour
{   
    // -- CHILD FUNCTIONS -- //
    // Changing data of all children

    public static void SetTransparencyOfChildren(GameObject inputObject, float a) {
        Image[] images = inputObject.GetComponentsInChildren<Image>();

        for (int i = 0; i < images.Length; i++) {
            images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, a);
        }
    }

    public static void DestroyChildren(GameObject inputObject) {
        Transform[] toDestroy = inputObject.GetComponentsInChildren<Transform>();
        for (int i = toDestroy.Length - 1; i >= 0; i--) {
            if (toDestroy[i].gameObject != inputObject) {
                Destroy(toDestroy[i].gameObject);
            }
            toDestroy[i] = null;
        }
    }

    public static void SetChildrenActive(GameObject inputObject, bool active) {
        Transform[] toDestroy = inputObject.GetComponentsInChildren<Transform>();
        for (int i = toDestroy.Length - 1; i >= 0; i--) {
            if (toDestroy[i].gameObject != inputObject) {
                toDestroy[i].gameObject.SetActive(active);
            }
            toDestroy[i] = null;
        }
    }

    public static Transform SearchChildrenForName(GameObject inputObject, string name) {
        Transform[] children = inputObject.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++) {
            if (children[i].gameObject.name == name) {
                // found the name, so return
                return children[i];
            }
        }

        // tried to find the object and did not find it
        Debug.Log("ERROR: Attempt to find an object of name " + name + " failed, object does not exist");
        return null;
    }

    // -- INTERACTION FUNCTIONS -- //
    // These use the order of children in the heirarchy to check interaction with the cursor

    // Detect whether the cursor is interacting with a supplied object
    public static bool IsCursorInteract(GameObject inputObject, bool ignoreChildren) {
        RectTransform[] canvasObjects = UIManager.Instance.canvasTransform.GetComponentsInChildren<RectTransform>();

        // loop backwards through the canvas objects
        // only works if they are organized in order
        for (int i = canvasObjects.Length - 1; i > 0; i--) {
            if (canvasObjects[i].gameObject == inputObject && IsCursorInBounds(inputObject, false)) {
                return true; // if we reach the input return true
            }
            else if (canvasObjects[i].gameObject.activeSelf && IsCursorInBounds(canvasObjects[i].gameObject, false) && canvasObjects[i].gameObject.GetComponent<Image>() != null) {
                if (ignoreChildren && canvasObjects[i].transform.IsChildOf(inputObject.transform)) { continue; }
                return false;
            }
        }

        if (inputObject == UIManager.Instance.canvasTransform.gameObject) { return true; }

        return false; // this should happen if the cursor isn't in the bounding box of the object at all
    }

    // Detect whether the cursor is within the bounds of a supplied object
    public static bool IsCursorInBounds(GameObject inputObject, bool mustBeActive) {
        // The object needs to have a recttransform in order for the function to work
        Vector2 scale = inputObject.GetComponent<RectTransform>().sizeDelta;
        Vector2 offset = new Vector2(inputObject.GetComponent<RectTransform>().pivot.x * scale.x, inputObject.GetComponent<RectTransform>().pivot.y * scale.y);
        // Return false if the gameobject is hidden
        if (!inputObject.activeSelf && mustBeActive) {return false;}
        // check if the cursor is inside the object's 'bounding box'
        if (Input.mousePosition.x > inputObject.transform.position.x - offset.x
        && Input.mousePosition.x < inputObject.transform.position.x - offset.x + scale.x
        && Input.mousePosition.y > inputObject.transform.position.y - offset.y
        && Input.mousePosition.y < inputObject.transform.position.y - offset.y + scale.y) {
            return true;
        }
        else {
            return false;
        }
    }

    // -- ARRAY MANIPULATION FUNCTIONS -- // 

    public static Vector3[] RemoveDuplicatePoints(Vector3[] points) {
        List<Vector3> toReturn = new List<Vector3>();
        for (int i = 0; i < points.Length; i++) {
            if (!toReturn.Contains(points[i])) {
                toReturn.Add(points[i]);
            }
        }
        return toReturn.ToArray();
    }

    public static Vector3[] RemovePoint(Vector3[] points, Vector3 toRemove) {
        List<Vector3> toReturn = new List<Vector3>();
        for (int i = 0; i < points.Length; i++) {
            if (points[i] != toRemove) {
                toReturn.Add(points[i]);
            }
        }
        return toReturn.ToArray();
    }

    public static int GetIndexOfMinimum(float[] values) {
        int minIndex = -1;
        float min = -1;
        for (int i = 0; i < values.Length; i++) {
            if (min == -1 || values[i] < min) {
                minIndex = i;
                min = values[i];
            }
        }
        return minIndex;
    }


    // public static void ApplyColorPalette() {
    //     Image[] images = UIManager.Instance.canvasTransform.gameObject.GetComponentsInChildren<Image>();
    //     for (int i = 0; i < images.Length; i++) {
    //         if (images[i].color == AppData.Instance.editorColor1) {
    //             images[i].color = AppData.Instance.color1;
    //         } else if (images[i].color == AppData.Instance.editorColor2) {
    //             images[i].color = AppData.Instance.color2;
    //         } else if (images[i].color == AppData.Instance.editorColor3) {
    //             images[i].color = AppData.Instance.color3;
    //         } else if (images[i].color == AppData.Instance.editorColor4) {
    //             images[i].color = AppData.Instance.color4;
    //         }
    //     }
    // }

    // public static void ApplyColorPaletteToObject(GameObject inputObject) {
    //     Image image = inputObject.GetComponent<Image>();
    //     if (image.color == AppData.Instance.editorColor1) {
    //         image.color = AppData.Instance.color1;
    //     } else if (image.color == AppData.Instance.editorColor2) {
    //         image.color = AppData.Instance.color2;
    //     } else if (image.color == AppData.Instance.editorColor3) {
    //         image.color = AppData.Instance.color3;
    //     } else if (image.color == AppData.Instance.editorColor4) {
    //         image.color = AppData.Instance.color4;
    //     }
    // }

    public static Color MakeOpaque(Color inputColor) {
        return new Color(inputColor.r, inputColor.g, inputColor.b, 1);
    }
}
