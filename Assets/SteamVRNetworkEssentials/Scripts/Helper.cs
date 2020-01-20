using UnityEngine;
using System.Collections;

public static class Helper
{
    public static void AttachAtGrip(Transform holder, Transform objectToAttach, string gripName = "Grip")
    {
        var grip = objectToAttach.Find(gripName);
        objectToAttach.position = holder.position;
        objectToAttach.rotation = holder.rotation;
        objectToAttach.parent = holder;
        objectToAttach.localPosition = grip.transform.localPosition;
        objectToAttach.localRotation = grip.localRotation;
    }
}
