using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public enum WeaponType
    {
        Sword,
        Spear,
        Scythe,
        Fireball,
        Bullet
    }

    public WeaponType type;
    public GameObject prefab;
    public bool isActive;
    public float damage;
    public int upgradeLevel; // Track number of upgrades

    public WeaponData(WeaponType type, GameObject prefab, bool isActive = false)
    {
        this.type = type;
        this.prefab = prefab;
        this.isActive = isActive;
        this.upgradeLevel = 0; // Initialize upgrade level to 0
        
        // Set initial damage based on weapon type
        switch (type)
        {
            case WeaponType.Sword:
                damage = 75f;
                break;
            case WeaponType.Spear:
                damage = 60f;
                break;
            case WeaponType.Scythe:
                damage = 50f;
                break;
            case WeaponType.Fireball:
                damage = 75f;
                break;
            case WeaponType.Bullet:
                damage = 50f;
                break;
        }
    }

    public void UpgradeDamage()
    {
        damage *= 1.1f; // Increase damage by 10%
        upgradeLevel++; // Increment upgrade level
    }
} 