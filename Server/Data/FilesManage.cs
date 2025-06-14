namespace AuthWithAdmin.Server.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

//לא לגעת - ניהול טוקנים

public class FilesManage
{
    private readonly IWebHostEnvironment _env;

    public FilesManage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool DeleteFile(string fileName, string containerName)
    {
        string folderPath = Path.Combine(_env.WebRootPath, containerName);
        string savingPath = Path.Combine(folderPath, fileName);

        if (File.Exists(savingPath))
        {
            File.Delete(savingPath);
            return true;
        }

        return false; 
    }


    public async Task<string> SavePdfFile(string base64Data, string containerName)
    {
        byte[] pdfData = Convert.FromBase64String(base64Data);

        var fileName = $"{Guid.NewGuid()}.pdf";
        string folderPath = Path.Combine(_env.WebRootPath, containerName);

        // Check if the folder exists and create it if it doesn't
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string savingPath = Path.Combine(folderPath, fileName);

        // Save the PDF file directly
        await File.WriteAllBytesAsync(savingPath, pdfData);

        return fileName;
    }


    public async Task<string> SaveFile(string imageBase64, string extension, string containerName)
    {  
        byte[] picture = Convert.FromBase64String(imageBase64);
        using (Image image = Image.Load(picture))
        {

            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(600, 600)
                }));

            var fileName = $"{Guid.NewGuid()}.{extension}";
            string folderPath = Path.Combine(_env.WebRootPath, containerName);

            // Check if the folder exists and create it if it doesn't
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string savingPath = Path.Combine(folderPath, fileName);

            await image.SaveAsync(savingPath); // Automatic encoder selected based on extension.

            return fileName;
        }
    }
    
}