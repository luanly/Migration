namespace SwissAcademic.Crm.Web.Cleverbridge
{


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    
    public partial class ProfitCalculationType
    {

        private decimal grossRevenueField;

        private decimal collectedVatField;

        private decimal netRevenueField;

        private decimal cbMarginPercentageField;

        private decimal cbMarginFixField;

        private decimal cbShippingFeeField;

        private bool cbShippingFeeFieldSpecified;

        private decimal cbMarketingFeeField;

        private bool cbMarketingFeeFieldSpecified;

        private decimal affiliateCommissionField;

        private bool affiliateCommissionFieldSpecified;

        private decimal yourNetProfitField;

        private decimal yourVatField;

        private decimal yourGrossProfitField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal GrossRevenue
        {
            get
            {
                return this.grossRevenueField;
            }
            set
            {
                this.grossRevenueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal CollectedVat
        {
            get
            {
                return this.collectedVatField;
            }
            set
            {
                this.collectedVatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal NetRevenue
        {
            get
            {
                return this.netRevenueField;
            }
            set
            {
                this.netRevenueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal CbMarginPercentage
        {
            get
            {
                return this.cbMarginPercentageField;
            }
            set
            {
                this.cbMarginPercentageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal CbMarginFix
        {
            get
            {
                return this.cbMarginFixField;
            }
            set
            {
                this.cbMarginFixField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal CbShippingFee
        {
            get
            {
                return this.cbShippingFeeField;
            }
            set
            {
                this.cbShippingFeeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CbShippingFeeSpecified
        {
            get
            {
                return this.cbShippingFeeFieldSpecified;
            }
            set
            {
                this.cbShippingFeeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal CbMarketingFee
        {
            get
            {
                return this.cbMarketingFeeField;
            }
            set
            {
                this.cbMarketingFeeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CbMarketingFeeSpecified
        {
            get
            {
                return this.cbMarketingFeeFieldSpecified;
            }
            set
            {
                this.cbMarketingFeeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal AffiliateCommission
        {
            get
            {
                return this.affiliateCommissionField;
            }
            set
            {
                this.affiliateCommissionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AffiliateCommissionSpecified
        {
            get
            {
                return this.affiliateCommissionFieldSpecified;
            }
            set
            {
                this.affiliateCommissionFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal YourNetProfit
        {
            get
            {
                return this.yourNetProfitField;
            }
            set
            {
                this.yourNetProfitField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal YourVat
        {
            get
            {
                return this.yourVatField;
            }
            set
            {
                this.yourVatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public decimal YourGrossProfit
        {
            get
            {
                return this.yourGrossProfitField;
            }
            set
            {
                this.yourGrossProfitField = value;
            }
        }
    }
}