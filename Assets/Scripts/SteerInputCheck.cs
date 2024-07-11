using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Content.Interaction;
using TMPro;

public class SteerInputCheck : MonoBehaviour
{
    public XR_Knob_Working XR_Knob;
    public CarController carController;
    private float Steer;
    public TextMeshProUGUI SteerValue;

    // Start is called before the first frame update
    void Start()
    {
        SetSteer(XR_Knob.value);
        XR_Knob.onValueChange.AddListener(SetSteer);

        SteerValue.text = "0.00";
    }

    void Update()
    {
        SetSteer(XR_Knob.value);
    }

    // Update is called once per frame
    public void SetSteer(float value)
    {
        //Making values go from 0 - 1  to  -1 - 1
        Steer = value;
        Steer -= 0.5f;
        Steer *= 2f;

        carController.SteerInput(Steer);

        SteerValue.text = Steer.ToString("0.00");
    }
}
