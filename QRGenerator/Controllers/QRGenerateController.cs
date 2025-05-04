using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QRGenerator.Models;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace QRGenerator.Controllers
{
    public class QRGenerateController : Controller
    {

        private void DrawRoundedEye(Graphics g, int offsetX, int offsetY, int moduleSize)
        {
            int eyeSize = moduleSize * 7;
            Rectangle outerRect = new Rectangle(offsetX, offsetY, eyeSize, eyeSize);
            Rectangle innerRect = new Rectangle(offsetX + 2 * moduleSize, offsetY + 2 * moduleSize, moduleSize * 3, moduleSize * 3);

            using (Pen outerPen = new Pen(Color.Black, 1))
            using (SolidBrush eyeBrush = new SolidBrush(Color.Black))
            {
                g.DrawRoundedRectangle(outerPen, outerRect, moduleSize * 1.5f);
                g.FillRoundedRectangle(eyeBrush, innerRect, moduleSize);
            }
        }

        private void GenerateQRCode(QRCodeModel model)
        {
            PayloadGenerator.Payload payload = null;

            switch (model.QRCodeType)
            {
                case 1:
                    payload = new PayloadGenerator.Url(model.WebsiteURL);
                    break;
                case 2:
                    payload = new PayloadGenerator.Bookmark(model.BookmarkURL, model.BookmarkURL);
                    break;
                case 3:
                    payload = new PayloadGenerator.SMS(model.SMSPhoneNumber, model.SMSBody);
                    break;
                case 4:
                    payload = new PayloadGenerator.WhatsAppMessage(model.WhatsAppNumber, model.WhatsAppMessage);
                    break;
                case 5:
                    payload = new PayloadGenerator.Mail(model.ReceiverEmailAddress, model.EmailSubject, model.EmailMessage);
                    break;
                case 6:
                    payload = new PayloadGenerator.WiFi(model.WIFIName, model.WIFIPassword, PayloadGenerator.WiFi.Authentication.WPA);
                    break;
                default:
                    return;
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);

            List<BitArray> moduleMatrix = qrCodeData.ModuleMatrix;
            int pixelsPerModule = 12;
            int matrixSize = moduleMatrix.Count;
            int quietZoneModules = 4;
            int totalModules = matrixSize + quietZoneModules * 2;
            int size = totalModules * pixelsPerModule;

            using (Bitmap bitmap = new Bitmap(size, size))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (SolidBrush darkBrush = new SolidBrush(Color.Black))
            {
                graphics.Clear(Color.White);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle logoRect = Rectangle.Empty;

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    using (System.Drawing.Image logo = System.Drawing.Image.FromFile(logoPath))
                    {
                        int logoSize = size / 5;
                        int logoX = (size - logoSize) / 2;
                        int logoY = (size - logoSize) / 2;
                        int padding = 10;

                        logoRect = new Rectangle(logoX - padding, logoY - padding, logoSize + 2 * padding, logoSize + 2 * padding);
                        graphics.FillRectangle(Brushes.White, logoRect);
                        graphics.DrawImage(logo, new Rectangle(logoX, logoY, logoSize, logoSize));
                    }
                }

                for (int y = 0; y < matrixSize; y++)
                {
                    for (int x = 0; x < matrixSize; x++)
                    {
                        if (moduleMatrix[y][x])
                        {
                            float cx = (x + quietZoneModules) * pixelsPerModule + pixelsPerModule / 2f;
                            float cy = (y + quietZoneModules) * pixelsPerModule + pixelsPerModule / 2f;
                            float radius = pixelsPerModule * 0.3f;

                            if (logoRect.Contains((int)cx, (int)cy) || IsInEyeRegion(x, y, matrixSize))
                                continue;

                            graphics.FillEllipse(darkBrush, cx - radius, cy - radius, radius * 2, radius * 2);
                        }
                    }
                }

                DrawRoundedEye(graphics, quietZoneModules * pixelsPerModule, quietZoneModules * pixelsPerModule, pixelsPerModule); // Top-left
                DrawRoundedEye(graphics, (quietZoneModules + matrixSize - 7) * pixelsPerModule, quietZoneModules * pixelsPerModule, pixelsPerModule); // Top-right
                DrawRoundedEye(graphics, quietZoneModules * pixelsPerModule, (quietZoneModules + matrixSize - 7) * pixelsPerModule, pixelsPerModule); // Bottom-left

                string qrCodeFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "QRCodes", "QRCodeGenerated");
                if (!Directory.Exists(qrCodeFolder))
                    Directory.CreateDirectory(qrCodeFolder);

                string pngFileName = $"QR_{Guid.NewGuid()}.png";
                string pngFilePath = Path.Combine(qrCodeFolder, pngFileName);

                bitmap.Save(pngFilePath, ImageFormat.Png);
                model.QRImageFileName = pngFileName;
                model.QRImageURL = "/QRCodes/QRCodeGenerated/" + pngFileName;
            }
        }

        private bool IsInEyeRegion(int x, int y, int size)
        {
            return (x < 7 && y < 7) || (x >= size - 7 && y < 7) || (x < 7 && y >= size - 7);
        }

        [HttpPost]
        public IActionResult Generate(QRCodeModel model)
        {
            if (ModelState.IsValid)
            {
                GenerateQRCode(model);
            }
            return View("/Views/QRIndex.cshtml", model);
        }
        public IActionResult Index()
        {
            QRCodeModel model = new QRCodeModel();
            return View("/Views/QRIndex.cshtml", model);
        }


    }
}

public static class GraphicsExtensions
{

    private static GraphicsPath RoundedRect(Rectangle bounds, float radius)
    {
        float diameter = radius * 2;
        GraphicsPath path = new GraphicsPath();

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);

        path.CloseFigure();
        return path;
    }

    public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, float radius)
    {
        using (GraphicsPath path = RoundedRect(rect, radius))
        {
            g.DrawPath(pen, path);
        }
    }

    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, float radius)
    {
        using (GraphicsPath path = RoundedRect(rect, radius))
        {
            g.FillPath(brush, path);
        }
    }

}
