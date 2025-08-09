using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerStatsManager))]
[RequireComponent(typeof(PlayerStatsData))]
[RequireComponent(typeof(PlayerEventBus))]
public class PlayerContext : MonoBehaviour
{
    [field: SerializeField] public PlayerMovement Movement {get; private set;}
    [field: SerializeField] public PlayerCombat Combat {get; private set;}
    [field: SerializeField] public PlayerStatsManager StatsManager {get; private set;}
    [field: SerializeField] public PlayerStatsData Stats {get; private set;}
    [field: SerializeField] public PlayerEventBus EventBus {get; private set;}
    [field: SerializeField] public PlayerControl Control {get; private set;}
}
