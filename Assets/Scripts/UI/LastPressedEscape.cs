using UnityEngine;

public class LastPressedEscape : MonoBehaviour {
    public static float LastPressedEscapeTime;

    public static void SetPressedEscape() {
        LastPressedEscapeTime = Time.time;
    }

    public static bool LastPressedEscapeCooldownOver(float cooldown) {
        return Time.time > LastPressedEscapeTime + cooldown;
    }
}
