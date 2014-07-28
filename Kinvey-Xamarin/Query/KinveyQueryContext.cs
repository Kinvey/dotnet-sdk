/*
Copyright (c) 2007- 2010 LinqExtender Toolkit Project.

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included 
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ast = LinqExtender.Ast;
using LinqExtender;

namespace KinveyXamarin
{
	public class KinveyQueryContext<T> : ExpressionVisitor, IQueryContext<T>
	{
		public KinveyQueryContext(){
		}

		public KinveyQueryContext (IQueryBuilder writer)
		{
			this.writer = writer;
		}

		public IEnumerable<T> Execute(Ast.Expression expression)
		{
			writer.Write ("{");
			this.Visit(expression);
			writer.Write ("}");
			return new List<T>().AsEnumerable();


		}

		public override Ast.Expression VisitTypeExpression(Ast.TypeExpression expression)
		{
			//this one is a no-op because we don't support typing

//			writer.Write(string.Format("select * from {0}", expression.Type.Name));


			return expression;
		}

		public override Ast.Expression VisitLambdaExpression(Ast.LambdaExpression expression)
		{
//			WriteNewLine();
//			writer.Write("where");
//			WriteNewLine();

			this.Visit(expression.Body);

			return expression;
		}

		public override Ast.Expression VisitBinaryExpression(Ast.BinaryExpression expression)
		{
			this.Visit(expression.Left);
			writer.Write(GetBinaryOperator(expression.Operator));
			this.Visit(expression.Right);

			return expression;
		}

		public override Ast.Expression VisitLogicalExpression(Ast.LogicalExpression expression)
		{
			WriteTokenIfReq(expression, Token.LeftParenthesis);

			this.Visit(expression.Left);

			WriteLogicalOperator(expression.Operator);

			this.Visit(expression.Right);

			WriteTokenIfReq(expression, Token.RightParentThesis);

			return expression;
		}

		public override Ast.Expression VisitMemberExpression(Ast.MemberExpression expression)
		{
			writer.Write(String.Format("\"{0}\"", expression.FullName));
//			writer.Write(expression.FullName);
			return expression;
		}

		public override Ast.Expression VisitLiteralExpression(Ast.LiteralExpression expression)
		{
			WriteValue(expression.Type, expression.Value);
			return expression;
		}

		public override Ast.Expression VisitOrderbyExpression(Ast.OrderbyExpression expression)
		{
			WriteNewLine();
			Write(string.Format("order by {0}.{1} {2}", 
				expression.Member.DeclaringType.Name,
				expression.Member.Name, 
				expression.Ascending ? "asc" : "desc"));
			WriteNewLine();

			return expression;
		}

		private static string GetBinaryOperator(BinaryOperator @operator)
		{
			switch (@operator)
			{
			case BinaryOperator.Equal:
				return ":";
			}
			throw new ArgumentException("Invalid binary operator");
		}

		private void WriteLogicalOperator(LogicalOperator logicalOperator)
		{
			WriteSpace();

			writer.Write(logicalOperator.ToString().ToUpper());

			WriteSpace();
		}

		private void WriteSpace()
		{
			writer.Write(" ");
		}

		private void WriteNewLine()
		{
			writer.Write(Environment.NewLine);
		}

		private void WriteTokenIfReq(Ast.LogicalExpression expression, Token token)
		{
			if (expression.IsChild)
			{
				WriteToken(token);
			}
		}

		private void WriteToken(Token token)
		{
			switch (token)
			{
			case Token.LeftParenthesis:
				writer.Write("(");
				break;
			case Token.RightParentThesis:
				writer.Write(")");
				break;
			}
		}

		public enum Token
		{
			LeftParenthesis,
			RightParentThesis
		}

		private void WriteValue(TypeReference type, object value)
		{
			if (type.UnderlyingType == typeof(string))
				writer.Write(String.Format("\"{0}\"", value));
			else
				writer.Write(value);
		}

		private void Write(string value)
		{
			writer.Write(value);
		}

		public IQueryBuilder writer;
		public bool parameter;
	}
}
