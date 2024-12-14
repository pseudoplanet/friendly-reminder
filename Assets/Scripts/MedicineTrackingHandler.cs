using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineTrackingHandler : MonoBehaviour
{   
    // the tracker is implemented with a singleten approach to make it easier to access
    private static MedicineTrackingHandler _instance;
    public static MedicineTrackingHandler Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("You messed up buddy.");
                Destroy(value);
            }
        }
    }

    // dosage frequency is ALWAYS in terms of hours
    // the user might want to put days in instead though, so in that case the multiplier will be used to conver
    // basically it's 1 if using hours, or 24 if using days
    public double dosageFrequencyMultiplier;
    public List<TrackedDose> trackedDoses;
    
    // Set up the singleton implmenentation
    private void Awake() {
        Instance = this;
        trackedDoses = new List<TrackedDose>();
    }

    public void CreateTestTrackers(int _count) {
        for (int i = 0; i < _count; i++) {
            AddNewTrackerToList(new TrackedDose("Test Name", "Test Sci Name", Random.Range(0, 2), Random.Range(1, 100), Random.Range(1, 100)));
        }
    }

    public void AddNewTrackerToList(TrackedDose newDose) {
        trackedDoses.Add(newDose);

        UIManager.Instance.UpdateTrackedDoses(trackedDoses);
    }
}
