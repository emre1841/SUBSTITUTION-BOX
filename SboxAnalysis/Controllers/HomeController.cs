using ExcelDataReader;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using SboxAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SboxAnalysis.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Sbox()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public JsonResult ImportSboxValues(string sboxStingValue)
        {
            var str = ImportSbox(sboxStingValue);
            return Json(str, JsonRequestBehavior.AllowGet);
        }

        private int BicNlCalculate(int[] sboxArr)
        {
            for (int i = 0; i < 16; i++)
            {

            }
            return 1;
        }
        private string ImportSbox(string sboxVal)
        {
            int[] sboxArr = new int[16];

            string[] sboxValues = new string[16];
            sboxValues = sboxVal.Split(',');
            var loop = 0;
            for (int i = 0; i < sboxValues.Length; i++)
            {
                sboxArr[loop] = Convert.ToByte(sboxValues[i]);
                loop++;
            }

            var str = GetHexValues(sboxArr);
            return str;
        }
        private string GetHexValues(int[] sboxArr)
        {
            byte[] sboxByteArr = new byte[16];
            var count = 0;
            for (int i = 0; i < sboxArr.Length; i++)
            {
                sboxByteArr[count] = Convert.ToByte(sboxArr[i]);

                count++;
            }
            string str = BitConverter.ToString(sboxByteArr);
            return str;
        }
        public JsonResult RandomSboxValues()
        {
            Random rnd = new Random();

            int[] sboxArr = new int[16];

            int uretilen = 15;
            while (uretilen > 0)
            {
                var rastgele = rnd.Next(0, 16);

                if (!sboxArr.Contains(rastgele))
                {
                    sboxArr[uretilen] = rastgele;
                    uretilen--;
                }
            }

            var rastgeleArr = rnd.Next(0, 16);
            var gecici = sboxArr[rastgeleArr];
            sboxArr[rastgeleArr] = sboxArr[0];
            sboxArr[0] = gecici;

            var str = GetHexValues(sboxArr);

            return Json(str, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ExportSboxValues(string sbox)
        {
            var sboxStrArr = new string[16];
            var sboxVal = "";

            sboxStrArr = sbox.Split('-');
            for (int i = 0; i < sboxStrArr.Length; i++)
            {
                sboxVal += int.Parse(sboxStrArr[i], System.Globalization.NumberStyles.HexNumber).ToString() + ',';
            }


            return Json(sboxVal, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Analysis(string sbox)
        {
            var sboxStrArr = new string[16];
            var sboxArr = new byte[16];

            sboxStrArr = sbox.Split('-');
            for (int i = 0; i < sboxStrArr.Length; i++)
            {
                sboxArr[i] = Convert.ToByte(int.Parse(sboxStrArr[i], System.Globalization.NumberStyles.HexNumber));
            }
            var result = CalculateLineerProbability(sboxArr);
            var bicNl = ComparisonMatris(sboxStrArr);

            result.BicNl = bicNl;

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private LinearResultModel CalculateLineerProbability(byte[] sboxArr)
        {

            var linearResult = new LinearResultModel();
            var maxLinearVal = 0;
            var maxDifVal = 0;
            var lineerBiasArr = new int[16];
            var difBiasArr = new int[16];
            var lineerBiasArrTemp = new int[8];


            ///------------------------------------
            ///Lineer Hesaplamalar
            for (int j = 15; j > 0; j--)
            {
                for (int k = 15; k > 0; k--)
                {
                    var linearBias = 8;
                    var diffBias = 0;
                    for (int i = 15; i > 0; i--)
                    {
                        var byteNum = Convert.ToByte(i & j ^ sboxArr[i] & k);
                        linearBias -= Parity(byteNum);
                        diffBias += Convert.ToByte((sboxArr[i] ^ sboxArr[i ^ j]) == k);
                    }
                    if (Math.Abs(linearBias) > Math.Abs(maxLinearVal))
                    {
                        maxLinearVal = Math.Abs(linearBias);
                        //lineer işlemlerdeki değerlerin tümünü lineerBiasArr dizisine atar. 
                        //Dizide bulunan en küçük değer MaxLineer değeri olmakta. 
                        //En büyük değeri minLineer değeri olarak kullanılmakta(Doğruluğu şüpheli) 
                        lineerBiasArr[k] = Math.Abs(linearBias);
                    }
                    //Max dif hesaplama
                    if (diffBias > maxDifVal)
                    {
                        maxDifVal = diffBias;
                        difBiasArr[k] = diffBias;
                    }
                }
            }

            lineerBiasArrTemp = lineerBiasArr.Where(x => x >= 3 && x <= 10).OrderByDescending(x => x).Take(8).ToArray();

            var difMin = difBiasArr.ToList().Where(x => x > 0).Min();
            var difMax = difBiasArr.ToList().Where(x => x > 0).Max();

            var linMin = lineerBiasArr.ToList().Where(x => x != 0).Min();
            var linMax = lineerBiasArr.ToList().Where(x => x != 0).Max();

            var linMinTemp = lineerBiasArrTemp.ToList().Where(x => x != 0).Min();
            var linMaxTemp = lineerBiasArrTemp.ToList().Where(x => x != 0).Max();


            var avgLin = lineerBiasArr.ToList().Where(x => x != 0).Average();
            var avgDif = difBiasArr.ToList().Where(x => x != 0).Average();

            linearResult.AvgLinearProbability = Math.Abs(Math.Round((avgLin / 0.16 / 100), 4));
            linearResult.MaxLinearProbability = Math.Abs(Math.Round((linMax / 0.16 / 100), 4));
            linearResult.MinLinearProbability = Math.Abs(Math.Round((linMin / 0.16 / 100), 4));

            linearResult.MaxDifferentialProbability = Math.Abs(Math.Round((difMax / 0.16/ 100), 4));
            linearResult.MinDifferentialProbability = Math.Abs(Math.Round((difMin / 0.16 / 100), 4));
            linearResult.AvgDifferentialProbability = Math.Abs(Math.Round((avgDif / 0.16 / 100), 4));

            var minNonLin = NonLinearity(linMax, 4);
            var maxNonLin = NonLinearity(linMin, 4);
            var avgNonLin = NonLinearity(Convert.ToInt32(avgLin), 4);


            var minNonLinTemp = NonLinearity(linMaxTemp / 2, 4);
            var maxNonLinTemp = NonLinearity(linMinTemp / 2, 4);
            var avgNonLinTemp = NonLinearity(Convert.ToInt32(((linMaxTemp + linMinTemp) / 2) / 2), 4);

            double nonLinMaxVal = 8 - linMaxTemp;
            double nonLinMinVal = 8 - linMinTemp;
            double nonLinAvgVal = 8 - ((linMaxTemp + linMinTemp) / 2);

            double nonLinPerMax = Convert.ToDouble(nonLinMinVal / 6);
            double nonLinPerMin = Convert.ToDouble(nonLinMaxVal / 6);
            double nonLinPerAvg = nonLinAvgVal / 6;

            linearResult.NonLinearProbabilityMax = nonLinMinVal;
            linearResult.NonLinearProbabilityMin = nonLinMaxVal;
            linearResult.NonLinearProbabilityAvg = nonLinAvgVal;


            //avg ondalıklı geldiği için hata vermekte. Bu konu için nasıl ilerlenmeli ? int çevirmek doğru olur mu ?
            //var avgNonLin = NonLinearity(avgLin, 8);
            linearResult.MaxNonLinearProbability = Math.Abs(Math.Round(nonLinPerMax, 4));
            linearResult.MinNonLinearProbability = Math.Abs(Math.Round(nonLinPerMin, 4));
            linearResult.AvgNonLinearProbability = Math.Abs(Math.Round(nonLinPerAvg, 4));

            //Sac işlemleri
            var sac = sbox_sac_matrix(sboxArr, 3, 3);

            float totalSac = 0;
            var sacArr = new float[3];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {

                    totalSac += sac[i][j];
                }
                sacArr[i] = totalSac;
                totalSac = 0;
            }


            var sacMax = sacArr.ToList().Max();
            var sacMin = sacArr.ToList().Min();
            var sacAvg = ((sacMax / 4) + (sacMin / 4)) / 2;

            linearResult.SacAvg = Math.Round(sacAvg, 4);
            linearResult.SacMax = Math.Round((sacMax / 4), 4);
            linearResult.SacMin = Math.Round((sacMin / 4), 4);

            var bıc = sbox_bic(sboxArr, 3, 3);

            linearResult.BıcSacMin = Math.Abs(Math.Round((bıc - (double)(sacMax / 8)), 4));
            linearResult.BıcSacMax = Math.Abs(Math.Round((bıc - (double)(sacMin/8)), 4));
            linearResult.BıcSacAvg = Math.Abs(Math.Round((double)((linearResult.BıcSacMin+ linearResult.BıcSacMax)/2), 4));

            return linearResult;
        }

        public static byte Parity(byte sayi)
        {
            return Convert.ToByte(0x6996 >> ((sayi ^ sayi >> 4) & 15) & 1);
        }

        private double NonLinearity(int maxLin, int power)
        {
            var minNlp = 128 - maxLin;
            var minLinPer = Math.Round((minNlp / (Math.Pow(2, (power - 1)) - Math.Pow(2, (power / 2 - 1)))), 2);
            return minLinPer;
        }

        private int two_power(int n)
        {
            var result = 1u << (n);

            return Convert.ToInt32(result);
        }

        private float[][] sbox_sac_matrix(byte[] sboxArr, int n, int m)
        {
            int i, j, X, ei, ej, dei;
            var sac = new float[3][];
            sac[0] = new float[3];
            sac[1] = new float[3];
            sac[2] = new float[3];

            for (i = 0; i < m; ++i)
            {
                ei = two_power(i);
                for (j = 0; j < n; ++j)
                {
                    ej = two_power(j);
                    for (X = 0; X < two_power(m); ++X)
                    {
                        dei = sboxArr[X] ^ sboxArr[X ^ ei];
                        sac[i][j] += (dei & ej) >> j; // increment sac[i][j] if bit at position j of dei is set
                    }
                }
            }

            float sacResult = 0;
            float outputLength = (float)two_power(n);
            for (i = 0; i < m; ++i)
            {
                for (j = 0; j < n; ++j)
                {
                    sac[i][j] /= outputLength;
                    sacResult += sac[i][j];
                }
            }
            return sac;
        }

        private double sbox_bic(byte[] sboxArr, int m, int n)
        {

            int i, ei, X, dei;
            int j, k, ej, ek, dej, dek;
            int[] aval_vector_j = new int[256];
            int[] aval_vector_k = new int[256];
            double[] dizi = new double[500];
            double corr = 0d;
            double maxCorr = 0.0;
            int count = 0;
            // for each input bit position
            for (i = 0; i < m; ++i)
            {
                ei = two_power(i);

                // for each j, k output bit change if j != k
                for (j = 0; j < n; ++j)
                {
                    for (k = 0; k < n; ++k)
                    {
                        if (j != k)
                        {
                            aval_vector_j[j] = two_power(m);
                            aval_vector_k[k] = two_power(m);

                            // for each possible input
                            var s = two_power(m);
                            for (X = 0; X < s - 1; ++X)
                            {
                                ej = two_power(j);
                                ek = two_power(k);

                                dei = sboxArr[X] ^ sboxArr[X ^ ei];
                                dej = (dei & ej) >> j;
                                dek = (dei & ek) >> k;

                                aval_vector_j[X] = dej;
                                aval_vector_k[X] = dek;
                            }

                            var result = correlation(aval_vector_j, aval_vector_k, two_power(m));
                            corr = Math.Abs(result);

                            if (maxCorr < corr)
                            {
                                maxCorr = corr;
                                dizi[count] = corr;
                                count++;
                            }


                        }
                    }
                }
            }

            return maxCorr;
        }
        private double correlation(int[] x, int[] y, int n)
        {

            double sx = 0.0;
            double sy = 0.0;
            double sxx = 0.0;
            double syy = 0.0;
            double sxy = 0.0;
            int i;

            for (i = 0; i < n; ++i)
            {
                double xi = x[i];
                double yi = y[i];

                sx += xi;
                sy += yi;
                sxx += xi * xi;
                syy += yi * yi;
                sxy += xi * yi;
            }

            // covariation
            double cov = sxy / n - sx * sy / n / n;
            if (cov == 0) return 0.0;

            // standard error of x
            double sigmax = Math.Sqrt(sxx / n - sx * sx / n / n);
            // standard error of y
            double sigmay = Math.Sqrt(syy / n - sy * sy / n / n);

            // correlation is just a normalized covariation
            return cov / sigmax / sigmay;
        }

        public FileResult DownloadExcel()
        {
            string path = "/Doc/Users.xlsx";
            return File(path, "application/vnd.ms-excel", "Users.xlsx");
        }

        [HttpPost]
        public ActionResult Sbox(HttpPostedFileBase upload)
        {
            var resultSbox = string.Empty;
            if (upload != null && upload.ContentLength > 0)
            {
                // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                // to get started. This is how we avoid dependencies on ACE or Interop:
                Stream stream = upload.InputStream;

                IExcelDataReader reader = null;


                if (upload.FileName.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (upload.FileName.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else
                {
                    ModelState.AddModelError("File", "This file format is not supported");
                    return View();
                }
                int fieldcount = reader.FieldCount;
                int rowcount = reader.RowCount;
                System.Data.DataTable dt = new System.Data.DataTable();
                DataRow row;
                System.Data.DataTable dt_ = new System.Data.DataTable();
                try
                {
                    dt_ = reader.AsDataSet().Tables[0];
                    for (int i = 0; i < dt_.Columns.Count; i++)
                    {
                        dt.Columns.Add(dt_.Rows[0][i].ToString());
                    }
                    int rowcounter = 0;
                    for (int row_ = 0; row_ < dt_.Rows.Count; row_++)
                    {
                        row = dt.NewRow();

                        for (int col = 0; col < dt_.Columns.Count; col++)
                        {
                            row[col] = dt_.Rows[row_][col].ToString();
                            rowcounter++;
                        }
                        dt.Rows.Add(row);
                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("File", "Unable to Upload file!");
                    return View();
                }

                DataSet result = new DataSet();
                result.Tables.Add(dt);
                reader.Close();
                reader.Dispose();
                System.Data.DataTable tmp = result.Tables[0];

                string sboxStringValue = string.Empty;
                var count = 1;
                for (int i = 0; i < tmp.Rows.Count; i++)
                {
                    for (int j = 0; j < tmp.Rows[i].ItemArray.Count(); j++)
                    {
                        sboxStringValue += tmp.Rows[i].ItemArray[j].ToString();
                        if (count != 16)
                        {
                            sboxStringValue += ',';
                        }
                        count++;

                    }

                }

                resultSbox = ImportSbox(sboxStringValue);
                ViewBag.Sbox = resultSbox;
            }
            else
            {
                ModelState.AddModelError("File", "Please Upload Your file");
            }
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public FileResult ExportHTML(string ExportData)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader reader = new StringReader(ExportData);
                Document PdfFile = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(PdfFile, stream);
                PdfFile.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, PdfFile, reader);
                PdfFile.Close();
                return File(stream.ToArray(), "application/pdf", "ExportData.pdf");
            }
        }

        private int ComparisonMatris(string[] sboxValues)
        {
            string[] sboxBinaryData = new string[16];
            string[] sboxBinaryDataNew = new string[16];

            for (int i = 0; i < 16; i++)
            {
                sboxBinaryData[i] = HexToBinary(sboxValues[i]);
            }

            Random rnd = new Random();
            var rastgele = rnd.Next(0, 15);
            string defaultBitValue = string.Empty;
            defaultBitValue = sboxBinaryData[rastgele];
            char[]  defCharValue = defaultBitValue.ToCharArray();

            for (int i = 0; i < 16; i++)
            {
                if (sboxBinaryData[i] != defaultBitValue)
                {
                    char[] firstValue = sboxBinaryData[i].ToCharArray();
                    string newBit = string.Empty;

                    for (int j = 0; j < 4; j++)
                    {
                        if (defCharValue[j] != firstValue[j])
                        {
                            newBit += "1";
                        }
                        else
                        {
                            newBit += "0";
                        }
                    }
                    sboxBinaryDataNew[i] = newBit;
                }
            }
           var a= defaultBitValue.Substring(0, 4);
            sboxBinaryDataNew[rastgele] = a;

            var bitDataDecimalTotalVal = CovertDec(sboxBinaryData);
            var bitDataNewDecimalTotalVal = CovertDec(sboxBinaryDataNew);

            var resultBicSac = bitDataDecimalTotalVal - bitDataNewDecimalTotalVal; 
            return resultBicSac;
        }

        //matrisin bit değerlerinin decimal toplamı.
        private int CovertDec(string [] bitData)
        {
            var bitDataTotal = 0;
            for (int i = 0; i < 16; i++)
            {
                char[] firstValue = bitData[i].ToCharArray();

                for (int j = 0; j < 4; j++)
                {
                    if (firstValue[j]=='1')
                    {
                        switch (j)
                        {
                            case 0:
                                bitDataTotal += 8;
                                break;
                            case 1:
                                bitDataTotal += 4;
                                break;
                            case 2:
                                bitDataTotal += 2;
                                break;
                            case 3:
                                bitDataTotal += 1;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return bitDataTotal;
        }

        public static string HexToBinary(string hexValue)
        {
            ulong number = UInt64.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            byte[] bytes = BitConverter.GetBytes(number);

            string binaryString = string.Empty;
            foreach (byte singleByte in bytes)
            {
                binaryString += Convert.ToString(singleByte,2);
            }

            return binaryString;
        }
    }
}