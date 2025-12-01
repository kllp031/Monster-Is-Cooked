using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/LevelDetails")]
public class LevelDetails : ScriptableObject
{
    [SerializeField] List<CustomerDetail> customerDetails = new();

    public List<CustomerDetail> CustomerDetails { get => customerDetails; }
}
