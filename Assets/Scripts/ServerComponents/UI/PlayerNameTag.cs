using Fusion;
using TMPro;
using UnityEngine;

public class PlayerNameTag : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private Vector3 _baseScale;

    public override void Spawned()
    {
        if (nameText != null)
            _baseScale = nameText.transform.parent.localScale;
    }

    public override void Render()
    {
        if (nameText == null) return;

        var data = GetComponent<PlayerDataManagerOnline>();
        if (data == null) return;

        nameText.text = data.PlayerName.ToString();

        // Giữ nguyên scale gốc (0.01), chỉ flip dấu x để counteract parent flip
        float flipX = transform.localScale.x >= 0 ? 1f : -1f;
        Transform nameTagTransform = nameText.transform.parent;
        nameTagTransform.localScale = new Vector3(_baseScale.x * flipX, _baseScale.y, _baseScale.z);
    }
}
