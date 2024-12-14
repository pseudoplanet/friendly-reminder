using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System;
using EAGetMail;
using System.Linq;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance {
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
    // Set up the singleton implmenentation
    private void Awake() {
        Instance = this;
    }
    
    public GameObject[] widgets;
    public Transform canvasTransform;
    public TextMeshProUGUI dateDisplay;

    public Sprite[] doseIcons;

    // object to act as a parent for all tracker objects
    public GameObject trackerContainer;
    public GameObject trackerPrefab;
    public float trackerSpacing;
    public float trackerScrollSpeed;

    public GameObject friendsContainer;

    void Start() {
        MedicineTrackingHandler.Instance.CreateTestTrackers(10);
        ShowOnlyWidget(1);
    }

    public void NewMedicineButtonPressed() {
        ShowWidget(0);
        SetupUIForDoseType(0);

        // the default unit for dosage frequency should be hours
        SetDoseFrequencyUnits(0);
    }

    public void ConfirmLogin() {
        HideWidget(1);
        MailHandler.Instance.isLoggedIn = true;

        MailHandler.Instance.emailDisplay.text = MailHandler.Instance.accountUsername;
    }

    void Update() {
        dateDisplay.text = DateTime.Now.Date.ToString().Substring(0, 10);

        for (int i = 0; i < trackerContainer.transform.childCount; i++) {
            trackerContainer.transform.GetChild(i).position += Vector3.up * (float)Input.GetAxis("Mouse ScrollWheel") * trackerScrollSpeed;
        }
    }

    public void AddFriendButtonPressed() {

    }

    public void AddDoseButtonPressed() {
        GameObject widgetObject = widgets[0];
        TrackedDose newDose = new TrackedDose();

        // fill in the data class with info that the user has provided
        newDose.doseType = CanvasUtils.SearchChildrenForName(widgetObject, "type").GetComponent<TMP_Dropdown>().value;
        newDose.doseFrequency = int.Parse(CanvasUtils.SearchChildrenForName(widgetObject, "frequency").GetComponent<TMP_InputField>().text) * MedicineTrackingHandler.Instance.dosageFrequencyMultiplier;

        newDose.commonName = CanvasUtils.SearchChildrenForName(widgetObject, "common name").GetComponent<TMP_InputField>().text;
        newDose.scientificName = CanvasUtils.SearchChildrenForName(widgetObject, "sci name").GetComponent<TMP_InputField>().text;

        if (newDose.doseType == (int)DoseType.Liquid) {
            newDose.doseMeasurement = double.Parse(CanvasUtils.SearchChildrenForName(widgetObject, "quantity").GetComponent<TMP_InputField>().text);
        }
        else {
            newDose.doseQuantity = double.Parse(CanvasUtils.SearchChildrenForName(widgetObject, "quantity").GetComponent<TMP_InputField>().text);
        }

        Debug.Log("Added new tracked dose." +
         " Type: " + newDose.doseType +
          ". Freq: " + newDose.doseFrequency +
           ". Start Date: " + newDose.firstDose + ".");

        MedicineTrackingHandler.Instance.AddNewTrackerToList(newDose);
    }

    public void UpdateTrackedDoses(List<TrackedDose> _input) {
        // delete all existing trackers, easier to start with a clean slate
        CanvasUtils.DestroyChildren(trackerContainer);

        // create UI elements for all trackers in the supplied list
        for (int i = 0; i < _input.Count; i++) {
            GameObject newTracker = Instantiate(trackerPrefab, Vector3.zero, Quaternion.identity);
            newTracker.transform.SetParent(trackerContainer.transform);

            newTracker.transform.localPosition = -Vector3.up * i * trackerSpacing;

            // displaying all the info like type, name, etc.
            _input[i].SetupWidget(newTracker);
        }
    }

    public void SetupUIForDoseType(TMP_Dropdown _input) {
        SetupUIForDoseType(_input.value);
    }
    void SetupUIForDoseType(int _type) {
        CanvasUtils.SearchChildrenForName(widgets[0], "icon").GetComponent<Image>().sprite = doseIcons[_type];

        if (_type == 0) {
            CanvasUtils.SearchChildrenForName(widgets[0], "quantity title").GetComponent<TextMeshProUGUI>().text = "Dosage Amount (ml)";
        } else if (_type == 1) {
            CanvasUtils.SearchChildrenForName(widgets[0], "quantity title").GetComponent<TextMeshProUGUI>().text = "Number of Pills";
        } else if (_type == 2) {
            CanvasUtils.SearchChildrenForName(widgets[0], "quantity title").GetComponent<TextMeshProUGUI>().text = "Number of Pills";
        }
    }

    public void SetDoseFrequencyUnits(TMP_Dropdown _input) {
        SetDoseFrequencyUnits(_input.value);
    }
    void SetDoseFrequencyUnits(int _type) {
        // if we're working with hours (type == 0) then we don't need a multiplier
        // if days (type == 1), we need a multipler to convert back to hours
        MedicineTrackingHandler.Instance.dosageFrequencyMultiplier = (_type == 0) ? 1 : 24;
    }

    public void ShowWidget(int _id) {
        widgets[_id].SetActive(true);
    }
    public void HideWidget(int _id) {
        widgets[_id].SetActive(false);
    }
    public void ShowOnlyWidget(int _id) {
        for (int i = 0; i < widgets.Length; i++) {
            if (i == _id) {
                widgets[i].SetActive(true);
            }
            else {
                widgets[i].SetActive(false);
            }
        }
    }
}
