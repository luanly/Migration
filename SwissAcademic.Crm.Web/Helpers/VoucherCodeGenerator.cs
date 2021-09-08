using System;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    internal class VoucherCodeGenerator
    {
        internal VoucherCodeInfo CreateVoucherCode()
        {
            var voucherCode = new VoucherCodeInfo();
            var random = new Random();

            var pw = string.Empty;

            var pwdGen = new IdGenerator();
            pwdGen.Minimum = 8;
            pwdGen.Maximum = 12;
            pwdGen.Exclusions = "O01IC";
            pw = pwdGen.GenerateId();

            voucherCode.VoucherCodePre = "C-" + pw.ToUpperInvariant();
            var formattedNumber = new StringBuilder(voucherCode.VoucherCodePre);
            var j = 5;
            while (j < formattedNumber.Length)
            {
                formattedNumber.Insert(j, "-");
                j += 4;
            }

            voucherCode.VoucherCode = formattedNumber.ToString();
            return voucherCode;
        }

        #region Helperclass ID Generator

        public class IdGenerator
        {
            const int DefaultMinimum = 7;
            const int DefaultMaximum = 20;

            int minSize;
            int maxSize;
            string exclusionSet;

            public string Exclusions
            {
                get { return exclusionSet; }
                set { exclusionSet = value; }
            }

            public int Minimum
            {
                get { return minSize; }
                set
                {
                    minSize = value;
                    if (DefaultMinimum > minSize)
                    {
                        minSize = DefaultMinimum;
                    }
                    if (minSize > DefaultMaximum)
                    {
                        minSize = DefaultMaximum;
                    }
                }
            }

            public int Maximum
            {
                get { return maxSize; }
                set
                {
                    maxSize = value;
                    if (minSize > maxSize)
                    {
                        maxSize = IdGenerator.DefaultMaximum;
                    }
                }
            }

            public IdGenerator()
            {
                Minimum = DefaultMinimum;
                Maximum = DefaultMaximum;
                Exclusions = string.Empty;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Keine unsichere Zufälligkeitsstufe verwenden", Justification = "Nur für Vouchers - Nicht Sicherheitrelevant")]
            public string GenerateId()
            {
                Random random = new Random();

                int lngth = random.Next(minSize, maxSize + 1);
                string id = string.Empty;
                int strNum = 0;

                while (id.Length < lngth)
                {
                    strNum = random.Next(1, 3);
                    var nextCharacter = string.Empty;

                    switch (strNum)
                    {
                        case 2:
                            {
                                nextCharacter = random.Next(1, 10).ToString();
                                break;
                            }
                        case 1:
                        default:
                            {
                                nextCharacter = Char.ConvertFromUtf32(random.Next(65, 91));
                                break;
                            }
                    }

                    if (!string.IsNullOrEmpty(Exclusions))
                    {
                        if (Exclusions.IndexOf(nextCharacter) == -1)
                        {
                            id += nextCharacter;
                        }
                    }
                }

                return id;
            }
        }

        #endregion

    }
    public class VoucherCodeInfo
    {
        public string VoucherCode { get; set; }
        public int VoucherCodeInt { get; set; }
        public string VoucherCodePre { get; set; }
    }
}
