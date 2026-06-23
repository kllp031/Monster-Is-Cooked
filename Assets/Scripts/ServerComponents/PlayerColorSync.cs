using Fusion;
using UnityEngine;

/// <summary>
/// Assigns each online player a distinct scarf color and syncs it across all clients.
/// Colors are picked from a fixed palette spaced evenly around the hue wheel (avoids red).
/// Requires the "Custom/SwapWhiteColor" shader.
/// Attach this component to the OnlinePlayer prefab and register it in the NetworkObject.
/// </summary>
public class PlayerColorSync : NetworkBehaviour
{
    // 8 hues spread evenly around the wheel, none close to red (the scarf's original color).
    // Hue 0/1 = red is the scarf base, so we skip that zone intentionally.
    private static readonly float[] Hues =
    {
        0.58f,  // blue
        0.33f,  // green
        0.75f,  // purple
        0.15f,  // yellow
        0.50f,  // teal
        0.90f,  // pink
        0.42f,  // cyan-green
        0.08f,  // orange
    };

    // Packed as 0x00RRGGBB (3 bytes). Networked so all clients see the same color.
    [Networked] private int PackedColor { get; set; }

    private SpriteRenderer _sr;
    private Material _mat;
    private int _lastPackedColor = -1;

    public override void Spawned()
    {
        _sr = GetComponent<SpriteRenderer>();

        if (Object.HasStateAuthority)
        {
            // Host player keeps PackedColor = 0 (sentinel: no swap, original scarf)
            // Game uses Fusion Shared Mode — Runner.IsServer is always false, use IsSharedModeMasterClient instead
            bool isHostPlayer = Runner.IsSharedModeMasterClient && Object.HasInputAuthority;
            if (!isHostPlayer)
            {
                float hue = Hues[Object.InputAuthority.PlayerId % Hues.Length];
                PackedColor = Pack(Color.HSVToRGB(hue, 0.85f, 1f));
            }
        }
    }

    public override void Render()
    {
        if (PackedColor == _lastPackedColor) return;
        _lastPackedColor = PackedColor;

        // PackedColor == 0 means "keep original scarf color" (host player)
        if (PackedColor == 0) return;

        if (_mat == null)
        {
            var shader = Shader.Find("Custom/SwapWhiteColor");
            if (shader == null)
            {
                Debug.LogError("[PlayerColorSync] Shader 'Custom/SwapWhiteColor' not found.");
                return;
            }
            _mat = new Material(shader);
            _sr.material = _mat;
        }

        _mat.SetColor("_PlayerColor", Unpack(PackedColor));
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static int Pack(Color c)
    {
        return ((int)(c.r * 255) << 16)
             | ((int)(c.g * 255) << 8)
             | (int)(c.b * 255);
    }

    private static Color Unpack(int packed)
    {
        return new Color(
            ((packed >> 16) & 0xFF) / 255f,
            ((packed >>  8) & 0xFF) / 255f,
            ( packed        & 0xFF) / 255f
        );
    }
}
