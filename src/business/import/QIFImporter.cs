using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using business.import.processor;

namespace business.import
{
    public class QIFImporter : ImporterBase
	{
		HashAlgorithm _hash;

		public QIFImporter()
		{
			_hash = SHA1.Create();
		}

		public override TransactionsFile Import(string fileName, Stream stream, out List<ImportError> errors)
		{
			errors = new List<ImportError>();

			var file = new TransactionsFile()
			{
				FileName = fileName,
			};

			if(SkipFile(file))
			{
				errors.Add(new ImportError() { Error = "File skipped"});
				return null;
			}

            TransactionData transaction = null;
			int lineId = 0;
			Dictionary<string, List<TransactionData>> hashToTransactions = new Dictionary<string, List<TransactionData>>();

			using (StreamReader reader
				= new StreamReader(stream, Encoding.UTF8, true, 1024, true))
			{
				while(!reader.EndOfStream)
				{
					lineId++;
					string line = reader.ReadLine().TrimEnd(' ');

					if(transaction == null)
						transaction = new TransactionData();
						
					if (line.StartsWith("!"))
					{
						if (line != "!Type:Bank")
						{
							errors.Add(new ImportError{ Line = lineId, Error = "Type Bank expected"});
							return null;
						}
						else
							continue;
					}

					if (line.Length == 0)
						continue;

					char tag = line[0];
					string data = line.Substring(1);

					switch (tag)
					{
						case 'D':
							data = data.Replace('\'', '/');
							transaction.Date = DateTime.ParseExact(data, new string[] { "dd/MM/yy", "dd/MM/yyyy"}, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None);
							break;
						case 'T':
							transaction.Amount = decimal.Parse(data, NumberFormatInfo.InvariantInfo);
							break;
						case '$':
							transaction.Amount = decimal.Parse(data, NumberFormatInfo.InvariantInfo);
							break;
						case 'P':
							transaction.Caption = data;
							break;
						case 'N':
							transaction.Number = data;
							break;
						case 'L':
							if (data.StartsWith("["))
							{
								string targetaccountname = data.Trim('[', ']');
								transaction.TransferTarget = targetaccountname;
							}
							else
							{
								transaction.Category = data;
							}
							break;
						case 'S':
							{
								/*operationdetail = new ImportTransactionDetail();
								operationdetail.OwnerTransaction = operation;
								operation.Details.Add(operationdetail);

								if (data.StartsWith("["))
								{
									string targetaccountname = data.Trim('[', ']');
									operationdetail.ImportTargetTransferAccount = targetaccountname;
								}
								else
									operationdetail.ImportCategory = data;*/
								transaction.Error = "Splits not supported";
								errors.Add(new ImportError{ Line = lineId, Error = "Splits not supported"});
							}
							break;
						case '^':
							{
								SetTransactionHash(transaction, hashToTransactions);
								
								var processorResult = RunTransactionProcessors(file, transaction);

								errors.AddRange(processorResult.Errors);

								if(!processorResult.SkipTransaction)
									file.Transactions.Add(transaction);

								// FIN DE TRANSACTION
								transaction = null;
							}
							break;
						case 'M':
							{
								//champ memo
								transaction.Memo = data;
							}
							break;
						case 'E':
							{
								//champ memo (mode split)
								//operationdetail.ImportMemo = data;
								transaction.Error = "Splits not supported";
								errors.Add(new ImportError{ Line = lineId, Error = "Splits not supported"});
							}
							break;
						case 'C':
							{
								//champ à ignorer
							}
							break;
						default:
							{
								Console.WriteLine("UNKNOWN TAG [" + tag + "] : " + line);
							}
							break;
					}
				}
			}

			if(transaction != null)
			{
				SetTransactionHash(transaction, hashToTransactions);

				var processorResult = RunTransactionProcessors(file, transaction);

				errors.AddRange(processorResult.Errors);

				if(!processorResult.SkipTransaction)
					file.Transactions.Add(transaction);
			}

			return file;
		}

		private void SetTransactionHash(TransactionData transaction, Dictionary<string, List<TransactionData>> hashToTransactions)
		{
			var hash = ComputeHash(transaction);

			List<TransactionData> trxFromHash;

			if(hashToTransactions.ContainsKey(hash))
				trxFromHash = hashToTransactions[hash];
			else
			{
				trxFromHash = new List<TransactionData>();
				hashToTransactions.Add(hash, trxFromHash);
			}

			int hashIndex = trxFromHash.Count();

			transaction.Hash = $"{hash}-{hashIndex}";

			trxFromHash.Add(transaction);
		}

		private string ComputeHash(TransactionData data)
		{
            string transactionString = $"td:{data.Date}/cp:{data.Caption}/tt:{data.TransferTarget}/a:{data.Amount}/m:{data.Memo}/n:{data.Number}";
            var hash = _hash.ComputeHash(Encoding.UTF8.GetBytes(transactionString));
			return Convert.ToBase64String(hash);
        }
	}
}
