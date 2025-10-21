#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.System;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using FTOptix.WebUI;
#endregion

public class WriteCurseCurveDatatoVariable : BaseNetLogic
{
    private float[,] tempCurveCoord;
    private float[,] normals;
    private float VisualCamModifier;

    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        DrawCourseCurve();

        DrawMotionData();

        // Start the asynchronous task to draw cam data

        DrawCamDataAsync = new LongRunningTask(() => DrawCamData(), LogicObject);
        DrawCamDataAsync.Start();
        
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        DrawCamDataAsync?.Dispose();
    }

    public void DrawCourseCurve()
    {
         //Read the array from the PLC
        var CourseCurve = Project.Current.GetVariable("Model/CourseCurve/arrCourseCurve");
        var CenterX = Project.Current.GetVariable("Model/CourseCurve/CenterX");
        var CenterZ = Project.Current.GetVariable("Model/CourseCurve/CenterZ");
        var PLC = LogicObject.GetVariable("PLC").Value.Value.ToString(); //Either CLX or VP1 or VP2

        var zoomFactor = Project.Current.GetVariable("Model/CourseCurve/ZoomFactor");
        var StartSealingLabel = Project.Current.GetVariable("Model/CourseCurve/StartSealingLabel");
        var EndSealingLabel = Project.Current.GetVariable("Model/CourseCurve/EndSealingLabel");
        var WaitingPointLabel = Project.Current.GetVariable("Model/CourseCurve/WaitingPointLabel");


        var x_Pos_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Program:VP_Calc_PLS_Cams/X_Tab_mm");
        var z_Pos_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Program:VP_Calc_PLS_Cams/Z_Tab_mm");
        var arr_x_pos = x_Pos_tag.RemoteRead();
        var arr_z_pos = z_Pos_tag.RemoteRead();

        // Values in degree of significant points on the Course curve
        //Sealing Begin is always 0
        //var StartSealing_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>("CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/XZ_Profile/P1_SealingBegin/Master_deg");
        var EndSealing_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Controller Tags/XZ_Profile/P2_SealingEnd/Master_deg");
        var WaitingPoint_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Controller Tags/XZ_Profile/PS_WaitingPointMachine/Master_deg");
        //var StartSealing_deg = StartSealing_tag.RemoteRead();
        var EndSealing_deg = EndSealing_tag.RemoteRead();
        var WaitingPoint_deg = WaitingPoint_tag.RemoteRead();
        //Convert to nearest degree by rounding to integer
        //StartSealing_deg = (int)Math.Round((double)StartSealing_deg);
        EndSealing_deg = (int)Math.Round((double)EndSealing_deg);
        WaitingPoint_deg = (int)Math.Round((double)WaitingPoint_deg);

        tempCurveCoord = (float[,])CourseCurve.Value.Value;
        float[] xArray = (float[])arr_x_pos.Value;
        float[] zArray = (float[])arr_z_pos.Value;
        float z_reference = zArray[0];

        // Initialize min/max with first values
        float minX = xArray[0], maxX = xArray[0];
        float minZ = zArray[0] - z_reference, maxZ = zArray[0] - z_reference;

        normals = new float[361, 2];

        for (int i = 0; i < 361; i++)
        {
            float x = xArray[i];
            float z = zArray[i] - z_reference;

            tempCurveCoord[i, 0] = x;
            tempCurveCoord[i, 1] = z;

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;

            // Calculate normal vector in the same loop
            int prev = (i == 0) ? i : i - 1;
            int next = (i == 360) ? i : i + 1;

            float dx = xArray[next] - xArray[prev];
            float dz = (zArray[next] - z_reference) - (zArray[prev] - z_reference);

            // Tangent vector (dx, dz), normal is (-dz, dx)
            float nx = dz;
            float nz = -dx;

            // Normalize
            float length = (float)Math.Sqrt(nx * nx + nz * nz);
            if (length > 0)
            {
                nx /= length;
                nz /= length;
            }
            else
            {
                nx = 0;
                nz = 0;
            }

            normals[i, 0] = nx;
            normals[i, 1] = nz;
        }

        // Update the model variable with the new value
        CourseCurve.SetValue(tempCurveCoord);
        CenterX.Value = (minX + maxX) / 2;
        CenterZ.Value = (minZ + maxZ) / 2;

        // Zoom calculation:
        // Zoom = 0.5 --> Z Axis height is 1500
        // Zoom = 1 --> Z Axis height is 750
        // Zoom = 2 --> Z-Axis height is 375
        // In order to have some space at the top and bottom of the curve, we need to adjust the zoom factor by one third
        float zoomZ = (750 * 3) / (4 * (maxZ - minZ));
        float zoomX = 870/(2*(maxX - minX)); // 870 is the width of the X-Axis
        // Smaller zoomfactor is chosen as content could be out of the window
        if (zoomX <= zoomZ)
            zoomFactor.Value = zoomX;
        else
            zoomFactor.Value = zoomZ;



        // after some testing the CamData visualization is exactly inverse to the zoom factor
        // Zoom 1 - VisualCamModifier 6
        // Zoom 2 - VisualCamModifier 3
        // Zoom 3 - VisualCamModifier 2
        // Zoom 6 - VisualCamModifier 1
        VisualCamModifier = 6 / zoomFactor.Value;


        // Set the start sealing label coordinates
        // The center of the XY Diagram is 477/362. These correspond to Centerx/CenterZ
        // Zoom Factor (currently fixed on 6) is used to scale the coordinates
        // Example: if CenterX is 10 and the X coordinate of the start sealing label is 50,
        // then the X coordinate in the XY Diagram will be 477 + (50 - 10) * 6 = 477 + 240 = 717

        //The Panel containing the labels have a fixed size of 210x50 pixels. 
        //The point that should be on the curve has a diameter of 10 pixels.
        //Therefore, the values of the panel are shifted by 5 pixels
        // Start Sealing is in the top left, End Sealing Bottom left and waiting Position top right

        var StartSealingCoord = (float[])StartSealingLabel.Value.Value;
        StartSealingCoord[0] = (xArray[0] - CenterX.Value) * zoomFactor.Value + 477 - 205; // 477 is the center X coordinate of the XY Diagram
        StartSealingCoord[1] = (zArray[0] - z_reference - CenterZ.Value) * zoomFactor.Value + 362 + 45; // 362 is the center Y coordinate of the XY Diagram
        StartSealingLabel.SetValue(StartSealingCoord);

        var EndSealingCoord = (float[])EndSealingLabel.Value.Value;
        EndSealingCoord[0] = (xArray[EndSealing_deg] - CenterX.Value) * zoomFactor.Value + 477 - 205;
        EndSealingCoord[1] = (zArray[EndSealing_deg] - z_reference - CenterZ.Value) * zoomFactor.Value + 362 + 5;
        EndSealingLabel.SetValue(EndSealingCoord);

        var WaitingPointCoord = (float[])WaitingPointLabel.Value.Value;
        WaitingPointCoord[0] = (xArray[WaitingPoint_deg] - CenterX.Value) * zoomFactor.Value + 477 - 5;
        WaitingPointCoord[1] = (zArray[WaitingPoint_deg] - z_reference - CenterZ.Value) * zoomFactor.Value + 362 + 45;
        WaitingPointLabel.SetValue(WaitingPointCoord);
    }

    public void DrawMotionData()
    {
        var PLC = LogicObject.GetVariable("PLC").Value.Value.ToString(); //Either CLX or VP1 or VP2
        var VelocityTag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Program:VP_Calc_PLS_Cams/Speed_Tab");
        var AccelerationXTag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Program:VP_Calc_PLS_Cams/X_Acceleration_Tab");
        var AccelerationZTag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Program:VP_Calc_PLS_Cams/Z_Acceleration_Tab");
        var arrVelocity = (float[])VelocityTag.RemoteRead().Value;
        var arrAccelerationX = (float[])AccelerationXTag.RemoteRead().Value;
        var arrAccelerationZ = (float[])AccelerationZTag.RemoteRead().Value;

        float[,] velocityCoords = new float[721, 2];
        float[,] accelerationCoords = new float[721, 2];

        // Copy tempCurveCoord to the first 361 entries
        for (int i = 0; i < 361; i++)
        {
            velocityCoords[i, 0] = tempCurveCoord[i, 0];
            velocityCoords[i, 1] = tempCurveCoord[i, 1];
            accelerationCoords[i, 0] = tempCurveCoord[i, 0];
            accelerationCoords[i, 1] = tempCurveCoord[i, 1];
        }

        // Calculate the remaining 360 entries using the normal vectors and the velocity/acceleration values
        for (int i = 0; i < 360; i++)
        {
            int idx = i + 361;
            // Velocity and Acceleration values have to be modified to be properly visible
            int iHMIFactorVelocity = 4;
            // Velocity
            velocityCoords[idx, 0] = tempCurveCoord[i, 0] + normals[i, 0] * arrVelocity[i]* iHMIFactorVelocity;
            velocityCoords[idx, 1] = tempCurveCoord[i, 1] + normals[i, 1] * arrVelocity[i]* iHMIFactorVelocity;

            int iHMIFactorAcceleration = 80;


            // Acceleration


            // Direction vector from prev to i
            int prev = (i == 0) ? 360 : i - 1;
            float dx = tempCurveCoord[i, 0] - tempCurveCoord[prev, 0];
            float dz = tempCurveCoord[i, 1] - tempCurveCoord[prev, 1];
            float dirLength = (float)Math.Sqrt(dx * dx + dz * dz);

            float dirX = dirLength > 0 ? dx / dirLength : 0;
            float dirZ = dirLength > 0 ? dz / dirLength : 0;

            // Acceleration vector at i
            float accX = arrAccelerationX[i];
            float accZ = arrAccelerationZ[i];

            // Dot product and sign
            float dot = dirX * accX + dirZ * accZ;
            int sign = Math.Sign(dot);

            // Acceleration magnitude
            float accMagnitude = (float)Math.Sqrt(accX * accX + accZ * accZ);

            // Signed acceleration value
            float signedAcc = sign * accMagnitude;
            accelerationCoords[idx, 0] = tempCurveCoord[i, 0] - normals[i, 0] * signedAcc* iHMIFactorAcceleration;
            accelerationCoords[idx, 1] = tempCurveCoord[i, 1] - normals[i, 1] * signedAcc* iHMIFactorAcceleration;
        }

        // Optionally: Save to model variables
        var velocityVar = Project.Current.GetVariable("Model/CourseCurve/arrVelocity");
        var accelerationVar = Project.Current.GetVariable("Model/CourseCurve/arrAcceleration");
        if (velocityVar != null)
            velocityVar.SetValue(velocityCoords);
        if (accelerationVar != null)
            accelerationVar.SetValue(accelerationCoords);
    }

    public void DrawCamData()
    {
        var PLC = LogicObject.GetVariable("PLC").Value.Value.ToString(); //Either CLX or VP1 or VP2
        
        //Calculate the Visual representation of Cam timing
        var CamDataTag = Project.Current.GetVariable($"CommDrivers/RAEtherNet_IPDriver1/{PLC}/Tags/Controller Tags/PLS_CamData");
        var CamData = CamDataTag.RemoteRead();
        int NumberOfCams = Convert.ToInt32(((UAManagedCore.Struct)CamData.Value).Values[1]);
        var CamList = ((UAManagedCore.Struct)CamData.Value).Values[0];

        // VisualCam
        int VisualCam = 0;
        // all data before and after the cam is found are 0/0 which destroys the curve
        // Therefore, we need to set those to the same value as the first/last point
        //bool Camstarted = false;
        for (int camIdx = 0; camIdx < NumberOfCams; camIdx++)
        {
            // Skip cams not used and Cam 0
            if (camIdx == 0 || (camIdx >= 19 && camIdx <= 24))
                continue;

            var camVisible = Project.Current.GetVariable($"Model/CourseCurve/Cams/CamVisible{camIdx}");
            camVisible.Value = Convert.ToBoolean((((UAManagedCore.Struct[])CamList)[camIdx].Values)[3]); //Enable

            if (camVisible.Value) // Enable
            {
                // CamOn != CamOff
                if (Convert.ToInt32((((UAManagedCore.Struct[])CamList)[camIdx].Values)[4]) != Convert.ToInt32((((UAManagedCore.Struct[])CamList)[camIdx].Values)[5]))
                {
                    int MinPos = Convert.ToInt32((((UAManagedCore.Struct[])CamList)[camIdx].Values)[4]); //CamOn
                    int MaxPos = Convert.ToInt32((((UAManagedCore.Struct[])CamList)[camIdx].Values)[5]); //CamOff

                    // Prepare array for cam coordinates
                    float[,] camCoords = new float[361, 2];

                    // Multiply normal by visual cam index
                    VisualCam++;

                    // Calculate first and last valid cam coordinates
                    float firstX = tempCurveCoord[MinPos, 0] + normals[MinPos, 0] * VisualCam * VisualCamModifier;
                    float firstZ = tempCurveCoord[MinPos, 1] + normals[MinPos, 1] * VisualCam * VisualCamModifier;
                    float lastX = tempCurveCoord[MaxPos, 0] + normals[MaxPos, 0] * VisualCam * VisualCamModifier;
                    float lastZ = tempCurveCoord[MaxPos, 1] + normals[MaxPos, 1] * VisualCam * VisualCamModifier;

                    if (MinPos <= MaxPos)
                    {
                        // Normal case: region is a single interval
                        for (int i = 0; i < 361; i++)
                        {
                            if (i < MinPos)
                            {
                                camCoords[i, 0] = firstX;
                                camCoords[i, 1] = firstZ;
                            }
                            else if (i > MaxPos)
                            {
                                camCoords[i, 0] = lastX;
                                camCoords[i, 1] = lastZ;
                            }
                            else
                            {
                                float baseX = tempCurveCoord[i, 0];
                                float baseZ = tempCurveCoord[i, 1];
                                float nx = normals[i, 0];
                                float nz = normals[i, 1];
                                camCoords[i, 0] = baseX + nx * VisualCam * VisualCamModifier;
                                camCoords[i, 1] = baseZ + nz * VisualCam * VisualCamModifier;
                            }
                        }
                    }
                    else
                    {
                        // Wrapped case: region is two intervals, combine as one continuous region starting at 0
                        int intervalSize = (360 - MinPos + 1) + (MaxPos + 1);
                        int outIdx = 0;

                        // Fill from MinPos to 360
                        for (int i = MinPos; i < 361; i++, outIdx++)
                        {
                            float baseX = tempCurveCoord[i, 0];
                            float baseZ = tempCurveCoord[i, 1];
                            float nx = normals[i, 0];
                            float nz = normals[i, 1];
                            camCoords[outIdx, 0] = baseX + nx * VisualCam * VisualCamModifier;
                            camCoords[outIdx, 1] = baseZ + nz * VisualCam * VisualCamModifier;
                        }
                        // Fill from 0 to MaxPos
                        for (int i = 0; i <= MaxPos; i++, outIdx++)
                        {
                            float baseX = tempCurveCoord[i, 0];
                            float baseZ = tempCurveCoord[i, 1];
                            float nx = normals[i, 0];
                            float nz = normals[i, 1];
                            camCoords[outIdx, 0] = baseX + nx * VisualCam * VisualCamModifier;
                            camCoords[outIdx, 1] = baseZ + nz * VisualCam * VisualCamModifier;
                        }
                        // Fill the rest (from intervalSize to 360) with lastX/lastZ
                        for (int i = intervalSize; i < 361; i++)
                        {
                            camCoords[i, 0] = lastX;
                            camCoords[i, 1] = lastZ;
                        }
                    }

                    // Save to model variable: Model/CourseCurve/Cams/Cam[camIdx]
                    var camVar = Project.Current.GetVariable($"Model/CourseCurve/Cams/Cam{camIdx}");


                    if (camVar != null)
                    {
                        camVar.SetValue(camCoords);
                        camVisible.Value = true;
                    }
                    else
                    {
                        camVisible.Value = false;
                    }
                }
            }
            else
            {
                camVisible.Value = false;
            }
        }
    }


    private LongRunningTask DrawCamDataAsync;
}
