#if !Web
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SwissAcademic.Net
{
    public static class NetworkInformation
    {
        #region Felder


        static Dictionary<string, bool> _checkedConnections = new Dictionary<string, bool>();

        static object _locky = new object();

        static bool _getHostAddressesFailed;

        #endregion

        #region Konstruktor

        static NetworkInformation()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        #endregion

        #region Eigenschaften

        internal static bool? _isNetworkAvailable;
        public static bool IsNetworkAvailable
        {
            get
            {
                if (_isNetworkAvailable.HasValue)
                {
                    return _isNetworkAvailable.Value;
                }

                lock (_locky)
                {
                    if (_isNetworkAvailable.HasValue)
                    {
                        return _isNetworkAvailable.Value;
                    }
                    _isNetworkAvailable = GetIsNetworkAvailable();
                    return _isNetworkAvailable.Value;
                }
            }
        }

        #endregion

        #region Methoden

        #region CanConnectToHost

        public static bool CanConnectToHost(string hostname, int port)
        {
            lock (_locky)
            {
                if (hostname == "0.0.0.0")
                {
                    return false;
                }

                bool canConnectToHost;
                if (_checkedConnections.TryGetValue(hostname, out canConnectToHost))
                {
                    return canConnectToHost;
                }

                try
                {
                    using (var clientsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        clientsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
                        clientsock.Connect(hostname, port);
                        clientsock.Shutdown(SocketShutdown.Both);
                        canConnectToHost = true;
                    }
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                }

                if (!canConnectToHost)
                {
                    var tt = new SasTraceTelemetry
                    {
                        Message = "Connection to host could not be established",
                        SeverityLevel = SeverityLevel.Information
                    };

                    tt.Properties[nameof(hostname)] = hostname;
                    tt.Properties[nameof(port)] = port.ToString();

                    Telemetry.TrackTrace(tt);
                }

                _checkedConnections[hostname] = canConnectToHost;

                return canConnectToHost;
            }
        }

        #endregion

        #region EnsureNetworkIsAvailable

        public static void EnsureNetworkIsAvailable()
        {
            if (!IsNetworkAvailable)
            {
                throw new NotConnectedToCloudException();
            }
        }

        #endregion 

        #region GetHostAddress

        public static System.Net.IPAddress GetHostAddress(string hostNameOrAddress)
        {
            lock (_locky)
            {
                if (_getHostAddressesFailed)
                {
                    return new System.Net.IPAddress(new byte[] { 0, 0, 0, 0 });
                }
                try
                {
                    return System.Net.Dns.GetHostAddresses(hostNameOrAddress).First();
                }

                catch (Exception ignored)
                {
                    //JHP Log File einer Kundin weist auf DNS Probleme hin, aber Fehler fehlte bis anhin
                    Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat, (nameof(hostNameOrAddress), hostNameOrAddress));
                    _getHostAddressesFailed = true;
                    return new System.Net.IPAddress(new byte[] { 0, 0, 0, 0 });
                }
            }
        }

        #endregion

        #region GetIsNetworkAvailable

        // https://stackoverflow.com/questions/520347/how-do-i-check-for-a-network-connection/38179695#38179695

        /// <summary>
        /// Indicates whether any network connection is available.
        /// Filter connections below a specified speed, as well as virtual network cards.
        /// </summary>
        /// <param name="minimumSpeed">The minimum speed required. Passing 0 will not filter connection using speed.</param>
        /// <returns>
        ///     <c>true</c> if a network connection is available; otherwise, <c>false</c>.
        /// </returns>
        public static bool GetIsNetworkAvailable(long minimumSpeed = 0, StringBuilder logger = null)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                logger?.AppendLine("IsNetworkAvailable: false");
                return false;
            }

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                try
                {
                    logger?.AppendLine("");

                    // discard because of standard reasons
                    if ((ni.OperationalStatus != OperationalStatus.Up) ||
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    {
                        logger?.AppendLine("Skip " + ni.Name);
                        logger?.AppendLine("OperationalStatus: " + ni.OperationalStatus);
                        logger?.AppendLine("NetworkInterfaceType: " + ni.NetworkInterfaceType);
                        continue;
                    }

                    // this allow to filter modems, serial, etc.
                    // I use 10000000 as a minimum speed for most cases
                    if (ni.Speed < minimumSpeed)
                    {
                        logger?.AppendLine("Skip " + ni.Name);
                        logger?.AppendLine("Speed: " + ni.Speed);
                        continue;
                    }

                    // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                    if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
					{
                        logger?.AppendLine("Skip " + ni.Name);
                        logger?.AppendLine("Description: " + ni.Description);
                        continue;
                    }

                    logger?.AppendLine("OK: " + ni.Name);

                    return true;
                }
                catch(Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Eventhandlers

        static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            _isNetworkAvailable = e.IsAvailable;
            Telemetry.TrackTrace($"NetworkAvailabilityChanged. {nameof(NetworkAvailabilityEventArgs.IsAvailable)}: {e.IsAvailable}", SeverityLevel.Verbose);
        }

        /// <summary>
        /// For Testing purposes
        /// </summary>
        internal static void NetworkChange_NetworkAvailabilityChanged(bool isAvailable)
        {
            _isNetworkAvailable = isAvailable;
        }

        #endregion
    }
}
#endif