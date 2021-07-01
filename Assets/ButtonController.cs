/* Purpose: Handle click for altitude mode button */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {

    // Button click event
    public void Button_Click() {
        // If the camera is not on, then turn it on, otherwise turn it off.
        if (!GetComponent<Camera>().isActiveAndEnabled) {
            GetComponent<Camera>().enabled = true;
        } else {
            GetComponent<Camera>().enabled = false;
        }
    }
}
