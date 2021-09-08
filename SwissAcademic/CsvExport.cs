using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace SwissAcademic
{
    //Source: http://stackoverflow.com/questions/2422212/simple-c-sharp-csv-excel-export-class
    //TODO JHP Consider Alternative Infragistcs Excel Exporter
    //http://www.infragistics.com/products/aspnet/excel-exporter/

    /// <summary>
    /// Simple CSV export
    /// Example:
    ///   CsvExport myExport = new CsvExport();
    ///
    ///   myExport.AddRow();
    ///   myExport["Region"] = "New York, USA";
    ///   myExport["Sales"] = 100000;
    ///   myExport["Date Opened"] = new DateTime(2003, 12, 31);
    ///
    ///   myExport.AddRow();
    ///   myExport["Region"] = "Sydney \"in\" Australia";
    ///   myExport["Sales"] = 50000;
    ///   myExport["Date Opened"] = new DateTime(2005, 1, 1, 9, 30, 0);
    ///
    /// Then you can do any of the following three output options:
    ///   string myCsv = myExport.Export();
    ///   myExport.ExportToFile("Somefile.csv");
    ///   byte[] myCsvData = myExport.ExportToBytes();
    /// </summary>
    public class CsvExport
    {

        #region Properties

        #region CurrentRow

        /// <summary>
        /// The current row
        /// </summary>
        Dictionary<string, object> CurrentRow
        {
            get
            {
                return Rows[Rows.Count - 1];
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// To keep the ordered list of column names
        /// </summary>
        List<string> _fields = new List<string>();

        public IEnumerable<string> Fields
        {
            get
            {
                foreach (var field in _fields)
                {
                    yield return field;
                }
            }
        }

        #endregion

        #region Rows

        /// <summary>
        /// The list of rows
        /// </summary>
        public List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>();

        #endregion

        #region Separator

        string _separator = ",";
        public string Separator
        {
            get
            {
                return _separator;
            }
            set
            {
                _separator = value;
            }
        }

        #endregion

        #region This[field]

        /// <summary>
        /// Set a value on this column
        /// </summary>
        public object this[string field]
        {
            set
            {
                // Keep track of the field names, because the dictionary loses the ordering
                if (!_fields.Contains(field)) _fields.Add(field);
                CurrentRow[field] = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region AddRow

        /// <summary>
        /// Call this before setting any fields on a row
        /// </summary>
        public void AddRow()
        {
            Rows.Add(new Dictionary<string, object>());
        }

        #endregion

        #region Export

        /// <summary>
        /// Output all rows as a CSV returning a string
        /// </summary>
        public string Export()
        {
            StringBuilder sb = new StringBuilder();


            // The header
            int counterFields = 0;
            bool lastField = false;

            foreach (string field in _fields)
            {
                counterFields++;
                lastField = counterFields == _fields.Count;

                string columnHeader = GetColumnHeader(field); //more describing or localized column header
                if (string.IsNullOrEmpty(columnHeader)) columnHeader = field;

                if (lastField)
                {
                    sb.Append(columnHeader);
                }
                else
                {
                    sb.Append(columnHeader).Append(Separator);
                }
            }



            // The rows
            int counterRows = 0;
            foreach (Dictionary<string, object> row in Rows)
            {
                if (row == null || row.Count == 0) continue;
                sb.AppendLine();

                counterRows++;
                counterFields = 0;
                lastField = false;

                foreach (string field in _fields)
                {
                    counterFields++;
                    lastField = counterFields == _fields.Count;
                    
                    object fieldValue = null;
                    bool found = row.TryGetValue(field, out fieldValue);

                    if (lastField)
                    {
                        sb.Append(MakeValueCsvFriendly(fieldValue));
                    }
                    else
                    {
                        sb.Append(MakeValueCsvFriendly(fieldValue)).Append(Separator);
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

        #region ExportToBytes

        /// <summary>
        /// Exports as raw UTF8 bytes
        /// </summary>
        public byte[] ExportToBytes()
        {
            return Encoding.UTF8.GetBytes(Export());
        }

        #endregion

        #region ExportToFile

        /// <summary>
        /// Exports to a file
        /// </summary>
        public void ExportToFile(string path)
        {
            //File.WriteAllText(path, Export());

            File.WriteAllText(path, Export(), Encoding.UTF8);
        }

        #endregion

        #region ExportToFileUTF8

        public void ExportToFileUTF8(string path)
        {
            UTF8Encoding utf8Encoding = new UTF8Encoding();
            using (var streamWriter = new StreamWriter(path, true, utf8Encoding))
            {
                byte[] export = ExportToBytes();
                byte[] preamble = utf8Encoding.GetPreamble();

                streamWriter.Write(preamble);
                streamWriter.Write(export);
            }
        }

        #endregion

        #region GetColumnHeader

        Dictionary<string, string> _columnHeaders = new Dictionary<string, string>();

        public string GetColumnHeader(string columnId)
        {
            if (string.IsNullOrWhiteSpace(columnId)) return null;
            
            string columnHeader;
            if (_columnHeaders.TryGetValue(columnId, out columnHeader))
            {
                return columnHeader;
            }

            //check if columnId is a field at all
            if (_fields.Contains(columnId))
            {
                //field exists, but no column header defined
                //add it using same string as columnId
                _columnHeaders[columnId] = columnId;
                return columnId;
            }
            else
            {
                //no such field
                return null;
            }
        }

        #endregion

        #region SetColumnHeader

        public void SetColumnHeader(string columnId, string columnHeader)
        {
            if (string.IsNullOrWhiteSpace(columnId)) return;

            //make sure field exists
            if (!_fields.Contains(columnId)) return;

            _columnHeaders[columnId] = string.IsNullOrWhiteSpace(columnHeader) ? columnId : columnHeader;
        }

        #endregion

        #region MakeValueCsvFriendly

        /// <summary>
        /// Converts a value to how it should output in a csv file
        /// If it has a comma, it needs surrounding with double quotes
        /// Eg Sydney, Australia -> "Sydney, Australia"
        /// Also if it contains any double quotes ("), then they need to be replaced with quad quotes[sic] ("")
        /// Eg "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
        /// </summary>
        string MakeValueCsvFriendly(object value)
        {
            if (value == null) return "";
            if (value is INullable && ((INullable)value).IsNull) return "";
            if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0) return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }

            string output = value.ToString();

            //first we replace CRLF to LF only, otherwise Excel will not put multiline content into a single cell.
            output = output.Replace("\r\n", " ");   //CRLF  space = '\u0020'
            output = output.Replace("\r", " ");     //CR or char c = '\u000D'
            output = output.Replace("\n", " ");     //LF or char c = '\u000A'
            output.Trim();

            if (output.Contains(Separator) || output.Contains("\"") || output.Contains("\n") || output.Contains("\t"))
            {
                output = '"' + output.Replace("\"", "\"\"") + '"';
            }
            return output;
        }

        #endregion

        #endregion

    }
}
