using Fluid_Full_;

GlobalCanvas globalCanvas = new GlobalCanvas();

LogFluid.AddPoint(99, 38);
LogFluid.AddPoint(99, 62);


globalCanvas.SetupScene(1);

for (int i = 0; i < 3000; i++)
{
    if(i%10 == 0) Console.WriteLine(i);
    globalCanvas.Simulate();
}



LogFluid.GetFullSpeed();
LogFluid.GetMass();
LogFluid.GetP();
LogFluid.Dispose();


//string path = "C:\\Users\\USER\\Desktop\\fluid\\Fluid(Full)\\output\\outputFluid.txt";

//using (StreamWriter streamWriter = new StreamWriter(path))
//{
//    streamWriter.Write(globalCanvas.scene.fluid.ToString());
//}