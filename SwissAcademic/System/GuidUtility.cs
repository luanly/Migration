using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System
{
    public static class GuidUtility
    {
        public static Guid CreateGuidFromString(string guidString)
        {
            if (string.IsNullOrWhiteSpace(guidString)) return Guid.Empty;
            Guid guid;
            if (!Guid.TryParse(guidString, out guid))
            {
#pragma warning disable CA5351, SCS0006 // Keine beschädigten kryptografischen Algorithmen verwenden
                using (var md5 = MD5.Create())
#pragma warning restore CA5351, SCS0006 // Keine beschädigten kryptografischen Algorithmen verwenden
                {
                    var hash = md5.ComputeHash(Encoding.Default.GetBytes(guidString));
                    guid = new Guid(hash);
                }

            }
            return guid;
        }

        public static string CreateSafeGuidString()
        {
            // return a Guid that starts with a character and has no dashes
            // suitable for random filenames and the like.
            return "a" + Guid.NewGuid().ToString("N").Substring(1);
        }

        public static string To16ByteString(this Guid guid)
        {
            //in sqlite 85feebf0-5ee0-4734-96cc-23383e66a4bf is equivalent to x'F0EBFE85E05E344796CC23383E66A4BF'
            //F0EBFE85E05E344796CC23383E66A4BF is the 16-byte array representation

            if (guid == null) return string.Empty;
            return string.Join("", Array.ConvertAll<byte, string>(guid.ToByteArray(), item => string.Format("{0:X2}", item)));
        }

        public static Guid ToGuid(object value)
        {
            if (value == null || value == DBNull.Value) return Guid.Empty;

            switch (value)
            {
                case Guid g:
                    return (Guid)value;

                case String s:
                    return new Guid(s);

                case byte[] b:
                    return new Guid(b);

                default:
                    throw new NotSupportedException($"type {value.GetType().Name} cannot be converted to a Guid");
            }
        }

        public static Guid? ToNullableGuid(object value)
        {
            if (value == null || value == DBNull.Value) return null;

            switch (value)
            {
                case Guid g:
                    return (Guid)value;

                case String s:
                    return new Guid(s);

                case byte[] b:
                    return new Guid(b);

                default:
                    throw new NotSupportedException($"type {value.GetType().Name} cannot be converted to a Guid");
            }
        }


        public static List<Guid> ToGuids(object value)
        {
            if (value == null) return new List<Guid>();

            Guid g;
            switch (value)
            {
                case List<Guid> guids:
                    return guids;

                case IEnumerable<Guid> e:
                    return e.ToList();

                case Guid guid:
                    return new List<Guid> { (Guid)value };

                case string s:
                    return (from item in s.Split(';')
                            let gs = item.Trim()
                            where !string.IsNullOrEmpty(gs)
                            where Guid.TryParse(gs, out g)
                            select g).ToList();

                default:
                    throw new NotSupportedException($"type {value.GetType().Name} cannot be converted to a List<Guid>");
            }
        }

        public static IEnumerable<Guid> ToGuids(object value, bool throwErrorIfNotSupported = true)
        {
            if (value == null) return Enumerable.Empty<Guid>();

            Guid guid;
            switch (value)
            {
                case Guid g:
                    return new[] { g };

                case IEnumerable<Guid> e:
                    return e;

                case string s:
                    return from item in s.Split(';')
                           let gs = item.Trim()
                           where !string.IsNullOrEmpty(gs)
                           where Guid.TryParse(gs, out guid)
                           select guid;

                default:
                    if (throwErrorIfNotSupported)
                    {
                        throw new NotSupportedException($"type {value.GetType().Name} cannot be converted to a List<Guid>");
                    }
                    else
                    {
                        return null;
                    }
            }
        }
    }
}
