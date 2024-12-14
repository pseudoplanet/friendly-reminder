using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

// data class for tracking medicine doses
public class TrackedDose
{   
    // consumer name of the drug (e.g. "Tylenol")
    public string commonName;
    // name of the drug (e.g. "ibuprofen")
    public string scientificName;

    // the type, either liquid, breakable, or capsule
    public int doseType;

    // NOT ALWAYS USED
    // the measured amount of medicine (either ml or g)
    public double doseMeasurement;
    // how many pills, capsules, etc. to take each time
    public double doseQuantity;

    // the date that the dose started being tracked
    public DateTime dateAdded;
    // The date and time that the first dose should be taken
    public DateTime firstDose;

    // how many HOURS in between doses
    // this number of hours is added to the first dose again and again to get all future doses
    public double doseFrequency;

    public TrackedDose() {
    }

    // frequency is in hours
    public TrackedDose(string _name, string _sciName, int _type, double _amt, double _freq) {
        commonName = _name;
        scientificName = _sciName;
        doseType = _type;
        doseMeasurement = doseType == (int)DoseType.Liquid ? _amt : 0;
        doseQuantity = doseType != (int)DoseType.Liquid ? _amt : 0;
        doseFrequency = _freq;
    }

    // // a function for figuring out how
    // public double GetHoursUntilNextDose() {
    //     DateTime currentDose = firstDose; 
        
    //     // increment the date of the dose by the frequency until we get to whatever's after today's date

        
    // }

    public void SetupWidget(GameObject _widget) {
        // the icon for the dose
        CanvasUtils.SearchChildrenForName(_widget, "preview").GetComponent<Image>().sprite = UIManager.Instance.doseIcons[doseType];

        // names (sci and common)
        CanvasUtils.SearchChildrenForName(_widget, "sci name").GetComponent<TextMeshProUGUI>().text = scientificName;
        CanvasUtils.SearchChildrenForName(_widget, "common name").GetComponent<TextMeshProUGUI>().text = commonName;

        // dosage stuff
        double quantity = doseType == (int)DoseType.Liquid ? doseMeasurement : doseQuantity;
        CanvasUtils.SearchChildrenForName(_widget, "dose amt").GetComponent<TextMeshProUGUI>().text = 
        quantity.ToString() + 
        (doseType == (int)DoseType.Liquid ? " ml" : " pill") + 
        (quantity > 1 && doseType != (int)DoseType.Liquid ? "s" : "");
    }
}

public enum DoseType {
    Liquid,
    Breakable,
    Capsule,
}
