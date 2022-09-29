﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Linq;

namespace Benchmarks;

static class DataExtensions
{
	public static void ProcessStrings(this IDataReader reader)
	{
		while (reader.Read())
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				reader.GetString(i);
			}
		}
	}

	public static void ProcessValues(this IDataReader reader)
	{
		while (reader.Read())
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				reader.GetValue(i);
			}
		}
	}

	public static void Process(this IDataReader reader)
	{
		TypeCode[] types = new TypeCode[reader.FieldCount];
		for (int i = 0; i < reader.FieldCount; i++)
		{
			var t = reader.GetFieldType(i);
			t = Nullable.GetUnderlyingType(t) ?? t;
			types[i] = Type.GetTypeCode(t);
		}

		while (reader.Read())
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				if (reader.IsDBNull(i))
					continue;
				ProcessField(reader, i, types[i]);
			}
		}
	}

	public static void Process(this DbDataReader reader)
	{
		var cols = reader.GetColumnSchema();
		bool[] allowDbNull = cols.Select(c => c.AllowDBNull != false).ToArray();
		TypeCode[] types = cols.Select(c =>
		{
			var type = c.DataType;
			type = Nullable.GetUnderlyingType(type) ?? type;
			return Type.GetTypeCode(c.DataType);
		}).ToArray();
		while (reader.Read())
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				if (allowDbNull[i] && reader.IsDBNull(i))
					continue;
				ProcessField(reader, i, types[i]);
			}
		}
	}

	public static async Task ProcessAsync(this DbDataReader reader)
	{
		var cols = reader.GetColumnSchema();
		bool[] allowDbNull = cols.Select(c => c.AllowDBNull != false).ToArray();
		TypeCode[] types = cols.Select(c => Type.GetTypeCode(c.DataType)).ToArray();
		while (await reader.ReadAsync())
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				if (allowDbNull[i] && await reader.IsDBNullAsync(i))
					continue;

				ProcessField(reader, i, types[i]);
			}
		}
	}

	static void ProcessField(this IDataReader reader, int i, TypeCode typeCode)
	{
		switch (typeCode)
		{
			case TypeCode.Boolean:
				reader.GetBoolean(i);
				break;
			case TypeCode.Int32:
				reader.GetInt32(i);
				break;
			case TypeCode.Int64:
				reader.GetInt64(i);
				break;
			case TypeCode.DateTime:
				reader.GetDateTime(i);
				break;
			case TypeCode.Single:
				reader.GetFloat(i);
				break;
			case TypeCode.Double:
				reader.GetDouble(i);
				break;
			case TypeCode.Decimal:
				reader.GetDecimal(i);
				break;
			case TypeCode.String:
				reader.GetString(i);
				break;
			default:
				// no cheating
				throw new NotSupportedException("" + typeCode);
		}
	}

	public static IEnumerable<T> GetRecordsDapper<T>(this DbDataReader dr)
	{
		var binder = dr.GetRowParser<T>();
		while (dr.Read())
		{
			yield return binder(dr);
		}
	}

	public static void ProcessSalesRecord(this DbDataReader reader)
	{
		var region = reader.GetString(0);
		var country = reader.GetString(1);
		var type = reader.GetString(2);
		var channel = reader.GetString(3);
		var priority = reader.GetString(4);
		var orderDate = reader.GetDateTime(5);
		var id = reader.GetInt32(6);
		var shipDate = reader.GetDateTime(7);
		var unitsSold = reader.GetInt32(8);
		var unitPrice = reader.GetDouble(9);
		var unitCost = reader.GetDouble(10);
		var totalRevenue = reader.GetDouble(11);
		var totalCost = reader.GetDouble(12);
		var totalProfit = reader.GetDouble(13);
	}


	public static void ProcessSalesRecordEDR(this IDataReader reader)
	{
		var region = reader.GetString(0);
		var country = reader.GetString(1);
		var type = reader.GetString(2);
		var channel = reader.GetString(3);
		var priority = reader.GetString(4);
		var orderDate = reader.GetDateTime(5);
		// ExcelDataReader doesn't allow reading as integers
		var id = reader.GetDouble(6);
		var shipDate = reader.GetDateTime(7);
		var unitsSold = reader.GetDouble(8);
		var unitPrice = reader.GetDouble(9);
		var unitCost = reader.GetDouble(10);
		var totalRevenue = reader.GetDouble(11);
		var totalCost = reader.GetDouble(12);
		var totalProfit = reader.GetDouble(13);
	}
}
