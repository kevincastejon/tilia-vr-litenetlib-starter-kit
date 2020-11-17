﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
public class LightVariator : MonoBehaviour
{
    public void ChangeLight(Single amount)
    {
        RenderSettings.skybox.SetFloat("_Exposure", 1.25f*amount);
    }
}
