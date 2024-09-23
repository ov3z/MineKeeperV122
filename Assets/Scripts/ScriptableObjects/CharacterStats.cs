using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Character Stats",menuName = "Stats/CharacterStats")]
public class CharacterStats : ScriptableObject
{
    public float speed;
    public float power;
    public float health;
    public float damage;
    public float attackRange;
    public int capacity;
}
