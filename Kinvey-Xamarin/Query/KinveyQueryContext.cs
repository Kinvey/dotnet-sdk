// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ast = LinqExtender.Ast;
using LinqExtender;
using Kinvey.DotNet.Framework;

namespace KinveyXamarin
{
	public abstract class KinveyQueryContext<T> : ExpressionVisitor, IQueryContext<T>
	{
		protected AbstractClient client;

		public KinveyQueryContext(AbstractClient client){
			this.client = client;
		}

		public KinveyQueryContext (AbstractClient client, IQueryBuilder writer)
		{
			this.writer = writer;
			this.client = client;
		}

		public IEnumerable<T> Execute(Ast.Expression expression)
		{
			writer.Write ("?query={");
			this.Visit(expression);
			writer.Write ("}");

			List<T> ret = executeQuery (writer.GetFullString());
			writer.Reset ();
			return ret;

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

			string sort = "&sort={\"" + expression.Member.Name + "\":" + (expression.Ascending ? "1" : "-1") + "}";
			writer.Dangle (sort);



//
//			WriteNewLine();
//			Write(string.Format("order by {0}.{1} {2}", 
//				expression.Member.DeclaringType.Name,
//				expression.Member.Name, 
//				expression.Ascending ? "asc" : "desc"));
//			WriteNewLine();

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

		protected abstract List<T> executeQuery(string query);
	}


}
