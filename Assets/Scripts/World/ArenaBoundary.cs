using UnityEngine;

// Keep the existing physical walls at the visible edge, including after a resize.
public class ArenaBoundary : MonoBehaviour
{
    public Camera arenaCamera;
    public SpriteRenderer background;
    public BoxCollider2D leftWall;
    public BoxCollider2D rightWall;
    public BoxCollider2D topWall;
    public BoxCollider2D bottomWall;
    public float edgePadding = 0.15f;
    public float wallThickness = 1f;

    Rect previousArea;
    bool fitted;

    void Awake() => FitToView();
    void LateUpdate() => FitToView();

    public void FitToView()
    {
        if (arenaCamera == null || !arenaCamera.orthographic ||
            leftWall == null || rightWall == null || topWall == null || bottomWall == null)
            return;

        float depth = Vector3.Dot(-arenaCamera.transform.position, arenaCamera.transform.forward);
        Vector3 min = arenaCamera.ViewportToWorldPoint(new Vector3(0, 0, depth));
        Vector3 max = arenaCamera.ViewportToWorldPoint(new Vector3(1, 1, depth));
        if (background != null)
        {
            min = Vector3.Max(min, background.bounds.min);
            max = Vector3.Min(max, background.bounds.max);
        }

        Rect area = Rect.MinMaxRect(min.x + edgePadding, min.y + edgePadding,
            max.x - edgePadding, max.y - edgePadding);
        if (area.width <= 0 || area.height <= 0 || (fitted && area == previousArea)) return;

        float thickness = Mathf.Max(0.1f, wallThickness);
        Place(leftWall, new Vector2(area.xMin - thickness / 2, area.center.y),
            new Vector2(thickness, area.height + thickness * 2));
        Place(rightWall, new Vector2(area.xMax + thickness / 2, area.center.y),
            new Vector2(thickness, area.height + thickness * 2));
        Place(topWall, new Vector2(area.center.x, area.yMax + thickness / 2),
            new Vector2(area.width + thickness * 2, thickness));
        Place(bottomWall, new Vector2(area.center.x, area.yMin - thickness / 2),
            new Vector2(area.width + thickness * 2, thickness));
        ConfinePlayer(area);
        previousArea = area;
        fitted = true;
    }

    static void ConfinePlayer(Rect area)
    {
        // Resizing the view can move a wall past the player's current position.
        var player = FindAnyObjectByType<Player>();
        if (player == null) return;
        var collider = player.GetComponent<Collider2D>();
        if (collider == null) return;
        Bounds bounds = collider.bounds;
        float horizontal = Mathf.Max(0, area.width / 2 - bounds.extents.x);
        float vertical = Mathf.Max(0, area.height / 2 - bounds.extents.y);
        Vector2 correction = new Vector2(
            Mathf.Clamp(bounds.center.x, area.center.x - horizontal, area.center.x + horizontal) - bounds.center.x,
            Mathf.Clamp(bounds.center.y, area.center.y - vertical, area.center.y + vertical) - bounds.center.y);
        if (correction.sqrMagnitude < 0.000001f) return;
        var body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position += correction;
            body.linearVelocity = Vector2.zero;
        }
        else player.transform.position += (Vector3)correction;
    }

    static void Place(BoxCollider2D wall, Vector2 position, Vector2 size)
    {
        wall.transform.SetPositionAndRotation(new Vector3(position.x, position.y, 0), Quaternion.identity);
        wall.transform.localScale = Vector3.one;
        wall.offset = Vector2.zero;
        wall.size = size;
    }
}
