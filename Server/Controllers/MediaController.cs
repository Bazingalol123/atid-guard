using AuthWithAdmin.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWithAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly FilesManage _filesManage;

        public MediaController(FilesManage filesManage)
        {
            _filesManage = filesManage;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromBody] string imageBase64)
        {
            Console.WriteLine("UploadFile called");
            string fileName = await _filesManage.SaveFile(imageBase64, "png", "uploadedFiles");
            Console.WriteLine($"File uploaded: {fileName}");
            return Ok(fileName);
        }

        [HttpPost("uploadTemp")]
        public async Task<IActionResult> UploadFileTemp([FromBody] string imageBase64)
        {
            Console.WriteLine("UploadFileTemp called");
            string fileName = await _filesManage.SaveFile(imageBase64, "png", "uploadTemp");
            Console.WriteLine($"Temp file uploaded: {fileName}");
            return Ok(fileName);
        }

        [HttpPost("deleteImages")]
        public async Task<IActionResult> DeleteImages([FromBody] List<string> images)
        {
            Console.WriteLine("DeleteImages called");
            var countFalseTry = 0;
            foreach (string img in images)
            {
                Console.WriteLine($"Attempting to delete image: {img}");
                if (_filesManage.DeleteFile(img, ""))
                {
                    Console.WriteLine($"Successfully deleted image: {img}");
                }
                else
                {
                    countFalseTry++;
                    Console.WriteLine($"Failed to delete image: {img}");
                }
            }

            if (countFalseTry > 0)
            {
                Console.WriteLine($"Problem with {countFalseTry} images");
                return BadRequest("problem with " + countFalseTry.ToString() + " images");
            }
            return Ok("deleted");
        }

        [HttpPost("deletePdfs")]
        public async Task<IActionResult> DeletePdfs([FromBody] List<string> pdfs)
        {
            Console.WriteLine("DeletePdfs called");
            var countFalseTry = 0;
            foreach (string pdf in pdfs)
            {
                Console.WriteLine($"Attempting to delete PDF: {pdf}");
                if (_filesManage.DeleteFile(pdf, "uploadedPdfs"))
                {
                    Console.WriteLine($"Successfully deleted PDF: {pdf}");
                }
                else
                {
                    countFalseTry++;
                    Console.WriteLine($"Failed to delete PDF: {pdf}");
                }
            }

            if (countFalseTry > 0)
            {
                Console.WriteLine($"Problem with {countFalseTry} PDFs");
                return BadRequest("problem with " + countFalseTry.ToString() + " PDFs");
            }
            return Ok("deleted");
        }

        [HttpPost("moveFiles")]
        public async Task<IActionResult> MoveFiles([FromBody] List<string> fileNames)
        {
            Console.WriteLine("MoveFiles called");
            var countFalseTry = 0;
            foreach (string fileName in fileNames)
            {
                string sourcePath = Path.Combine("wwwroot/uploadTemp", fileName);
                string destinationPath = Path.Combine("wwwroot/uploadedFiles", fileName);

                Console.WriteLine($"Attempting to move file: {fileName}");
                Console.WriteLine($"Source path: {sourcePath}");
                Console.WriteLine($"Destination path: {destinationPath}");

                // Check if the file already exists in the destination
                if (System.IO.File.Exists(destinationPath))
                {
                    // Skip the file as it already exists in the destination
                    Console.WriteLine($"File already exists in destination: {destinationPath}");
                    continue;
                }

                // Check if the file exists in the source
                if (!System.IO.File.Exists(sourcePath))
                {
                    countFalseTry++;
                    Console.WriteLine($"File does not exist in source: {sourcePath}");
                    continue;
                }

                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        Console.WriteLine($"Created directory: {Path.GetDirectoryName(destinationPath)}");
                    }

                    System.IO.File.Move(sourcePath, destinationPath);
                    Console.WriteLine($"Successfully moved file to: {destinationPath}");
                }
                catch (Exception ex)
                {
                    countFalseTry++;
                    Console.WriteLine($"Error moving file: {fileName}, Exception: {ex.Message}");
                }
            }

            if (countFalseTry > 0)
            {
                Console.WriteLine($"Problem with moving {countFalseTry} files");
                return BadRequest("Problem with moving " + countFalseTry.ToString() + " files");
            }
            return Ok("Files moved successfully");
        }

        [HttpPost("uploadPdfTemp")]
        public async Task<IActionResult> UploadPdfTemp([FromBody] string pdfBase64)
        {
            Console.WriteLine("UploadPdfTemp called");

            try
            {
                // Strip the base64 prefix if present
                if (pdfBase64.StartsWith("data:application/pdf;base64,"))
                {
                    pdfBase64 = pdfBase64.Substring("data:application/pdf;base64,".Length);
                }

                byte[] pdfData = Convert.FromBase64String(pdfBase64);

                var fileName = $"{Guid.NewGuid()}.pdf";
                string folderPath = Path.Combine("wwwroot", "pdfTemp");

                // Check if the folder exists and create it if it doesn't
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string savingPath = Path.Combine(folderPath, fileName);

                // Save the PDF file directly
                await System.IO.File.WriteAllBytesAsync(savingPath, pdfData);

                Console.WriteLine($"Temp PDF uploaded: {fileName}");
                return Ok(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading temp PDF: {ex.Message}");
                return BadRequest($"Error uploading temp PDF: {ex.Message}");
            }
        }

        [HttpPost("movePdfs")]
        public async Task<IActionResult> MovePdfs([FromBody] List<string> fileNames)
        {
            Console.WriteLine("MovePdfs called");
            var countFalseTry = 0;
            foreach (string fileName in fileNames)
            {
                string sourcePath = Path.Combine("wwwroot/pdfTemp", fileName);
                string destinationPath = Path.Combine("wwwroot/uploadedFiles", fileName);

                Console.WriteLine($"Attempting to move PDF: {fileName}");
                Console.WriteLine($"Source path: {sourcePath}");
                Console.WriteLine($"Destination path: {destinationPath}");

                // Check if the file already exists in the destination
                if (System.IO.File.Exists(destinationPath))
                {
                    // Skip the file as it already exists in the destination
                    Console.WriteLine($"PDF already exists in destination: {destinationPath}");
                    continue;
                }

                // Check if the file exists in the source
                if (!System.IO.File.Exists(sourcePath))
                {
                    countFalseTry++;
                    Console.WriteLine($"PDF does not exist in source: {sourcePath}");
                    continue;
                }

                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        Console.WriteLine($"Created directory: {Path.GetDirectoryName(destinationPath)}");
                    }

                    System.IO.File.Move(sourcePath, destinationPath);
                    Console.WriteLine($"Successfully moved PDF to: {destinationPath}");
                }
                catch (Exception ex)
                {
                    countFalseTry++;
                    Console.WriteLine($"Error moving PDF: {fileName}, Exception: {ex.Message}");
                }
            }

            if (countFalseTry > 0)
            {
                Console.WriteLine($"Problem with moving {countFalseTry} PDFs");
                return BadRequest("Problem with moving " + countFalseTry.ToString() + " PDFs");
            }
            return Ok("PDFs moved successfully");
        }

        [HttpPost("uploadPdf")]
        public async Task<IActionResult> UploadPdf([FromBody] string pdfBase64)
        {
            Console.WriteLine("UploadPdf called");

            // Strip the base64 prefix if present
            if (pdfBase64.StartsWith("data:application/pdf;base64,"))
            {
                pdfBase64 = pdfBase64.Substring("data:application/pdf;base64,".Length);
            }

            try
            {
                byte[] pdfData = Convert.FromBase64String(pdfBase64);

                var fileName = $"{Guid.NewGuid()}.pdf";
                string folderPath = Path.Combine("wwwroot", "uploadedFiles");

                // Check if the folder exists and create it if it doesn't
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string savingPath = Path.Combine(folderPath, fileName);

                // Save the PDF file directly
                await System.IO.File.WriteAllBytesAsync(savingPath, pdfData);

                Console.WriteLine($"PDF uploaded: {fileName}");
                return Ok(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading PDF: {ex.Message}");
                return BadRequest($"Error uploading PDF: {ex.Message}");
            }
        }
    }
}
