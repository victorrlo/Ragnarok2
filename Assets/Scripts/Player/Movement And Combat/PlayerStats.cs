using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int _maxHP = 100;
    [SerializeField] private int _maxSP = 50;

    public int CurrentHP {get; private set;}
    public int CurrentSP {get; private set;}

    private void Awake()
    {
        CurrentHP = _maxHP;
        CurrentSP = _maxSP;
        HUDManager.Instance.UpdateHUD(CurrentHP, _maxHP, CurrentSP, _maxSP);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        HUDManager.Instance.UpdateHUD(CurrentHP, _maxHP, CurrentSP, _maxSP);
        if(CurrentHP <= 0)
        {
            Die();
        }
    }

    public void UseSP(int amount)
    {
        CurrentSP -= amount;
        CurrentSP = Mathf.Max(CurrentSP, 0);
        HUDManager.Instance.UpdateHUD(CurrentHP, _maxHP, CurrentSP, _maxSP);
    }

    private void Die()
    {
        Debug.Log("Player morreu");
    }
}
