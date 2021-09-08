namespace System.Net
{
    public static class IPAddressUtility
    {
        public static decimal ConvertToDecimal(this IPAddress address)
        {
            var bytes = address.GetAddressBytes();
            if (System.BitConverter.IsLittleEndian) Array.Reverse(bytes);
            if (address.AddressFamily == Sockets.AddressFamily.InterNetworkV6) return BitConverter.ToUInt64(bytes, 0);
            return BitConverter.ToUInt32(bytes, 0);
            //http://stackoverflow.com/questions/461742/how-to-convert-an-ipv4-address-into-a-integer-in-c/13350494#13350494
        }
    }
}
