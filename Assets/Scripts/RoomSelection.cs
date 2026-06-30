using UnityEngine;

// Handles Room Selection submenu options.
// Assign the Renderer(s) of your room geometry to roomRenderers in the Inspector.
// Option ids: room_white, room_black, room_gray, room_blue, room_warm
public class RoomSelection : MonoBehaviour
{
    public RadialMenuController radialMenuController;

    [Tooltip("Renderers that make up the room (walls, floor, ceiling).")]
    public Renderer[] roomRenderers;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[RoomSelection] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        Color? color = option.id switch
        {
            "room_white" => Color.white,
            "room_black" => Color.black,
            "room_gray"  => new Color(0.35f, 0.35f, 0.35f),
            "room_blue"  => new Color(0.12f, 0.22f, 0.45f),
            "room_warm"  => new Color(0.55f, 0.42f, 0.30f),
            _ => (Color?)null
        };

        if (color == null) return;

        foreach (Renderer r in roomRenderers)
        {
            if (r != null) r.material.color = color.Value;
        }

        Debug.Log($"[RoomSelection] Room -> {option.id}");
    }
}
