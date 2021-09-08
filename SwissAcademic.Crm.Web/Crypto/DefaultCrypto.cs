/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */
using SwissAcademic.Crm.Web.Helpers;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SwissAcademic.Crm.Web
{
    public class DefaultCrypto
    {
        public const char PasswordHashingIterationCountSeparator = '.';

        public string Hash(string value)
        {
            return Crypto.Hash(value);
        }

        public bool VerifyHash(string value, string hash)
        {
            var hashedValue = Hash(value);
            return SlowEquals(hashedValue, hash);
        }

        public string Hash(string value, string key)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException("value");
            }

            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            using (var alg = new System.Security.Cryptography.HMACSHA512(keyBytes))
            {
                var hash = alg.ComputeHash(valueBytes);

                var result = Crypto.BinaryToHex(hash);
                return result;
            }
        }

        public bool VerifyHash(string value, string key, string hash)
        {
            var hashedValue = Hash(value, key);
            return SlowEquals(hashedValue, hash);
        }

        public string GenerateNumericCode(int digits)
        {
            // 18 is good size for a long
            if (digits > 18)
            {
                digits = 18;
            }

            if (digits <= 0)
            {
                digits = 6;
            }

            var bytes = Crypto.GenerateSaltInternal(sizeof(long));
            var val = BitConverter.ToInt64(bytes, 0);
            var mod = (int)Math.Pow(10, digits);
            val %= mod;
            val = Math.Abs(val);

            return val.ToString("D" + digits);
        }

        public string GenerateSalt()
        {
            return Crypto.GenerateSalt();
        }

        public string HashPassword(string password, int iterations)
        {
            var count = iterations;
            if (count <= 0)
            {
                //count = GetIterationsFromYear(GetCurrentYear());
                count = 50000; // changed to a default of 50K due to outdated OWASP/PKCS
            }
            var result = Crypto.HashPassword(password, count);
            return EncodeIterations(count) + PasswordHashingIterationCountSeparator + result;
        }

        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword.Contains(PasswordHashingIterationCountSeparator))
            {
                var parts = hashedPassword.Split(PasswordHashingIterationCountSeparator);
                if (parts.Length != 2)
                {
                    return false;
                }

                int count = DecodeIterations(parts[0]);
                if (count <= 0)
                {
                    return false;
                }

                hashedPassword = parts[1];

                return Crypto.VerifyHashedPassword(hashedPassword, password, count);
            }
            else
            {
                return Crypto.VerifyHashedPassword(hashedPassword, password);
            }
        }

        public bool SlowEquals(string a, string b)
        {
            return SlowEqualsInternal(a, b);
        }

        public string EncodeIterations(int count)
        {
            return count.ToString("X");
        }

        public int DecodeIterations(string prefix)
        {
            int val;
            if (Int32.TryParse(prefix, System.Globalization.NumberStyles.HexNumber, null, out val))
            {
                return val;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static bool SlowEqualsInternal(string a, string b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool same = true;
            for (var i = 0; i < a.Length; i++)
            {
                same &= (a[i] == b[i]);
            }
            return same;
        }
    }
}
