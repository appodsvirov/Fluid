using Fluid_Full_;

GlobalCanvas globalCanvas = new GlobalCanvas();

LogFluid.AddPoint(99, 38);
LogFluid.AddPoint(99, 62);


globalCanvas.SetupScene();
globalCanvas.Simulate();



LogFluid.GetFullSpeed();


//string path = "C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\outputFluid.txt";

//using (StreamWriter streamWriter = new StreamWriter(path))
//{
//    streamWriter.Write(globalCanvas.scene.fluid.ToString());
//}