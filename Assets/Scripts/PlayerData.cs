using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{    
    public float[] Position;
    public int MaxHealth;
    public int Health;
    public float MoveSpeed;
    public int Kills;
    public WeaponName Weapon1Name, Weapon2Name;
    public List<SkillType> UnlockedSkillTypes;

    public override string ToString()
    {
        return $"[PlayerData]\nPosition: {Position[0]},{Position[1]},{Position[2]}\nHealth: {Health}\nWeapon1: {Weapon1Name}\nWeapon2: {Weapon2Name}";
    }
}
