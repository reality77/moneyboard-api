using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace business.import
{
    public class OFXImporter : ImporterBase
	{
        private enum OFXTransactionType
        {
            None,

            CREDIT,
            DEBIT,
            INT,
            DIV,
            FEE,
            SRVCHG,
            DEP,
            ATM,
            POS,
            XFER,
            CHECK,
            PAYMENT,
            CASH,
            DIRECTDEP,
            DIRECTDEBIT,
            REPEATPMT,
            OTHER,
        }


        public OFXImporter()
		{
		}

        public override TransactionsFileImportResult Import(string fileName, Stream stream)
		{
            throw new NotImplementedException("OFX is not supported currently");
            /* 
            string[] contents = filecontent.Split('\n');

            ImportTransaction operation = null;
            //ImportTransactionDetail operationdetail = null;
            ImportAccount account = new ImportAccount();

            string encoding = "";
            string charset = "";
            int lineIndex = 0;

            string currency = "";

            DateTime? dateStart = null;
            DateTime? dateEnd= null;

            OFXTransactionType transactionType = OFXTransactionType.None;

            bool bInTransactionResponse = false;
            bool bInTransactionList = false;
            bool bInTransaction = false;
            bool bInAccountInfo = false;
            bool bInLedgerBal = false;
            bool bInAvailBal = false;

            foreach (string sline in contents)
            {
                lineIndex++;

                string line = sline.TrimEnd(' ', '\r');

                if (line.Length == 0)
                    continue;

                // --- TODO
                //OFXHEADER:100
                //DATA:OFXSGML
                //VERSION:102
                //SECURITY:NONE
                //ENCODING:USASCII
                //CHARSET:1252
                //COMPRESSION:NONE
                //OLDFILEUID:NONE
                //NEWFILEUID:NONE
                // ---

                if (line.StartsWith("ENCODING:"))
                {
                    encoding = line.Substring(("ENCODING:").Length);
                    continue;
                }
                else if (line.StartsWith("CHARSET:"))
                {
                    charset = line.Substring(("CHARSET:").Length);
                    continue;
                }
                else if (!line.StartsWith("<"))
                {
                    Debug.WriteLine("Not processed : " + line);
                    continue;
                }

                int indexEndTag = line.IndexOf(">");
                int indexStartTag = 0;

                if (indexEndTag <= 0)
                {
                    Debug.WriteLine("Malformed OFXSGML : " + line);
                    return null;
                }

                bool bIsEndTag = false;

                char c = line[1];

                string data = line.Substring(indexEndTag + 1);

                if (c == '/')
                {
                    bIsEndTag = true;
                    indexStartTag++;
                    indexEndTag--;
                }

                string tag = line.Substring(indexStartTag + 1, indexEndTag - 1).ToUpper();

                if (!bIsEndTag)
                {
                    switch (tag)
                    {
                        // --------------------- Response and AccountInfo

                        case "STMTTRNRS":
                            if (bInTransactionResponse)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInTransactionResponse = true;
                            break;
                        case "CURDEF":
                            if (!bInTransactionResponse)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            currency = data;
                            break;
                        case "BANKACCTFROM":
                            if (!bInTransactionResponse)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInAccountInfo = true;

                            account = new ImportAccount();
                            account.AccountName = "";
                            break;
                        case "ACCTID":
                            if (!bInAccountInfo)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            account.AccountNumber = data;
                            account.OriginalAccountName = data;
                            break;

                        // --------------------- Transaction List
                        case "BANKTRANLIST":
                            if (bInTransactionList)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInTransactionList = true;
                            break;
                        case "DTSTART":
                            if (!bInTransactionList)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            dateStart = DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);
                            break;
                        case "DTEND":
                            if (!bInTransactionList)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            dateEnd = DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);
                            break;

                        // ------------------ Transaction
                        case "STMTTRN":
                            if (!bInTransactionList || bInTransaction)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInTransaction = true;

                            operation = new ImportTransaction();
                            operation.OwnerAccount = account;
                            account.Transactions.Add(operation);
                            transactionType = OFXTransactionType.None;

                            break;
                        case "TRNTYPE":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                transactionType = (OFXTransactionType)Enum.Parse(typeof(OFXTransactionType), data, true);
                            }
                            break;
                        case "DTPOSTED":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                DateTime date = DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);
                                operation.PostedDate = date;
                            }
                            break;
                        case "DTUSER":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                DateTime date = DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);
                                operation.UserDate = date;
                            }
                            break;
                        case "DTAVAIL":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                DateTime date = DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);
                                operation.AvailableDate = date;
                            }
                            break;
                        case "TRNAMT":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                char sign = data[0];

                                if (sign == '+' || sign == '-')
                                    data = data.Substring(1);

                                //TODO : tester culture correcte !!!
                                //decimal amount = decimal.Parse(data, CultureInfo.InvariantCulture);
                                decimal amount = decimal.Parse(data, CultureInfo.CurrentCulture);

                                if (sign == '-')
                                    amount = 0 - amount;

                                operation.Amount = amount;
                            }
                            break;
                        case "FITID":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                operation.ImportID = data;
                            }
                            break;
                        case "CHECKNUM":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                operation.CheckNumber = data;
                            }
                            break;
                        case "REFNUM":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                if (operation.CheckNumber.Length == 0)
                                    operation.CheckNumber = data;
                            }
                            break;
                        case "NAME":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                operation.ImportName = data;
                            }
                            break;
                        case "PAYEE":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                if (operation.ImportName.Length == 0)
                                    operation.ImportName = data;
                            }
                            break;
                        case "MEMO":
                            {
                                if (!bInTransaction)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                operation.ImportMemo = data;
                            }
                            break;

                        // ------------------ Ledger/Available balance
                        case "LEDGERBAL":
                            {
                                if (!bInTransactionResponse)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                bInLedgerBal = true;
                            }
                            break;
                        case "AVAILBAL":
                            {
                                if (!bInTransactionResponse)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                bInAvailBal = true;
                            }
                            break;
                        case "BALAMT":
                            {
                                if (!bInLedgerBal && !bInAvailBal)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                char sign = data[0];

                                if (sign == '+' || sign == '-')
                                    data = data.Substring(1);

                                //TODO : tester culture correcte !!!
                                //decimal amount = decimal.Parse(data, CultureInfo.InvariantCulture);
                                decimal amount = decimal.Parse(data, CultureInfo.CurrentCulture);

                                if (sign == '-')
                                    amount = 0 - amount;

                                if(bInLedgerBal)
                                    account.LedgerBalanceAmount = amount;
                                else if(bInAvailBal)
                                    account.AvailableBalanceAmount = amount;
                            }
                            break;
                        case "DTASOF":
                            {
                                if (!bInLedgerBal && !bInAvailBal)
                                    throw new OFXImporterException(lineIndex, "Bad location", tag);

                                DateTime date = DateTime.ParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture);

                                if (bInLedgerBal)
                                    account.LedgerBalanceDate = date;
                                else if (bInAvailBal)
                                    account.AvailableBalanceDate = date;
                            }
                            break;
                    }
                }
                else
                {
                    switch (tag)
                    {
                        case "STMTTRNRS":
                            if (!bInTransactionResponse)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInTransactionResponse = false;
                            break;
                        case "BANKACCTFROM":
                            if (!bInAccountInfo)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInAccountInfo = false;
                            break;
                        case "BANKTRANLIST":
                            if (!bInTransactionList)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInTransactionList = false;
                            break;
                        case "STMTTRN":
                            if (!bInTransaction)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInTransaction = false;
                            break;
                        case "LEDGERBAL":
                            if (!bInLedgerBal)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInLedgerBal = false;
                            break;
                        case "AVAILBAL":
                            if (!bInAvailBal)
                                throw new OFXImporterException(lineIndex, "Bad location", tag);
                            bInAvailBal = false;
                            break;
                    }
                }
            }

            account.Currency = currency;

			return account;
            */
		}
	}
}
