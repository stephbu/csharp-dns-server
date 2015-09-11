// //------------------------------------------------------------------------------------------------- 
// // <copyright file="CsvRow.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace Dns.Utility
{
    using System.Collections.Generic;

    /// <summary>Represents row in comma separated value file</summary>
    public class CsvRow
    {
        private readonly string[] _fieldValues;
        private readonly Dictionary<string, string> _fieldsByName = new Dictionary<string, string>();

        internal CsvRow(string[] fields, string[] fieldValues)
        {
            this._fieldValues = fieldValues;
            if ((fields != null) && (fields.Length == fieldValues.Length))
            {
                
                for(int index = 0; index < fields.Length; index++)
                {
                    this._fieldsByName[fields[index]] = fieldValues[index];
                }
            }
        }

        /// <summary>Returns value for specified field ordinal</summary>
        /// <param name="index">Specifed field ordinal</param>
        /// <returns>Value of field</returns>
        public string this[int index]
        {
            get { return this._fieldValues[index]; }
        }

        /// <summary>Returns value for specified field name</summary>
        /// <param name="name">Specified field name</param>
        /// <returns>Value of field</returns>
        public string this[string name]
        {
            get
            {
                string fieldValue;
                if (this._fieldsByName.TryGetValue(name, out fieldValue))
                {
                    return fieldValue;
                }
                return null;
            }
        }
    }
}