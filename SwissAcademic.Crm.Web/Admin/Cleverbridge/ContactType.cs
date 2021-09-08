using System;
using System.Xml.Serialization;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    [Serializable]
    
    public partial class ContactType
    {
        #region Felder

        private string languageField;

        private LanguageIdType languageIdField;

        private LocaleType localeField;

        private bool localeFieldSpecified;

        private string salutationField;

        private SalutationIdType salutationIdField;

        private bool salutationIdFieldSpecified;

        private string titleField;

        private string companyField;

        private string companyTypeField;

        private string companyTypeIdField;

        private string firstnameField;

        private string lastnameField;

        private string companyKatakanaField;

        private string firstnameKatakanaField;

        private string lastnameKatakanaField;

        private string companyRomanizedField;

        private string firstnameRomanizedField;

        private string lastnameRomanizedField;

        private string street1Field;

        private string street2Field;

        private string postalCodeField;

        private string cityField;

        private string stateField;

        private StateIdType stateIdField;

        private bool stateIdFieldSpecified;

        private string postLineField;

        private string countryField;

        private CountryIdType countryIdField;

        private string phone1Field;

        private string phone2Field;

        private string faxField;

        private string emailField;

        private string vatIdField;

        private string urlField;

        #endregion

        #region Eigenschaften

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Language
        {
            get
            {
                return languageField;
            }
            set
            {
                languageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public LanguageIdType LanguageId
        {
            get
            {
                return languageIdField;
            }
            set
            {
                languageIdField = value;
            }
        }

        public LanguageType? LanguageResolved
        {
            get
            {
                switch (LanguageId)
                {
                    case LanguageIdType.fr:
                        return LanguageType.French;

                    case LanguageIdType.en:
                        return LanguageType.English;

                    case LanguageIdType.de:
                        return LanguageType.German;

                    case LanguageIdType.it:
                        return LanguageType.Italian;

                    case LanguageIdType.es:
                        return LanguageType.Spanish;

                    case LanguageIdType.pt:
                        return LanguageType.Portuguese;

                    case LanguageIdType.pl:
                        return LanguageType.Polish;
                }

                return null;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public LocaleType Locale
        {
            get
            {
                return localeField;
            }
            set
            {
                localeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LocaleSpecified
        {
            get
            {
                return localeFieldSpecified;
            }
            set
            {
                localeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Salutation
        {
            get
            {
                return salutationField;
            }
            set
            {
                salutationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public SalutationIdType SalutationId
        {
            get
            {
                return salutationIdField;
            }
            set
            {
                salutationIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SalutationIdSpecified
        {
            get
            {
                return salutationIdFieldSpecified;
            }
            set
            {
                salutationIdFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Title
        {
            get
            {
                return titleField;
            }
            set
            {
                titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Company
        {
            get
            {
                return companyField;
            }
            set
            {
                companyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CompanyType
        {
            get
            {
                return companyTypeField;
            }
            set
            {
                companyTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CompanyTypeId
        {
            get
            {
                return companyTypeIdField;
            }
            set
            {
                companyTypeIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Firstname
        {
            get
            {
                return firstnameField;
            }
            set
            {
                firstnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Lastname
        {
            get
            {
                return lastnameField;
            }
            set
            {
                lastnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CompanyKatakana
        {
            get
            {
                return companyKatakanaField;
            }
            set
            {
                companyKatakanaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string FirstnameKatakana
        {
            get
            {
                return firstnameKatakanaField;
            }
            set
            {
                firstnameKatakanaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string LastnameKatakana
        {
            get
            {
                return lastnameKatakanaField;
            }
            set
            {
                lastnameKatakanaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string CompanyRomanized
        {
            get
            {
                return companyRomanizedField;
            }
            set
            {
                companyRomanizedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string FirstnameRomanized
        {
            get
            {
                return firstnameRomanizedField;
            }
            set
            {
                firstnameRomanizedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string LastnameRomanized
        {
            get
            {
                return lastnameRomanizedField;
            }
            set
            {
                lastnameRomanizedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Street1
        {
            get
            {
                return street1Field;
            }
            set
            {
                street1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Street2
        {
            get
            {
                return street2Field;
            }
            set
            {
                street2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string PostalCode
        {
            get
            {
                return postalCodeField;
            }
            set
            {
                postalCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string City
        {
            get
            {
                return cityField;
            }
            set
            {
                cityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string State
        {
            get
            {
                return stateField;
            }
            set
            {
                stateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public StateIdType StateId
        {
            get
            {
                return stateIdField;
            }
            set
            {
                stateIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StateIdSpecified
        {
            get
            {
                return stateIdFieldSpecified;
            }
            set
            {
                stateIdFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string PostLine
        {
            get
            {
                return postLineField;
            }
            set
            {
                postLineField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Country
        {
            get
            {
                return countryField;
            }
            set
            {
                countryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public CountryIdType CountryId
        {
            get
            {
                return countryIdField;
            }
            set
            {
                countryIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Phone1
        {
            get
            {
                return phone1Field;
            }
            set
            {
                phone1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Phone2
        {
            get
            {
                return phone2Field;
            }
            set
            {
                phone2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Fax
        {
            get
            {
                return faxField;
            }
            set
            {
                faxField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Email
        {
            get
            {
                return emailField;
            }
            set
            {
                emailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string VatId
        {
            get
            {
                return vatIdField;
            }
            set
            {
                vatIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string Url
        {
            get
            {
                return urlField;
            }
            set
            {
                urlField = value;
            }
        }

        #endregion

        #region Methoden

        #region Equals

        public bool Equals(ContactType other)
        {
            if (other == null)
            {
                return false;
            }

            if (string.Equals(Email, other.Email, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (string.Equals(Lastname == other.Lastname, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(Firstname == other.Firstname, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        #endregion

        #endregion
    }
}
