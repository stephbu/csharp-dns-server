// //------------------------------------------------------------------------------------------------- 
// // <copyright file="CsvParser.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace Dns.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>Parses CSV files</summary>
    public class CsvParser
    {
        private static readonly char[] CSVDELIMITER = new[] {','};
        private static readonly char[] COLONDELIMITER = new[] {':'};

        private readonly string _filePath;

        private string _currentLine;
        private string[] _fields;

        private CsvParser()
        {
        }

        private CsvParser(string filePath)
        {
            this._filePath = filePath;
        }

        /// <summary>List of fields detected in CSV file</summary>
        public IEnumerable<string> Fields
        {
            get { return this._fields; }
        }

        /// <summary>
        ///   Returns enumerable collection of rows
        /// </summary>
        public IEnumerable<CsvRow> Rows
        {
            get
            {
                using (FileStream stream = new FileStream(this._filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (StreamReader csvReader = new StreamReader(stream))
                while (true)
                {
                    if (csvReader.Peek() < 0)
                    {
                        yield break;
                    }

                    this._currentLine = csvReader.ReadLine();
                    if (this._currentLine == null)
                    {
                        yield break;
                    }
                    if(this._currentLine.Trim() == string.Empty)
                    {
                        continue;
                    }
                    if ("#;".Contains(this._currentLine[0]))
                    {
                        // is a comment
                        if (this._currentLine.Length > 1 && this._currentLine.Substring(1).StartsWith("Fields"))
                        {
                            string[] fieldDeclaration = this._currentLine.Split(COLONDELIMITER);
                            if (fieldDeclaration.Length != 2)
                            {
                                this._fields = null;
                            }
                            else
                            {
                                this._fields = fieldDeclaration[1].Trim().Split(CSVDELIMITER);
                            }
                        }
                    }
                    else
                    {
                        yield return new CsvRow(this._fields, this._currentLine.Split(CSVDELIMITER));
                    }
                }
            }
        }

        /// <summary>
        ///   Create instance of CSV Parser
        /// </summary>
        /// <param name="filePath"> Path of file to parse </param>
        /// <returns> CSV Parser instance </returns>
        public static CsvParser Create(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File Not Found", filePath);
            }

            CsvParser result = new CsvParser(filePath);
            return result;
        }
    }
}