using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Explosion
{
    public float radius;
    public int entityDamage;
    public int blockDamage;
    public float knockback;
    public AnimationCurve damageFalloff;
    public AnimationCurve knockbackFalloff;
    public LayerMask hitDetection;

    public void Detonate(Vector3 centreOfBlast)
    {
        Collider[] thingsHit = Physics.OverlapSphere(centreOfBlast, radius, hitDetection);
        for (int i = 0; i < thingsHit.Length; i++)
        {
            // Check to deal damage to enemies

            #region Check chunks and damage blocks accordingly
            // Check to deal damage to blocks
            Chunk chunkDetected = thingsHit[i].GetComponent<Chunk>();
            if (chunkDetected != null) // If a chunk collider is detected
            {
                List<Vector3Int> blocksToDamage = new List<Vector3Int>();
                Vector3Int lengths = chunkDetected.Dimensions;
                for (int x = 0; x < lengths.x; x++)
                {
                    for (int y = 0; y < lengths.y; y++)
                    {
                        for (int z = 0; z < lengths.z; z++)
                        {
                            // If the space is empty, skip
                            Vector3Int coords = new Vector3Int(x, y, z);
                            Debug.Log("Checking coordinates for explosion");
                            if (!chunkDetected.Block(coords).Exists)
                            {
                                continue;
                            }
                            // If block position is outside blast radius, skip
                            float distance = Vector3.Distance(centreOfBlast, chunkDetected.WorldPositionFromCoordinates(coords));
                            if (distance > radius)
                            {
                                continue;
                            }
                            blocksToDamage.Add(coords);
                        }
                    }
                }
                chunkDetected.DamageMultipleBlocks(blocksToDamage.ToArray(), blockDamage);
            }
            #endregion
        }
    }

    public Explosion(bool useDefaultData)
    {
        if (useDefaultData)
        {
            radius = 3;
            entityDamage = 10;
            blockDamage = 5;
            knockback = 50;
            damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);
            knockbackFalloff = AnimationCurve.Linear(0, 1, 1, 0);
            hitDetection = ~0;
        }
        else
        {
            this = new Explosion();
        }
    }
}
