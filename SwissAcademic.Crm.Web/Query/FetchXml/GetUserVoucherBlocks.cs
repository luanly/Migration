// ------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion: 16.0.0.0
//  
//     Änderungen an dieser Datei können fehlerhaftes Verhalten verursachen und gehen verloren, wenn
//     der Code neu generiert wird.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class GetUserVoucherBlocks : GetUserVoucherBlocksBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("<fetch distinct=\"true\" no-lock=\"true\" mapping=\"logical\">\r\n  <entity name=\"new_vou" +
                    "cherblock\">\r\n    <attribute name=\"createdon\" />\r\n    ");
            
            #line 4 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
 foreach(Enum item in EntityPropertySets.GetFullPropertySet(typeof(VoucherBlock)))
          {
            
            #line default
            #line hidden
            this.Write("              \r\n                    <attribute name=\"");
            
            #line 7 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(EntityNameResolver.ResolveAttributeName(CrmEntityNames.VoucherBlock, item.ToString())));
            
            #line default
            #line hidden
            this.Write("\" />\r\n                      ");
            
            #line 8 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
}
         
            
            #line default
            #line hidden
            this.Write("\r\n    <filter type=\"and\">\r\n        <condition attribute=\"contactid\" operator=\"eq\"" +
                    " value=\"");
            
            #line 12 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
Write(ContactId);
            
            #line default
            #line hidden
            this.Write("\" entityname=\"");
            
            #line 12 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FetchXmlConstants.ContactVoucherBlockAliasName));
            
            #line default
            #line hidden
            this.Write(@""" />
        <condition attribute=""new_tempdeact"" operator=""eq"" value=""0"" entityname=""new_voucherblock"" />
        <condition attribute=""new_showinlicensemanagement"" operator=""eq"" value=""1"" entityname=""new_voucherblock"" />
          <condition attribute=""statuscode"" operator=""eq"" value=""1""/>
        </filter>
      <link-entity name=""new_citaviproduct"" to=""new_citaviproductid"" from=""new_citaviproductid"" link-type=""outer"" alias=""citaviproduct_voucherblock.new_citaviproduct"">
        <attribute name=""new_key"" />
        <attribute name=""new_citaviproductcode"" />
        <attribute name=""new_citaviproductname"" />
        <attribute name=""statuscode"" />
        <attribute name=""new_citaviproductid"" />
      </link-entity>
      <link-entity name=""new_pricing"" to=""new_pricingid"" from=""new_pricingid"" link-type=""outer"" alias=""");
            
            #line 24 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FetchXmlConstants.PricingVoucherBlockAliasName));
            
            #line default
            #line hidden
            this.Write("\">\r\n      <attribute name=\"new_key\" />\r\n      <attribute name=\"new_pricingcode\" /" +
                    ">\r\n    </link-entity>\r\n                        \r\n    <link-entity name=\"contact\"" +
                    " to=\"new_contactid\" from=\"contactid\" link-type=\"outer\" alias=\"");
            
            #line 29 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FetchXmlConstants.ContactVoucherBlockAliasName));
            
            #line default
            #line hidden
            this.Write("\">\r\n      <attribute name=\"new_key\" />\r\n\t    <attribute name=\"fullname\" />\r\n    <" +
                    "/link-entity>\r\n                        \r\n    <link-entity name=\"new_licensetyp\" " +
                    "to=\"new_licensetypid\" from=\"new_licensetypid\" link-type=\"outer\" alias=\"");
            
            #line 34 "C:\Git\Citavi6Dev\SwissAcademic.Crm.Web2\Query\FetchXml\GetUserVoucherBlocks.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FetchXmlConstants.LicenseTypeVoucherBlockAliasName));
            
            #line default
            #line hidden
            this.Write("\">\r\n      <attribute name=\"new_key\" />\r\n      <attribute name=\"new_licensecode\" /" +
                    ">\r\n    </link-entity>\r\n    <link-entity name=\"account\" to=\"new_accountid\" from=\"" +
                    "accountid\" link-type=\"outer\" alias=\"account_voucherblock.account\">\r\n        <att" +
                    "ribute name=\"accountid\" />\r\n        <attribute name=\"new_key\" />\r\n        <attri" +
                    "bute name=\"name\" />\r\n        <link-entity name=\"new_campuscontract\" to=\"accounti" +
                    "d\" from=\"new_accountid\" link-type=\"outer\" alias=\"account_campuscontract.new_camp" +
                    "uscontract\">\r\n          <attribute name=\"new_key\" />\r\n          <attribute name=" +
                    "\"new_contractduration\" />\r\n          <attribute name=\"new_contracttype\" />\r\n    " +
                    "      <attribute name=\"new_contractnumber\" />\r\n          <attribute name=\"new_in" +
                    "fowebsite\" />\r\n          <attribute name=\"new_voucherorderurl\" />\r\n          <at" +
                    "tribute name=\"new_orderurl\" />\r\n          <attribute name=\"statuscode\" />\r\n     " +
                    "     <attribute name=\"new_campuscontractid\" />\r\n          <filter type=\"and\">\r\n " +
                    "             <condition attribute=\"new_newcontractavailable\" operator=\"eq\" value" +
                    "=\"0\"/>\r\n              <condition attribute=\"statuscode\" operator=\"eq\" value=\"1\" " +
                    "/>\r\n          </filter>\r\n\t\t\t\t\t<link-entity name=\"new_campuscontract_n_citaviprod" +
                    "uct\" to=\"new_campuscontractid\" from=\"new_campuscontractid\" link-type=\"outer\" ali" +
                    "as=\"new_campuscontract_n_citaviproduct.new_citaviproduct\">\r\n\t\t\t\t\t\t<attribute nam" +
                    "e=\"new_citaviproductid\" />\r\n\t\t\t\t\t</link-entity>\r\n        </link-entity>\r\n       " +
                    " <link-entity name=\"new_emaildomainorcampusname\" to=\"accountid\" from=\"new_accoun" +
                    "tid\" link-type=\"outer\" alias=\"account_emaildomainorcampusname.new_emaildomainorc" +
                    "ampusname\">\r\n          <attribute name=\"new_key\" />\r\n          <attribute name=\"" +
                    "new_email_domain_or_campus_name\" />\r\n          <attribute name=\"new_organization" +
                    "nameforordermails\" />\r\n          <attribute name=\"statuscode\" />\r\n          <att" +
                    "ribute name=\"new_emaildomainorcampusnameid\" />\r\n        </link-entity>\r\n      </" +
                    "link-entity>\r\n  </entity>\r\n</fetch>");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public class GetUserVoucherBlocksBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
