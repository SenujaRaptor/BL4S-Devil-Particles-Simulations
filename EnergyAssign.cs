using TMPro;
using UnityEngine;
using System.IO;
using System.Collections;

public class EnergyAssign : MonoBehaviour
{
    public float energy;
    public float airLossRate = 206f;
    private Rigidbody rb;
    private ConeOneSpawn Cone1;
    public GameObject energyLabelPrefab;
    private TextMeshPro energyLabel;
    private float cherenkovAngleDegrees;
    private float cherenkovAngleDegreesE;
    private bool hasHitLead = false;
    const double MuonRestLifetime = 2.2e-6; // seconds
    const double MuonRestMassMeV = 105.7;
    private ValuesExport Values;
    private float lastVelocity;
    private double lastDecayTime;
    private string csvFilePath;
    private bool passedThroughLead = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Transform Particlesystem = this.transform.GetChild(0);
        Particlesystem.gameObject.SetActive(false);

        Transform ParticlesystemE = this.transform.GetChild(2);
        ParticlesystemE.gameObject.SetActive(false);
        Transform Electron = this.transform.GetChild(1);
        Electron.gameObject.SetActive(false);
        Values = FindObjectOfType < ValuesExport>();

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
        // existing setup...

        csvFilePath = Application.persistentDataPath + "/MuonsData.csv";



        
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
            ReduceEnergy();

            if (energy > 850)
            {
                this.gameObject.GetComponent<Collider>().isTrigger = true;
                Debug.Log("Went through the Lead");

                // Save to CSV
                passedThroughLead = true; // set the flag to log later
            }
            else
            {
                // Non-muon or stopped muon gets destroyed after 2 seconds
                Destroy(gameObject, 0.5f);
            }
        }

    }
    private void ReduceEnergy()
    {
        energy -= 850;
    }

    public void LogDataToCSV(float energy, double decayTime, float velocity)
    {
        csvFilePath = Application.persistentDataPath + "/MuonsData.csv";

        if (!File.Exists(csvFilePath))
        {
            // Write headers if file doesn't exist
            using (StreamWriter writer = new StreamWriter(csvFilePath, false))
            {
                writer.WriteLine("Energy (MeV),Decay Time (s),Velocity (fraction of c)");
            }
        }

        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine($"{energy:F2},{decayTime:F6},{velocity:F4}");
        }

        Debug.Log("Data written to: " + csvFilePath);
    }



    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Scintillator"))
        {
            Transform Particlesystem = this.transform.GetChild(0);
            Particlesystem.gameObject.SetActive(true);

            lastDecayTime = GetMuonDecayTime(energy); // assign decay time globally

            ParticleSystem ps = Particlesystem.gameObject.GetComponent<ParticleSystem>();
            var shapee = ps.shape;
            shapee.angle = cherenkovAngleDegrees;

            float Velocity = Mathf.Sqrt(1f - Mathf.Pow(105.66f / energy, 2));
            lastVelocity = Velocity;

            Debug.Log($"Decay Time: {lastDecayTime}, Velocity: {lastVelocity}");

            Invoke("generateElectron", (float)lastDecayTime);
        }


    }

    private void generateElectron()
    {
        Transform Electron=this.transform.GetChild(1);
        Transform ParticlesystemE = this.transform.GetChild(2);

        GetComponent<MeshRenderer>().enabled = false;

        Electron.gameObject.SetActive(true);
        ParticlesystemE.gameObject.SetActive(true);

        ParticleSystem psE = ParticlesystemE.gameObject.GetComponent<ParticleSystem>();
        var shapeE = psE.shape;
        shapeE.angle = cherenkovAngleDegreesE;

        Debug.Log(shapeE.angle);

        if (passedThroughLead)
        {
            LogDataToCSV(energy, lastDecayTime, lastVelocity);
            Debug.Log("Data written to: " + csvFilePath);
        }



    }
    public void CalculateCherenkovAngle()
    {
        // Convert energy to MeV for calculations
        

        // Calculate Lorentz factor (γ = E / mc²) 
        float gamma = energy / 105.66f; //rest mass of muon=105.66

        // Calculate β (v/c) = sqrt(1 - 1/γ²)
        float beta = Mathf.Sqrt(1f - 1f / (gamma * gamma));

        // Check if particle is above Cherenkov threshold (β > 1/n)
        bool isAboveThreshold = (beta > (1f / 1.33)); // 1.33=refractive index

        if (isAboveThreshold)
        {
            // Calculate cos(θ_C) = 1 / (n * β)
            float cosThetaC = 1f / (1.33f * beta);

            // Clamp to avoid floating-point errors (e.g., if β ≈ 1)
            cosThetaC = Mathf.Clamp(cosThetaC, -1f, 1f);

            // Calculate θ_C in radians, then convert to degrees
            float thetaCRadians = Mathf.Acos(cosThetaC);
            cherenkovAngleDegrees = thetaCRadians * Mathf.Rad2Deg;
        }
        else
        {
            cherenkovAngleDegrees = 0f; // No Cherenkov radiation
        }

    }
    public void CalculateCherenovAngleE()
    {


        float a = Random.Range(50f, 70f);

        // Calculate Lorentz factor (γ = E / mc²) 

        float gamma = (energy*a*0.01f) / 0.511f; //rest mass of muon=105.66
       
        // Calculate β (v/c) = sqrt(1 - 1/γ²)
        float beta = Mathf.Sqrt(1f - 1f / (gamma * gamma));

        // Check if particle is above Cherenkov threshold (β > 1/n)
        bool isAboveThreshold = (beta > (1f / 1.33)); // 1.33=refractive index

        if (isAboveThreshold)
        {
            // Calculate cos(θ_C) = 1 / (n * β)
            float cosThetaC = 1f / (1.33f * beta);

            // Clamp to avoid floating-point errors (e.g., if β ≈ 1)
            cosThetaC = Mathf.Clamp(cosThetaC, -1f, 1f);

            // Calculate θ_C in radians, then convert to degrees
            float thetaCRadians = Mathf.Acos(cosThetaC);
            cherenkovAngleDegreesE = thetaCRadians * Mathf.Rad2Deg;
            Debug.Log("elecron produced");
        }
        else
        {
            cherenkovAngleDegreesE = 0f; // No Cherenkov radiation
        }

    }
    private double GetMuonDecayTime(double muonEnergyMeV)
    {
        // Calculate gamma = E / (mc^2)
        double gamma = muonEnergyMeV / MuonRestMassMeV;

        // Time-dilated lifetime
        double boostedLifetime = gamma * MuonRestLifetime;

        // Sample from exponential distribution
        float u = Random.Range(0f, 1f);  // UnityEngine.Random gives a float between 0 and 1
      
        double decayTime = -boostedLifetime * Mathf.Log(u);
    
        
        //Values.LogFloatValue((float) decayTime); 
      
        
        return decayTime; // in seconds
        Debug.Log("time...");

    }


}



