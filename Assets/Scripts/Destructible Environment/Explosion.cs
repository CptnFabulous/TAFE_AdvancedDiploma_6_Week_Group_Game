using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Explosion
{
    public float radius = 3;
    public int entityDamage = 10;
    public int blockDamage = 5;
    public float knockback = 50;
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);
    public AnimationCurve knockbackFalloff = AnimationCurve.Linear(0, 1, 1, 0);
    public LayerMask hitDetection = ~0;

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
}
