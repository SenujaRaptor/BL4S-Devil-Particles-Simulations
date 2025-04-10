using UnityEngine;
using System.Collections; // Needed for coroutine

public class CosmicRaySpawner : MonoBehaviour
{
    public GameObject muonPrefab;
    public GameObject nonMuonPrefab;

    public int totalParticles = 100;
    [Range(0f, 1f)]
    public float muonProbability = 0.75f;

    public float spawnRadius = 1f;
    public float spawnHeight = 10f;

    public float spawnDelay = 0.5f; // Time between spawns (in seconds)

    void Start()
    {
        StartCoroutine(SpawnParticlesWithDelay());
    }

    IEnumerator SpawnParticlesWithDelay()
    {
        for (int i = 0; i < totalParticles; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

            GameObject prefabToSpawn = Random.value < muonProbability ? muonPrefab : nonMuonPrefab;
            GameObject spawnedParticle = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

            // Assign random energy
            EnergyAssign particleEnergy = spawnedParticle.GetComponent<EnergyAssign>();
            if (particleEnergy != null)
            {
                float rand = Random.value;
                float energyValue = 0;

                if (rand < 0.75f) // 75% chance: Low energy muons
                {
                    energyValue = Random.Range(200f, 1200f);
                }
                else if (rand < 0.95f) // Next 20%: Medium energy muons
                {
                    energyValue = Random.Range(1200f, 5000f);
                }
                else // Remaining 5%: High energy muons
                {
                    energyValue = Random.Range(5000f, 10000f);
                }            
                particleEnergy.energy = energyValue;
            }


            yield return new WaitForSeconds(spawnDelay);
        }
    }
}

