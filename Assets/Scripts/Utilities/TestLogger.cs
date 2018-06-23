using System;
using System.IO;

public static class TestLogger
{
    static private string fileName = "testLog.txt";
    public static void CreateFile()
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        File.CreateText(fileName);
        
    }

    public static void AddLine(string line)
    {
        File.AppendAllText(fileName, line + "\n");
    }
}
