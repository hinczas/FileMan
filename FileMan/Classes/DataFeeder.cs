using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileMan.Classes
{
    public static class DataFeeder
    {
        // Privates
        private static string DefaultIcon
        {
            get
            {
                return "_blank.png";
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
                    {"aac","aac.png"},
                    {"ai","ai.png"},
                    {"aiff","aiff.png"},
                    {"avi","avi.png"},
                    {"bmp","bmp.png"},
                    {"c","c.png"},
                    {"cpp","cpp.png"},
                    {"css","css.png"},
                    {"csv","csv.png"},
                    {"dat","dat.png"},
                    {"dmg","dmg.png"},
                    {"doc","doc.png"},
                    {"docx","doc.png"},
                    {"dotx","dotx.png"},
                    {"dwg","dwg.png"},
                    {"dxf","dxf.png"},
                    {"eps","eps.png"},
                    {"exe","exe.png"},
                    {"flv","flv.png"},
                    {"gif","gif.png"},
                    {"h","h.png"},
                    {"hpp","hpp.png"},
                    {"html","html.png"},
                    {"ics","ics.png"},
                    {"iso","iso.png"},
                    {"java","java.png"},
                    {"jpg","jpg.png"},
                    {"js","js.png"},
                    {"json","json.png"},
                    {"key","key.png"},
                    {"less","less.png"},
                    {"mid","mid.png"},
                    {"mp3","mp3.png"},
                    {"mp4","mp4.png"},
                    {"mpg","mpg.png"},
                    {"odf","odf.png"},
                    {"ods","ods.png"},
                    {"odt","odt.png"},
                    {"otp","otp.png"},
                    {"ots","ots.png"},
                    {"ott","ott.png"},
                    {"pdf","pdf.png"},
                    {"php","php.png"},
                    {"png","png.png"},
                    {"ppt","ppt.png"},
                    {"psd","psd.png"},
                    {"py","py.png"},
                    {"qt","qt.png"},
                    {"rar","rar.png"},
                    {"rb","rb.png"},
                    {"rtf","rtf.png"},
                    {"sass","sass.png"},
                    {"scss","scss.png"},
                    {"sql","sql.png"},
                    {"tga","tga.png"},
                    {"tgz","tgz.png"},
                    {"tiff","tiff.png"},
                    {"txt","txt.png"},
                    {"wav","wav.png"},
                    {"xls","xls.png"},
                    {"xlsx","xlsx.png"},
                    {"xml","xml.png"},
                    {"yml","yml.png"},
                    {"zip","zip.png"}
                };
            }
        }

        // Publics
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