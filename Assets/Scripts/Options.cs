/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class Options : MonoBehaviour {
	public static PXCUPipeline.Mode mode=PXCUPipeline.Mode.GESTURE;
	
    void OnGUI() {
		if (GUI.Button (new Rect (10,10,120,30),((mode&(PXCUPipeline.Mode.COLOR_VGA|PXCUPipeline.Mode.FACE_LANDMARK|PXCUPipeline.Mode.FACE_LOCATION))!=0)?"COLOR":"NO COLOR")) {
			mode^=PXCUPipeline.Mode.COLOR_VGA;
			Norm ();
			Application.LoadLevel(0);
		}
		if (GUI.Button (new Rect (10,40,120,30),((mode&(PXCUPipeline.Mode.DEPTH_QVGA|PXCUPipeline.Mode.GESTURE))!=0)?"DEPTH":"NO DEPTH")) {
			mode^=PXCUPipeline.Mode.DEPTH_QVGA;
			Norm ();
			Application.LoadLevel(0);
		}
		if (GUI.Button (new Rect (10,70,120,30),((mode&PXCUPipeline.Mode.GESTURE)!=0)?"GESTURE":"NO GESTURE")) {
			mode^=PXCUPipeline.Mode.GESTURE;
			Norm ();
			Application.LoadLevel(0);
		}
		/*if (GUI.Button (new Rect (10,100,120,30),((mode&PXCUPipeline.Mode.FACE_LOCATION)!=0)?"FACE":"NO FACE")) {
			mode^=(PXCUPipeline.Mode.FACE_LOCATION|PXCUPipeline.Mode.FACE_LANDMARK);
			Norm ();
			Application.LoadLevel(0);
		}
		if (GUI.Button (new Rect (10,130,120,30),((mode&PXCUPipeline.Mode.VOICE_RECOGNITION)!=0)?"VOICE":"NO VOICE")) {
			mode^=PXCUPipeline.Mode.VOICE_RECOGNITION;
			Norm ();
			Application.LoadLevel(0);
		}*/
	}
	
	void Norm() {
		if ((mode&PXCUPipeline.Mode.FACE_LANDMARK)!=0) mode|=PXCUPipeline.Mode.FACE_LOCATION;
		if ((mode&PXCUPipeline.Mode.FACE_LOCATION)!=0) mode|=PXCUPipeline.Mode.COLOR_VGA;
		if ((mode&PXCUPipeline.Mode.GESTURE)!=0) mode|=PXCUPipeline.Mode.DEPTH_QVGA;
	}
}
