using TMPro;
using UnityEngine;

public class BlueEnergyDisplay : MonoBehaviour
{
    public float energy;
    public float airLossRate = 2f;
    private Rigidbody rb;

    public GameObject energyLabelPrefab;
    private TextMeshPro energyLabel;

    private bool hasHitLead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (energyLabelPrefab != null)
        {
            GameObject labelObj = Instantiate(energyLabelPrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
            labelObj.transform.SetParent(transform);
            energyLabel = labelObj.GetComponent<TextMeshPro>();

            if (energyLabel == null)
            {
                Debug.LogError("Energy Label Prefab missing TextMeshPro component.");
            }
            else
            {
                energyLabel.text = $"{energy:F1} MeV";
            }
        }
    }

    void Update()
    {
        if (energy > 0)
        {
            float distancePerFrame = rb.linearVelocity.magnitude * Time.deltaTime;
            energy -= airLossRate * distancePerFrame;
            energy = Mathf.Max(energy, 0f);
        }

        if (energyLabel != null)
        {
            energyLabel.text = $"{energy:F1} MeV";
            energyLabel.transform.position = transform.position + Vector3.up * 0.6f;

            if (Camera.main != null)
                energyLabel.transform.rotation = Quaternion.LookRotation(energyLabel.transform.position - Camera.main.transform.position);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("LeadBarrier"))
        {
            // Non-muon or stopped muon gets destroyed after 2 seconds
            Destroy(gameObject, 0.5f);          
        }

    }
}
