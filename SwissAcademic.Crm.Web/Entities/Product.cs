using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Product)]
    [DataContract]
    public class Product
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Product()
            :
            base(CrmEntityNames.Product)
        {

        }

        #endregion

        #region Eigenschaften

        #region CitaviMajorVersion

        public int CitaviMajorVersion
        {
            get
            {
                switch (CitaviProductCode)
                {
                    case ProductCodes.C2Pro:
                        return 2;

                    case ProductCodes.C3Pro:
                    case ProductCodes.C3UpgradeProTeam:
                    case ProductCodes.C3Reader:
                    case ProductCodes.C3Team:
                        return 3;

                    case ProductCodes.C4Pro:
                    case ProductCodes.C4Reader:
                    case ProductCodes.C4Team:
                    case ProductCodes.C4Up3T4:
                    case ProductCodes.C4Up4T4:
                    case ProductCodes.C4Up3P4:
                    case ProductCodes.C4Ur3R4:
                    case ProductCodes.C4Ut3T4:
                        return 4;

                    case ProductCodes.C5DBServerPerSeat:
                    case ProductCodes.C5DBServerConcurrent:
                    case ProductCodes.C5Windows:
                    case ProductCodes.C5UpC3:
                    case ProductCodes.C5UpC4:
                    case ProductCodes.C5dbsUpC3:
                    case ProductCodes.C5dbsUpC4:
                    case ProductCodes.C5dbcUpC3:
                    case ProductCodes.C5dbcUpC4:
                    case ProductCodes.C5dbsUpC5:
                    case ProductCodes.C5dbcUpC5:
                    case ProductCodes.C5DBServerREADER:
                    case "c5dbr5":
                    case "c5dbr10":
                    case "c5dbrx":
                        return 5;

                    case ProductCodes.C6DBServerCONCURRENT:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC5:
                    case ProductCodes.C6DBServerPerSeat:
                    case ProductCodes.C6DBServerPerSeatUpgradeC4:
                    case ProductCodes.C6DBServerPerSeatUpgradeC5:
                    case ProductCodes.C6DBServerReader:
                    case ProductCodes.C6Windows:
                    case ProductCodes.C6WindowsUpgradeC4:
                    case ProductCodes.C6WindowsUpgradeC5:
                        return 6;

                    case ProductCodes.CitaviWeb:
                    case ProductCodes.CitaviSpace:
                        return 0; //Darf nicht grösser als 6 sein, sonst kann Citavi 6 Desktop das nicht korrekt angezeigt, da dort nach MajorVersion sortiert wird

                    case ProductCodes.CitaviDBServerCONCURRENTSubscription:
                    case ProductCodes.CitaviDBServerPERSEATSubscription:
                    case ProductCodes.CitaviDBServerREADERSubscription:
                    case ProductCodes.CitaviWebAndWin:
                        return CrmConfig.CurrentLicenseMajorVersion;//Entspricht immer der aktuellen C-Version
                }

                return -1;
            }
        }

        #endregion

        #region CitaviProductCode

        [CrmProperty]
        [DataMember]
        public string CitaviProductCode
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CitaviProductName

        [CrmProperty]
        [DataMember]
        public string CitaviProductName
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region IsCampusContractProduct

        /// <summary>
        /// Wenn false, dürfen wir keine Campuslizenz für dieses Produkt ausstellen
        /// Beispiel SQL-Server Produkte. Die werden nur via Verlängerung oder via UI erstellt
        /// </summary>
        public bool IsCampusContractProduct
        {
            get
            {
                if (IsSqlServerProduct)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region IsCitaviSpace

        public bool IsCitaviSpace => CitaviProductCode == ProductCodes.CitaviSpace;

        #endregion

        #region IsSqlServerProduct

        public bool IsSqlServerProduct
        {
            get
            {
                switch (CitaviProductCode)
                {
                    case ProductCodes.C5DBServerPerSeat:
                    case ProductCodes.C5DBServerConcurrent:
                    case ProductCodes.C5dbsUpC3:
                    case ProductCodes.C5dbsUpC4:
                    case ProductCodes.C5dbcUpC3:
                    case ProductCodes.C5dbcUpC4:
                    case ProductCodes.C5dbsUpC5:
                    case ProductCodes.C5dbcUpC5:
                    case ProductCodes.C5DBServerREADER:

                    case ProductCodes.C6DBServerCONCURRENT:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC5:
                    case ProductCodes.C6DBServerPerSeat:
                    case ProductCodes.C6DBServerPerSeatUpgradeC4:
                    case ProductCodes.C6DBServerPerSeatUpgradeC5:
                    case ProductCodes.C6DBServerReader:

                    case ProductCodes.CitaviDBServerCONCURRENTSubscription:
                    case ProductCodes.CitaviDBServerPERSEATSubscription:
                    case ProductCodes.CitaviDBServerREADERSubscription:

                        return true;
                }
                return false;
            }
        }

        #endregion

        #region IsSqlServerConcurrentProduct

        public bool IsSqlServerConcurrentProduct
        {
            get
            {
                switch (CitaviProductCode)
                {
                    case ProductCodes.C5dbcUpC5:
                    case ProductCodes.C5dbcUpC4:
                    case ProductCodes.C5dbcUpC3:
                    case ProductCodes.C5DBServerConcurrent:

                    case ProductCodes.C6DBServerCONCURRENT:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC5:

                    case ProductCodes.CitaviDBServerCONCURRENTSubscription:
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region IsSqlServerReader

        public bool IsSqlServerReader
        {
            get
            {
                switch (CitaviProductCode)
                {
                    case ProductCodes.C5DBServerREADER:
                    case ProductCodes.C6DBServerReader:
                    case ProductCodes.CitaviDBServerREADERSubscription:
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region IsDesktopSubscription

        public bool IsDesktopSubscription => CitaviProductCode == ProductCodes.CitaviWebAndWin;

        #endregion

        #region IsSQLServerSubscription

        public bool IsSQLServerSubscription => CitaviProductCode == ProductCodes.CitaviDBServerREADERSubscription ||
                                               CitaviProductCode == ProductCodes.CitaviDBServerCONCURRENTSubscription ||
                                               CitaviProductCode == ProductCodes.CitaviDBServerPERSEATSubscription;

        #endregion

        #region IsCitaviWeb

        public bool IsCitaviWeb => CitaviProductCode == ProductCodes.CitaviWeb;

        #endregion

        #region IsSubscription

        public bool IsSubscription => IsCitaviWeb || IsCitaviSpace || IsDesktopSubscription || IsSQLServerSubscription;

        #endregion

        #region IsRental

        public bool IsRental
        {
            get
            {
                switch(CitaviProductCode)
                {
                    case ProductCodes.CitaviDBServerCONCURRENTSubscription:
                    case ProductCodes.CitaviDBServerPERSEATSubscription:
                    case ProductCodes.CitaviDBServerREADERSubscription:
                        return true;

                    default:
                        return false;
                }
            }
        }

        #endregion

        #region IsUpgradeProduct

        bool? _isUpgradeProduct;
        public bool IsUpgradeProduct
		{
            get
            {
				if (_isUpgradeProduct.HasValue)
				{
                    return _isUpgradeProduct.Value;
                }
                switch (CitaviProductCode)
                {
                    case ProductCodes.C3UpgradeProTeam:
                    case ProductCodes.C4Up3P4:
                    case ProductCodes.C4Up3T4:
                    case ProductCodes.C4Up4T4:
                    case ProductCodes.C4Ur3R4:
                    case ProductCodes.C4Ut3T4:
                    case ProductCodes.C5dbcUpC3:
                    case ProductCodes.C5dbcUpC4:
                    case ProductCodes.C5dbcUpC5:
                    case ProductCodes.C5dbsUpC3:
                    case ProductCodes.C5dbsUpC4:
                    case ProductCodes.C5dbsUpC5:
                    case ProductCodes.C5UpC3:
                    case ProductCodes.C5UpC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC5:
                    case ProductCodes.C6DBServerPerSeatUpgradeC4:
                    case ProductCodes.C6DBServerPerSeatUpgradeC5:
                    case ProductCodes.C6WindowsUpgradeC4:
                    case ProductCodes.C6WindowsUpgradeC5:
                        {
                            _isUpgradeProduct = true;
                            return true;
                        }
                }

                _isUpgradeProduct = false;
                return false;
            }
        }

        #endregion

        #region UpgradeFromCitaviMajorVersion

        int? _upgradeFromCitaviMajorVersion;
        public int UpgradeFromCitaviMajorVersion
        {
            get
            {
                if (_upgradeFromCitaviMajorVersion.HasValue)
                {
                    return _upgradeFromCitaviMajorVersion.Value;
                }
                switch (CitaviProductCode)
                {
                    case ProductCodes.C3UpgradeProTeam:
                    case ProductCodes.C4Up3P4:
                    case ProductCodes.C4Up3T4:
                    case ProductCodes.C4Ur3R4:
                    case ProductCodes.C4Ut3T4:
                    case ProductCodes.C5dbcUpC3:
                    case ProductCodes.C5dbsUpC3:
                    case ProductCodes.C5UpC3:
                        {
                            _upgradeFromCitaviMajorVersion = 3;
                        }
                        break;

                    case ProductCodes.C4Up4T4:
                    case ProductCodes.C5dbcUpC4:
                    case ProductCodes.C5dbsUpC4:
                    case ProductCodes.C5UpC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                    case ProductCodes.C6DBServerPerSeatUpgradeC4:
                    case ProductCodes.C6WindowsUpgradeC4:
                        {
                            _upgradeFromCitaviMajorVersion = 4;
                        }
                        break;

                    case ProductCodes.C5dbcUpC5:
                    case ProductCodes.C5dbsUpC5:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC5:
                    case ProductCodes.C6DBServerPerSeatUpgradeC5:
                    case ProductCodes.C6WindowsUpgradeC5:
                        {
                            _upgradeFromCitaviMajorVersion = 5;
                        }
                        break;

                    default:
                        _upgradeFromCitaviMajorVersion = -1;
                        break;
                }
                
                return _upgradeFromCitaviMajorVersion.Value;
            }
        }

        #endregion

        #region UpgradeToBaseProduct

        /// <summary>
        /// C6DBServerCONCURRENTUpgradeC4 -> C6DBServerCONCURRENT
        /// C6WindowsUpgradeC4 -> C6Windows
        /// </summary>

        Product _upgradeToBaseProduct;
        public Product UpgradeToBaseProduct
        {
            get
            {
                if (_upgradeToBaseProduct != null)
                {
                    return _upgradeToBaseProduct;
                }
                switch (CitaviProductCode)
                {
                    case ProductCodes.C3UpgradeProTeam:
                        _upgradeToBaseProduct = Product.C3Team;
                        break;

                    case ProductCodes.C4Up3P4:
                    case ProductCodes.C4Up3T4:
                    case ProductCodes.C4Up4T4:
                    case ProductCodes.C4Ur3R4:
                    case ProductCodes.C4Ut3T4:
                        _upgradeToBaseProduct = Product.C4Pro;
                        break;

                    case ProductCodes.C5dbcUpC3:
                    case ProductCodes.C5dbcUpC4:
                    case ProductCodes.C5dbcUpC5:
                        _upgradeToBaseProduct = Product.C5DBServerConcurrent;
                        break;

                    case ProductCodes.C5dbsUpC3:
                    case ProductCodes.C5dbsUpC4:
                    case ProductCodes.C5dbsUpC5:
                        _upgradeToBaseProduct = Product.C5DBServerPerSeat;
                        break;

                    case ProductCodes.C5UpC3:
                    case ProductCodes.C5UpC4:
                        _upgradeToBaseProduct = Product.C5Windows;
                        break;

                    case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                    case ProductCodes.C6DBServerCONCURRENTUpgradeC5:
                        _upgradeToBaseProduct = Product.C6DBServerCONCURRENT;
                        break;

                    case ProductCodes.C6DBServerPerSeatUpgradeC4:
                    case ProductCodes.C6DBServerPerSeatUpgradeC5:
                        _upgradeToBaseProduct = Product.C6DBServerPerSeat;
                        break;

                    case ProductCodes.C6WindowsUpgradeC4:
                    case ProductCodes.C6WindowsUpgradeC5:
                        _upgradeToBaseProduct = Product.C6Windows;
                        break;

                    default:
                        return null;
                }

                return _upgradeToBaseProduct;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(ProductPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region IsEqualDBServerProduct

        internal bool IsEqualDBServerProduct(Product product)
		{
            switch (CitaviProductCode)
            {
                case ProductCodes.C5DBServerPerSeat:
                case ProductCodes.C5dbsUpC3:
                case ProductCodes.C5dbsUpC4:
                case ProductCodes.C5dbsUpC5:
                case ProductCodes.C6DBServerPerSeat:
                case ProductCodes.C6DBServerPerSeatUpgradeC4:
                case ProductCodes.C6DBServerPerSeatUpgradeC5:
                    return product == Product.C6DBServerPerSeat;

                case ProductCodes.C5DBServerConcurrent:
                case ProductCodes.C5dbcUpC3:
                case ProductCodes.C5dbcUpC4:
                case ProductCodes.C5dbcUpC5:
                case ProductCodes.C6DBServerCONCURRENT:
                case ProductCodes.C6DBServerCONCURRENTUpgradeC4:
                case ProductCodes.C6DBServerCONCURRENTUpgradeC5:
                    return product == Product.C6DBServerCONCURRENT;

            }

            return false;
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(CitaviProductName))
            {
                return CitaviProductName;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region Statische Eigenschaften

        #region C2Pro

        public static Product C2Pro => CrmCache.ProductsByCode[ProductCodes.C2Pro];

        #endregion

        #region C3Pro

        public static Product C3Pro => CrmCache.ProductsByCode[ProductCodes.C3Pro];

        #endregion

        #region Citavi 3 Upgrade Pro Team

        public static Product C3UpgradeProTeam => CrmCache.ProductsByCode[ProductCodes.C3UpgradeProTeam];

        #endregion

        #region C3Reader

        public static Product C3Reader => CrmCache.ProductsByCode[ProductCodes.C3Reader];

        #endregion

        #region C3Team

        public static Product C3Team => CrmCache.ProductsByCode[ProductCodes.C3Team];

        #endregion

        #region C4Pro

        public static Product C4Pro => CrmCache.ProductsByCode[ProductCodes.C4Pro];

        #endregion

        #region C4Reader

        public static Product C4Reader => CrmCache.ProductsByCode[ProductCodes.C4Reader];

        #endregion

        #region Citavi 4 Team

        public static Product C4Team => CrmCache.ProductsByCode[ProductCodes.C4Team];

        #endregion

        #region Citavi 4 Team (Upgrade Citavi 3 Pro)

        /// <summary>
        /// Citavi 4 Team (Upgrade Citavi 3 Pro)
        /// </summary>
        public static Product C4Up3T4 => CrmCache.ProductsByCode[ProductCodes.C4Up3T4];

        #endregion

        #region Citavi 4 Team (Upgrade Citavi 4 Pro)

        /// <summary>
        /// Citavi 4 Team (Upgrade Citavi 4 Pro)
        /// </summary>
        public static Product C4Up4T4 => CrmCache.ProductsByCode[ProductCodes.C4Up4T4];

        #endregion

        #region Citavi 4 Pro (Upgrade Citavi 3 Pro)

        /// <summary>
        /// Citavi 4 Pro (Upgrade Citavi 3 Pro)
        /// </summary>
        public static Product C4Up3P4 => CrmCache.ProductsByCode[ProductCodes.C4Up3P4];

        #endregion

        #region Citavi 4 Reader (Upgrade Citavi 3 Reader)

        /// <summary>
        /// Citavi 4 Reader (Upgrade Citavi 3 Reader)
        /// </summary>
        public static Product C4Ur3R4 => CrmCache.ProductsByCode[ProductCodes.C4Ur3R4];

        #endregion

        #region Citavi 4 Team (Upgrade Citavi 3 Team)

        /// <summary>
        /// Citavi 4 Team (Upgrade Citavi 3 Team)
        /// </summary>
        public static Product C4Ut3T4 => CrmCache.ProductsByCode[ProductCodes.C4Ut3T4];

        #endregion

        #region Citavi 5 for DBServer PER SEAT

        /// <summary>
        /// Citavi 5 for DBServer PER SEAT
        /// </summary>
        public static Product C5DBServerPerSeat => CrmCache.ProductsByCode[ProductCodes.C5DBServerPerSeat];

        #endregion

        #region Citavi 5 for DBServer CONCURRENT

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT
        /// </summary>
        public static Product C5DBServerConcurrent => CrmCache.ProductsByCode[ProductCodes.C5DBServerConcurrent];

        #endregion

        #region Citavi 5 for Windows

        /// <summary>
        /// Citavi 5 for Windows
        /// </summary>
        public static Product C5Windows => CrmCache.ProductsByCode[ProductCodes.C5Windows];

        #endregion

        #region Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 5)
        /// </summary>
        public static Product C5dbcUpC5 => CrmCache.ProductsByCode[ProductCodes.C5dbcUpC5];

        #endregion

        #region Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 5)
        /// </summary>
        public static Product C5dbsUpC5 => CrmCache.ProductsByCode[ProductCodes.C5dbsUpC5];

        #endregion

        #region Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 4)
        /// </summary>
        public static Product C5dbcUpC4 => CrmCache.ProductsByCode[ProductCodes.C5dbcUpC4];

        #endregion

        #region Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 3)

        /// <summary>
        /// Citavi 5 for DBServer CONCURRENT - Business (Upgrade Citavi 3)
        /// </summary>
        public static Product C5dbcUpC3 => CrmCache.ProductsByCode[ProductCodes.C5dbcUpC3];

        #endregion

        #region Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 4)
        /// </summary>
        public static Product C5dbsUpC4 => CrmCache.ProductsByCode[ProductCodes.C5dbsUpC4];

        #endregion

        #region Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 3)

        /// <summary>
        /// Citavi 5 for DBServer SEAT - Business (Upgrade Citavi 3)
        /// </summary>
        public static Product C5dbsUpC3 => CrmCache.ProductsByCode[ProductCodes.C5dbsUpC3];

        #endregion

        #region Citavi 5 for Windows (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 5 for Windows (Upgrade Citavi 4)
        /// </summary>
        public static Product C5UpC4 => CrmCache.ProductsByCode[ProductCodes.C5UpC4];

        #endregion

        #region Citavi 5 for Windows (Upgrade Citavi 3)

        /// <summary>
        /// Citavi 5 for Windows (Upgrade Citavi 3)
        /// </summary>
        public static Product C5UpC3 => CrmCache.ProductsByCode[ProductCodes.C5UpC3];

        #endregion

        #region Citavi 5 for DBServer READER

        /// <summary>
        /// Citavi 5 for DBServer READER 100
        /// </summary>
        public static Product C5DBServerREADER => CrmCache.ProductsByCode[ProductCodes.C5DBServerREADER];

        #endregion

        #region Citavi 6 for DBServer CONCURRENT

        /// <summary>
        /// Citavi 6 for DBServer CONCURRENT
        /// </summary>
        public static Product C6DBServerCONCURRENT => CrmCache.ProductsByCode[ProductCodes.C6DBServerCONCURRENT];

        #endregion

        #region Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 4)
        /// </summary>
        public static Product C6DBServerCONCURRENTUpgradeC4 => CrmCache.ProductsByCode[ProductCodes.C6DBServerCONCURRENTUpgradeC4];

        #endregion

        #region Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 6 for DBServer CONCURRENT (Upgrade Citavi 5)
        /// </summary>
        public static Product C6DBServerCONCURRENTUpgradeC5 => CrmCache.ProductsByCode[ProductCodes.C6DBServerCONCURRENTUpgradeC5];

        #endregion

        #region Citavi 6 for DBServer PER SEAT

        /// <summary>
        /// Citavi 6 for DBServer PER SEAT
        /// </summary>
        public static Product C6DBServerPerSeat => CrmCache.ProductsByCode[ProductCodes.C6DBServerPerSeat];

        #endregion

        #region Citavi 6 for DBServer PER SEAT (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 6 for DBServer PER SEAT (Upgrade Citavi 4)
        /// </summary>
        public static Product C6DBServerPerSeatUpgradeC4 => CrmCache.ProductsByCode[ProductCodes.C6DBServerPerSeatUpgradeC4];

        #endregion

        #region Citavi 6 for DBServer PER SEAT (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 6 for DBServer PER SEAT (Upgrade Citavi 5)
        /// </summary>
        public static Product C6DBServerPerSeatUpgradeC5 => CrmCache.ProductsByCode[ProductCodes.C6DBServerPerSeatUpgradeC5];

        #endregion

        #region Citavi 6 for DBServer READER

        /// <summary>
        /// Citavi 6 for DBServer READER
        /// </summary>
        public static Product C6DBServerReader => CrmCache.ProductsByCode[ProductCodes.C6DBServerReader];

        #endregion

        #region Citavi 6 for Windows

        /// <summary>
        /// Citavi 6 for Windows
        /// </summary>
        public static Product C6Windows => CrmCache.ProductsByCode[ProductCodes.C6Windows];

        #endregion

        #region Citavi 6 for Windows (Upgrade Citavi 4)

        /// <summary>
        /// Citavi 6 for Windows (Upgrade Citavi 4)
        /// </summary>
        public static Product C6WindowsUpgradeC4 => CrmCache.ProductsByCode[ProductCodes.C6WindowsUpgradeC4];

        #endregion

        #region Citavi 6 for Windows (Upgrade Citavi 5)

        /// <summary>
        /// Citavi 6 for Windows (Upgrade Citavi 5)
        /// </summary>
        public static Product C6WindowsUpgradeC5 => CrmCache.ProductsByCode[ProductCodes.C6WindowsUpgradeC5];

        #endregion

        #region Citavi Web

        /// <summary>
        /// Citavi Web
        /// </summary>
        public static Product CitaviWeb => CrmCache.ProductsByCode[ProductCodes.CitaviWeb];

        #endregion

        #region Cloudspace

        /// <summary>
        /// Citavi Cloudspace
        /// </summary>
        public static Product CitaviSpace => CrmCache.ProductsByCode[ProductCodes.CitaviSpace];

        #endregion

        #region Citavi for DBServer PER SEAT Subscription

        /// <summary>
        /// Citavi for DBServer PER SEAT Subscription
        /// </summary>
        public static Product CitaviDBServerPERSEATSubscription => CrmCache.ProductsByCode[ProductCodes.CitaviDBServerPERSEATSubscription];

        #endregion

        #region Citavi for DBServer CONCURRENT Subscription

        /// <summary>
        /// Citavi for DBServer CONCURRENT Subscription
        /// </summary>
        public static Product CitaviDBServerCONCURRENTSubscription => CrmCache.ProductsByCode[ProductCodes.CitaviDBServerCONCURRENTSubscription];

        #endregion

        #region Citavi for DBServer READER Subscription

        /// <summary>
        /// Citavi for DBServer READER Subscription
        /// </summary>
        public static Product CitaviDBServerREADERSubscription => CrmCache.ProductsByCode[ProductCodes.CitaviDBServerREADERSubscription];

        #endregion

        #region Citavi for Windows (Subscription)

        /// <summary>
        /// Citavi Desktop & Citavi Web Subscription
        /// </summary>
        public static Product CitaviWindowsAndWeb => CrmCache.ProductsByCode[ProductCodes.CitaviWebAndWin];

        #endregion

        #region All

        static IEnumerable<Product> _all;
        internal static IEnumerable<Product> All => System.Threading.LazyInitializer.EnsureInitialized(ref _all, () =>
        {
            var list = new List<Product>();
            var type = typeof(ProductCodes);
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var prop in fieldInfos)
            {
                if(prop.IsLiteral && !prop.IsInitOnly)
                {
                    var productCode = (string)prop.GetRawConstantValue();
                    list.Add(CrmCache.ProductsByCode[productCode]);
                }
            }
            return list;
        });

        #endregion
        
        #endregion
    }
}
