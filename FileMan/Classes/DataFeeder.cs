using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Classes
{
    public static class DataFeeder
    {
        private static string DefaultIcon
        {
            get
            {
                return "~/Content/Images/Icons/file.png";
            }
        }

        private static string[] DocuViewerExtensions {
            get
            {
                return new string[]
                {
                    "DOCX", "BMP", "IFF", "JPEG", "PIC", "RLE", "XPM", "DC2", "K25", "PTX",
                    "TXT", "CUT", "ICO", "JPG", "PCX", "SGI", "3FR", "DCR", "KDC", "PXN",
                    "RTF", "DDS", "J2K", "JPE", "PFM", "TGA", "ARW", "DRF", "MDC", "QTK",
                    "SVG", "DIB", "J2C", "KOA", "PGM", "TARGA", "BAY", "DSC", "MEF", "RAF",
                    "PDF", "DICOM", "JB2", "LBM", "PSD", "TIFF", "BMQ", "DNG", "MOS", "RDC",
                    "EMF", "EXIF", "JBIG2", "MNG", "PNG", "TIF", "CAP", "ERF", "MRW", "RW2",
                    "WMF", "EXR", "JIF", "PBM", "PNM", "WBMP", "CINE", "FFF", "NEF", "RW1",
                    "CUR", "FAX", "JFIF", "PCD", "PPM", "WAP", "CR2", "IA", "NRW", "RWZ",
                    "WSQ", "G3", "JNG", "PCT", "RAS", "WBM", "CRW", "IIQ", "ORF", "SR2",
                     "HDR", "JP2", "PICT", "RAW", "XBM", "CS1", "KC2", "PEF", "SRF",
                    "SRW", "STI"
                };
            }
        }

        private static Dictionary<string, string> IconMappings {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"aac","~/Content/Images/Icons/32px/aac.png"},
                    {"ai","~/Content/Images/Icons/32px/ai.png"},
                    {"aiff","~/Content/Images/Icons/32px/aiff.png"},
                    {"avi","~/Content/Images/Icons/32px/avi.png"},
                    {"bmp","~/Content/Images/Icons/32px/bmp.png"},
                    {"c","~/Content/Images/Icons/32px/c.png"},
                    {"cpp","~/Content/Images/Icons/32px/cpp.png"},
                    {"css","~/Content/Images/Icons/32px/css.png"},
                    {"csv","~/Content/Images/Icons/32px/csv.png"},
                    {"dat","~/Content/Images/Icons/32px/dat.png"},
                    {"dmg","~/Content/Images/Icons/32px/dmg.png"},
                    {"doc","~/Content/Images/Icons/32px/doc.png"},
                    {"dotx","~/Content/Images/Icons/32px/dotx.png"},
                    {"dwg","~/Content/Images/Icons/32px/dwg.png"},
                    {"dxf","~/Content/Images/Icons/32px/dxf.png"},
                    {"eps","~/Content/Images/Icons/32px/eps.png"},
                    {"exe","~/Content/Images/Icons/32px/exe.png"},
                    {"flv","~/Content/Images/Icons/32px/flv.png"},
                    {"gif","~/Content/Images/Icons/32px/gif.png"},
                    {"h","~/Content/Images/Icons/32px/h.png"},
                    {"hpp","~/Content/Images/Icons/32px/hpp.png"},
                    {"html","~/Content/Images/Icons/32px/html.png"},
                    {"ics","~/Content/Images/Icons/32px/ics.png"},
                    {"iso","~/Content/Images/Icons/32px/iso.png"},
                    {"java","~/Content/Images/Icons/32px/java.png"},
                    {"jpg","~/Content/Images/Icons/32px/jpg.png"},
                    {"js","~/Content/Images/Icons/32px/js.png"},
                    {"key","~/Content/Images/Icons/32px/key.png"},
                    {"less","~/Content/Images/Icons/32px/less.png"},
                    {"mid","~/Content/Images/Icons/32px/mid.png"},
                    {"mp3","~/Content/Images/Icons/32px/mp3.png"},
                    {"mp4","~/Content/Images/Icons/32px/mp4.png"},
                    {"mpg","~/Content/Images/Icons/32px/mpg.png"},
                    {"odf","~/Content/Images/Icons/32px/odf.png"},
                    {"ods","~/Content/Images/Icons/32px/ods.png"},
                    {"odt","~/Content/Images/Icons/32px/odt.png"},
                    {"otp","~/Content/Images/Icons/32px/otp.png"},
                    {"ots","~/Content/Images/Icons/32px/ots.png"},
                    {"ott","~/Content/Images/Icons/32px/ott.png"},
                    {"pdf","~/Content/Images/Icons/32px/pdf.png"},
                    {"php","~/Content/Images/Icons/32px/php.png"},
                    {"png","~/Content/Images/Icons/32px/png.png"},
                    {"ppt","~/Content/Images/Icons/32px/ppt.png"},
                    {"psd","~/Content/Images/Icons/32px/psd.png"},
                    {"py","~/Content/Images/Icons/32px/py.png"},
                    {"qt","~/Content/Images/Icons/32px/qt.png"},
                    {"rar","~/Content/Images/Icons/32px/rar.png"},
                    {"rb","~/Content/Images/Icons/32px/rb.png"},
                    {"rtf","~/Content/Images/Icons/32px/rtf.png"},
                    {"sass","~/Content/Images/Icons/32px/sass.png"},
                    {"scss","~/Content/Images/Icons/32px/scss.png"},
                    {"sql","~/Content/Images/Icons/32px/sql.png"},
                    {"tga","~/Content/Images/Icons/32px/tga.png"},
                    {"tgz","~/Content/Images/Icons/32px/tgz.png"},
                    {"tiff","~/Content/Images/Icons/32px/tiff.png"},
                    {"txt","~/Content/Images/Icons/32px/txt.png"},
                    {"wav","~/Content/Images/Icons/32px/wav.png"},
                    {"xls","~/Content/Images/Icons/32px/xls.png"},
                    {"xlsx","~/Content/Images/Icons/32px/xlsx.png"},
                    {"xml","~/Content/Images/Icons/32px/xml.png"},
                    {"yml","~/Content/Images/Icons/32px/yml.png"},
                    {"zip","~/Content/Images/Icons/32px/zip.png"}
                };
            }
        }

        public static bool DocuCompatible(string extension)
        {
            if (extension.StartsWith("."))
            {
                extension = extension.Replace(".", "");
            }
            extension = extension.ToUpper();

            bool result = DocuViewerExtensions.Contains(extension);
            return result;
        }

        public static string GetIcon(string extension)
        {
            if (extension.StartsWith("."))
            {
                extension = extension.Replace(".", "");
            }

            extension = extension.ToLower();

            string path="";

            if (IconMappings.TryGetValue(extension, out path)) {
                return path;
            } else
            {
                return DefaultIcon;
            }
            
        }
    }
}