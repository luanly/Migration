using System;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;

namespace SwissAcademic.Testing
{
	public static class TestEnvironment
	{
		#region ForceLargeChangeset

		public static bool ForceLargeChangeset
		{
			get
			{
				var value = CallContext2.GetData(nameof(ForceLargeChangeset));
				return value == null ? false : (bool)value;
			}
			set
			{
				if (value)
				{
					CallContext2.SetData(nameof(ForceLargeChangeset), value);
				}
				else
				{
					CallContext2.SetData(nameof(ForceLargeChangeset), null);
				}
			}
		}

		#endregion

		#region ForceNetworkAvailabilityChanged

#if !Web
        public static void ForceNetworkAvailabilityChanged(bool isAvailable)
        {
            SwissAcademic.Net.NetworkInformation.NetworkChange_NetworkAvailabilityChanged(isAvailable);
        }
#endif

		#endregion

		#region IsTestHickupException

		public static bool IsTestHickupException(Exception exception)
		{
			if (exception == null) return false;

			var aggregateException = exception as AggregateException;

			if (aggregateException == null)
			{
				if (exception.Message.Contains("TestHickup")) return true;
				return IsTestHickupException(exception.InnerException);
			}
			else
			{
				return aggregateException.InnerExceptions.Any(x => IsTestHickupException(x));
			}
		}

		#endregion

		#region ShouldRaiseTestHickupException

		static object _lock = new object();
		static int _testHickupCounter;

		public static bool ShouldRaiseTestHickupException
		{
			get
			{
				if (_testHickupExceptionRate == 0) return false;

				lock (_lock)
				{
					_testHickupCounter++;
					var value = _testHickupCounter % TestHickupExceptionRate == 0;
					return value;
				}
			}
		}

		#endregion

		#region TestHickupExceptionRate

		static int _testHickupExceptionRate;

		public static int TestHickupExceptionRate
		{
			get { return _testHickupExceptionRate; }
			set
			{
				lock (_lock)
				{
					_testHickupExceptionRate = value;
				}
			}
		}

		#endregion
	}
}
