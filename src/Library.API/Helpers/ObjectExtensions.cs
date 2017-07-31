﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Library.API.Helpers
{
	public static class ObjectExtensions
	{
		public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			var dataShapedObject = new ExpandoObject();
			var propertyInfoList = new List<PropertyInfo>();
			if (string.IsNullOrWhiteSpace(fields))
			{
				propertyInfoList.AddRange(typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));
			}
			else
			{
				var fieldsAfterSplit = fields.Split(',');
				foreach (var field in fieldsAfterSplit)
				{
					var propertyName = field.Trim();
					var propertyInfos = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					if (propertyInfos == null)
						throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
					propertyInfoList.Add(propertyInfos);

				}
			}
			foreach (var propertyInfo in propertyInfoList)
			{
				var propertyValue = propertyInfo.GetValue(source);
				((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
			}
			return dataShapedObject;
		}
	}
}
