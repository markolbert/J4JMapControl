namespace MapLibTests;

public class ClearImageFiles : TestBase
{
    public ClearImageFiles()
    {
        foreach (var projName in ProjectionNames)
        {
            foreach (var file in Directory.GetFiles(GetCheckImagesFolder(projName)))
            {
                File.Delete(file);
            }
        }
    }
}
