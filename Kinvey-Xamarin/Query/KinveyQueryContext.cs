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

//throw not supported exception instead of just ignoring

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
			//TODO not sure need ?query here
			writer.Write ("?query={");
			this.Visit(expression);
			writer.Write ("}");
			return new List<T>().AsEnumerable();

			//TODO execution


		}

		public override Ast.Expression VisitTypeExpression(Ast.TypeExpression expression)
		{
			//this one is a no-op because we don't support typing, (one type per collection and no joins)

			//writer.Write(string.Format("select * from {0}", expression.Type.Name));
			return expression;
		}

		public override Ast.Expression VisitLambdaExpression(Ast.LambdaExpression expression)
		{
			//another no-op, `where` clause is implicit in Mongo

//			WriteNewLine();
//			writer.Write("where");
//			WriteNewLine();

			this.Visit(expression.Body);

			return expression;
		}

		public override Ast.Expression VisitBinaryExpression(Ast.BinaryExpression expression)
		{
			//TODO
			//gte, lte, lt, gt, eq, ne, contains are binary expressions
			this.Visit(expression.Left);
			writer.Write(GetBinaryOperator(expression.Operator));
			this.Visit(expression.Right);

			return expression;
		}

		public override Ast.Expression VisitLogicalExpression(Ast.LogicalExpression expression)
		{
			//TODO
			//And, Or are logical expressions

			if (expression.Operator == LogicalOperator.And) {

				//WriteTokenIfReq (expression, Token.LeftParenthesis);

				this.Visit (expression.Left);

				writer.Write (",");
//				WriteLogicalOperator (expression.Operator);

				this.Visit (expression.Right);

				//WriteTokenIfReq (expression, Token.RightParentThesis);

			} else {
				//it's an OR
				writer.Write ("$or:");
				writer.Write("[");
				writer.Write ("{");
				this.Visit (expression.Left);
				writer.Write ("},");
				writer.Write ("{");
				this.Visit (expression.Right);
				writer.Write ("}");
				writer.Write ("]");
			}

			return expression;
		}

		public override Ast.Expression VisitMemberExpression(Ast.MemberExpression expression)
		{


			//expression.FullName returns `MyClass.MyField`, need to remove `MyClass.`
			//so find the first index of the . and then substring from one character after it.
			string withoutClassDec = expression.FullName.Substring (expression.FullName.IndexOf (".") + 1);

			writer.Write(String.Format("\"{0}\"", withoutClassDec));
//			writer.Write(expression.FullName);
			return expression;
		}

		public override Ast.Expression VisitLiteralExpression(Ast.LiteralExpression expression)
		{
			//TODO literal expression is where type is implied by a constant's inline declaration -> string, "hey" or float, 10f

			WriteValue(expression.Type, expression.Value);
			return expression;
		}

		public override Ast.Expression VisitOrderbyExpression(Ast.OrderbyExpression expression)
		{

			//TODO
			//ascending, descending

			//writer.Dangle (string.Format("&sort={\"{0}\": {1}" + "}", expression.Member.Name, expression.Ascending ? "1" : "-1"));

//			expression.Ascending ? "1" : "-1";



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
