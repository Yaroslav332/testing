﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;


namespace HomeExercises
{
	public class NumberValidatorTests
	{
		private static object[][] _validTestObjects =
		{
			new object[]{17, 2, true, "0.0"},
			new object[]{17, 2, true, "0"},
			new object[]{17, 2, false, "-0"},
			new object[]{1, 0, true, "0"},
			new object[]{2, 0, false, "-0"},
			new object[]{4, 2, true, "+1.23"},
			new object[]{4, 2, false, "-1.23"},
		};

		private static object[][] _invalidTestObjects =
		{
			new object[]{3, 2, true, "00.00"},
			new object[]{1, 0, true, "-0"},
			new object[]{3, 0, true, "00."},
			new object[]{3, 2, true, "-0.00"},
			new object[]{3, 2, true, "+0.00"},
			new object[]{3, 2, true, "+1.23"},
			new object[]{17, 2, true, "00.000"},
			new object[]{3, 2, true, "-1.23"},
			new object[]{3, 2, true, "a.sd"},
			new object[]{3, 2, true, "a.00"},
			new object[]{3, 2, true, "0.sd"},
		};
		
		[Test, TestCaseSource(nameof(_validTestObjects))]
		public void IsValidNumber_WithRightArgs_IsTrue(int prec, int scale, bool pos, string num)
		{
			Assert.IsTrue(new NumberValidator(prec, scale, pos).IsValidNumber(num));
		}
		
		[Test, TestCaseSource(nameof(_invalidTestObjects))]
		public void IsValidNumber_WithWrongArgs_IsFalse(int prec, int scale, bool pos, string num)
		{
			Assert.IsFalse(new NumberValidator(prec, scale, pos).IsValidNumber(num));
		}
		[Test]
		public void When_NegativePrecision_ThrowsException()
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, true), "Failed with onlyPositive = true");
			Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, false), "Failed with onlyPositive = false");
		}

		[Test]
		public void When_ZeroScale_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => new NumberValidator(1, 0, true), "Failed with onlyPositive = true");
			Assert.DoesNotThrow(() => new NumberValidator(1, 0, false), "Failed with onlyPositive = false");
		}

	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}